using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    
    public static readonly int[] pst =
    {
        100,  100,  100,  100,  105,  110,  110,   80,  100,  100,  100,  100,  100,  100,  100,  120, 
        100,  100,  100,  125,  120,  120,  125,  130,  150,  150,  150,  150,  100,  100,  100,  100, 
        250,  260,  270,  280,  260,  280,  290,  305,  270,  310,  315,  305,  280,  310,  310,  320, 
        280,  310,  310,  320,  270,  310,  310,  305,  260,  280,  290,  305,  250,  260,  270,  280, 
        280,  280,  280,  280,  300,  315,  310,  310,  300,  320,  320,  320,  300,  310,  320,  320, 
        300,  310,  320,  320,  300,  310,  320,  320,  300,  310,  310,  310,  290,  310,  310,  310, 
        480,  500,  505,  510,  485,  500,  500,  500,  490,  500,  500,  500,  490,  500,  500,  500, 
        490,  500,  500,  500,  490,  500,  500,  500,  520,  520,  520,  520,  500,  500,  500,  500, 
        900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900, 
        900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900,  900, 
         20,   30,  -30,    0,   10,   10,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40, 
        -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,  -40,
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
            int result = 0;
            for (int sq = 0; sq < 64; sq++)
            {
                Piece piece = board.GetPiece(new Square(sq));
                if (piece.PieceType == PieceType.None) continue;
                int index = GetPstIndex(piece.IsWhite, (int)piece.PieceType, sq);
                if (piece.IsWhite == board.IsWhiteToMove) result += pst[index];
                else result -= pst[index];
            }

            return result;
        }

        return bestMove;
    }
    
}