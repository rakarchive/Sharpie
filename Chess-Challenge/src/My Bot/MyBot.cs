using System;
using System.Numerics;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Searcher searcher = new Searcher(board, timer);
        return searcher.Search();
    }

    public class Searcher
    {
        private Board board;
        private Timer timer;

        private Move overallBestMove;
        private Int32 overallBestScore;
        
        const Int32 EVAL_INF = Int32.MaxValue - 1;

        public Searcher(Board b, Timer t)
        {
            board = b;
            timer = t;
        }

        public Move Search()
        {
            Negamax(0, 4, -EVAL_INF, EVAL_INF);
            return overallBestMove;
        }
        
        public Int32 Negamax(Int32 plys, Int32 depth, Int32 alpha, Int32 beta)
        {
            if (board.IsInCheckmate()) return -EVAL_INF + plys;
            if (board.IsDraw()) return 0;
            if (depth <= 0) return Evaluate();

            Int32 bestScore = -Int32.MaxValue + 1;
            Move bestMove = Move.NullMove;

            Move[] moves = board.GetLegalMoves();
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                Int32 score = -Negamax(plys + 1, depth - 1, -beta, -alpha);
                board.UndoMove(move);

                if (score > bestScore)
                {
                    bestScore = score;

                    if (score > alpha)
                    {
                        alpha = score;
                        bestMove = move;

                        if (plys == 0 && (score > overallBestScore || bestMove == Move.NullMove))
                        {
                            overallBestScore = score;
                            overallBestMove = bestMove;
                        }

                        if (score >= beta) break;
                    }
                }
            }
        
            return bestScore;
        }

        public Int32 Evaluate()
        {

            return BitOperations.PopCount(board.GetPieceBitboard(PieceType.Pawn, board.IsWhiteToMove))       +
                   BitOperations.PopCount(board.GetPieceBitboard(PieceType.Knight, board.IsWhiteToMove)) * 3 +
                   BitOperations.PopCount(board.GetPieceBitboard(PieceType.Bishop, board.IsWhiteToMove)) * 3 +
                   BitOperations.PopCount(board.GetPieceBitboard(PieceType.Rook, board.IsWhiteToMove))   * 5 +
                   BitOperations.PopCount(board.GetPieceBitboard(PieceType.Queen, board.IsWhiteToMove))  * 9;
        }
    }
}