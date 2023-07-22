using System;
using System.Numerics;
using ChessChallenge.API;
using ChessChallenge.Application.APIHelpers;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        const int INF = int.MaxValue - 1;

        Move bestMove = Move.NullMove;
        int timeToUse = timer.MillisecondsRemaining / 20;
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
                
                if (evaluation > beta) break;
            }
            
            if (ply == 0) bestMove = currentBestMove;
            
            return bestEvaluation;
        }

        int Evaluate()
        {
            bool isWhite = board.IsWhiteToMove;
            bool isBlack = !isWhite           ;
            return Difference(PieceType.Pawn  ) +
                   Difference(PieceType.Knight) * 3 +
                   Difference(PieceType.Bishop) * 3 +
                   Difference(PieceType.Rook  ) * 5 +
                   Difference(PieceType.Queen ) * 9 ;

            int Difference(PieceType pieceType)
            {
                return BitOperations.PopCount(board.GetPieceBitboard(pieceType, isWhite)) -
                       BitOperations.PopCount(board.GetPieceBitboard(pieceType, isBlack));
            }
        }
        
        return bestMove;
    }
    
}