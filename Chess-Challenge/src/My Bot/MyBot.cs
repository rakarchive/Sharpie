using System;
using System.Numerics;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        const int INF = int.MaxValue - 1;

        Move bestMove = Move.NullMove;
        int timeToUse = timer.MillisecondsRemaining / 20 + 75;
        for (int depth = 1; timer.MillisecondsElapsedThisTurn < timeToUse; depth++) {
            try {
                Search(0, depth, -INF, INF);
            } catch {
                break;
            }
        }

        int Search(int ply, int depth, int alpha, int beta, bool quiescence = false)
        {
            if (timer.MillisecondsElapsedThisTurn >= timeToUse)
                throw new TimeoutException();

            if (!quiescence && depth <= 0) return Search(ply, depth, alpha, beta, true);

            Move[] moves = board.GetLegalMoves(quiescence);
            if (!quiescence && moves.Length == 0) return board.IsInCheck() ? -INF + ply : 0;

            int bestEvaluation = -INF;
            if (quiescence) {
                bestEvaluation = Evaluate();
                if (bestEvaluation >= beta) return bestEvaluation;
                alpha = Math.Max(alpha, bestEvaluation);
            }
            
            Move currentBestMove = Move.NullMove;
            foreach (Move move in moves) {
                board.MakeMove(move);
                int evaluation = -Search(ply + 1, depth - 1, -beta, -alpha, quiescence);
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

        int Evaluate()
        {
            bool  stm = board.IsWhiteToMove;
            bool nstm = !stm               ;
            return Difference(PieceType.Pawn  ) +
                   Difference(PieceType.Knight) * 3 +
                   Difference(PieceType.Bishop) * 3 +
                   Difference(PieceType.Rook  ) * 5 +
                   Difference(PieceType.Queen ) * 9 ;

            int Difference(PieceType pieceType)
            {
                return BitOperations.PopCount(board.GetPieceBitboard(pieceType,  stm)) -
                       BitOperations.PopCount(board.GetPieceBitboard(pieceType, nstm));
            }
        }
        
        return bestMove;
    }
    
}