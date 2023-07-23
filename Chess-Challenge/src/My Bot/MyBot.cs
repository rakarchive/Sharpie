using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{

    private static readonly long[] pst =
    {
        28147927174348900,    23925905605394510,    28992364991545432,    32933027548889163, 
        34621903179415629,    46444041180348496,    47288766758781017,    28147927174348900, 
        79658586658636053,    84162383854829849,    89791879096631580,    92325162476699940, 
        94858652015264066,   108088233622307160,    90917972275822875,   101613338489585857, 
        83599348001079585,    89228989273932087,    91199339879727424,    96828774988906810, 
        97673307293024564,    97110413176144180,    95703076946051335,    80221519434547502, 
        145243265561461210,   140458208139280813,   141584060801548755,   143272880597762525, 
        147495185636131296,   145524903753351684,   163257921773109792,   158472598061253151, 
        249109112542266194,   255583208807138181,   251923978276438921,   252768377436046209, 
        253049942606742405,   261494359414997949,   248827989756740538,   269938557176447921, 
        283730951299662838,   269375779026043888,   269094243918218189,   268531233834075061, 
        273034915067069380,   275849797980521426,   279227454747444171,   265716527018935285,  
    };
    
    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        int timeToUse = timer.MillisecondsRemaining / 25 + 70;
        for (int depth = 1; timer.MillisecondsElapsedThisTurn < timeToUse && depth <= 64; depth++) {
            try {
                Negamax(0, depth, -2147483646, 2147483646);
            } catch {
                break;
            }
        }

        int Negamax(int ply, int depth, int alpha, int beta)
        {
            if (timer.MillisecondsElapsedThisTurn >= timeToUse)
                throw new TimeoutException();
            
            if (ply != 0 && board.IsDraw()) return 0;
            if (depth <= 0) return Evaluate();

            Move[] moves = board.GetLegalMoves();
            if (moves.Length == 0) return board.IsInCheck() ? -2147483646 + ply : 0;

            int  bestEvaluation  = -2147483646  ;
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

        int Evaluate()
        {
            long result = 0;
            for (int sq = 0; sq < 64; sq++)
            {
                Piece piece = board.GetPiece(new Square(sq));
                if (piece.PieceType != 0)
                {
                    // Dear Programmer!
                    // When I wrote this code, only god and I
                    // knew how it worked.
                    // 
                    // Now, only god knows it!
                    //
                    // TLDR: DO NOT TOUCH THE FORMULA
                    long value = pst[((int)piece.PieceType - 1) * 8 + (piece.IsWhite ? sq : sq ^ 56) / 8] >> (sq ^ (sq & 4) / 4 * 7) % 4 * 16 & 32767;
                    result += piece.IsWhite == board.IsWhiteToMove ? value : -value;
                }
            }
            return (int)result;
        }

        return bestMove;
    }
    
}