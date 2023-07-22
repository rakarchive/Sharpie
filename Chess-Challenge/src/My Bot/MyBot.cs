using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    
    public static readonly Int64[] pst =
    {
        28147927174348900,    22518470590464105,    28147927174348900,    33777426708562020, 
        35184801592115300,    36592283851161720,    42221890761523350,    28147927174348900, 
        78814153137193210,    85851113455616260,    85851220831764750,    90073324007588120, 
        90073324007588120,    85851199356928270,    85851113455616260,    78814153137193210, 
        78814196088176920,    87258574240809260,    90073366957916460,    90073366957261100, 
        90073366957261100,    90073366957261100,    87258574240481580,    87258574240481570, 
        143554407113687520,   140739635871744485,   140739635871744490,   140739635871744490, 
        140739635871744490,   140739635871744490,   146369221306614280,   140739635871744500, 
        251923926735258480,   253331344569140090,   254738740927529850,   254738740927529855, 
        254738740927529855,   254738740927529850,   253331344569140090,   251923926735258480, 
        281479142896436220,   270220100877026290,   270220100873749440,   270220100873749440, 
        270220100873749440,   270220100873749440,   270220100873749440,   270220100873749440,  
    };
    
    public Move Think(Board board, Timer timer)
    {
        const int INF = int.MaxValue - 1;

        Move bestMove = Move.NullMove;
        int timeToUse = Math.Min(timer.MillisecondsRemaining / 25 + 70, timer.MillisecondsRemaining);
        for (int depth = 1; timer.MillisecondsElapsedThisTurn < timeToUse; depth++) {
            try {
                Negamax(0, depth, -INF, +INF);
            } catch {
                break;
            }
        }

        int Negamax(int ply, int depth, int alpha, int beta)
        {
            if (timer.MillisecondsElapsedThisTurn >= timeToUse)
                throw new TimeoutException();
            
            if (board.IsDraw()) return 0;
            if (depth <= 0) return Evaluate();

            Move[] moves = board.GetLegalMoves();
            if (moves.Length == 0) return board.IsInCheck() ? -INF + ply : 0;

            int  bestEvaluation  = -INF         ;
            Move currentBestMove = Move.NullMove;
            foreach (Move move in moves) {
                board.MakeMove(move);
                int evaluation = -Negamax(ply + 1, depth - 1, -beta, -alpha);
                board.UndoMove(move);
                
                if (evaluation <= bestEvaluation) continue;
                bestEvaluation  = evaluation;
                currentBestMove = move;
                
                if (evaluation <= alpha) continue;
                alpha = evaluation;
                
                if (evaluation >= beta) break;
            }
            
            if (ply == 0) bestMove = currentBestMove;
            
            return bestEvaluation;
        }
        
        int GetPstIndex(bool pc, int pt, int square)
        {
            if (pc == false) square ^= 56;
            if ((square & 4) != 0) square ^= 7;
            return (pt - 1) * 32 + (square & 3 | square >> 3 << 2);
        }

        int Evaluate()
        {
            Int64 result = 0;
            for (int sq = 0; sq < 64; sq++)
            {
                Piece piece = board.GetPiece(new Square(sq));
                if (piece.PieceType == PieceType.None) continue;
                int index = GetPstIndex(piece.IsWhite, (int)piece.PieceType, sq);
                Int64 value = (pst[index / 4] >> ((index & 3) * 16)) & 32767;
                if (piece.IsWhite == board.IsWhiteToMove) result += value;
                else result -= value;
            }
            return (int)result;
        }

        return bestMove;
    }
    
}