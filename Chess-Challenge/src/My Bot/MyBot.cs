using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{

    // Compressed Piece-Square Tables.
    // Refer to the generation script for details on it's internals.
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
        var bestRootMove = Move.NullMove;
        var timeToUse = timer.MillisecondsRemaining / 25 + 70;
        try
        {
            // Main iterative deepening loop.
            for (var depth = 1; timer.MillisecondsElapsedThisTurn < timeToUse && depth <= 64; depth++)
                Negamax(0, depth, -2147483646, 2147483646);
        } catch { /* Catch clause to catch timeout error. */}

        // Negamax search function.
        int Negamax(int ply, int depth, int alpha, int beta)
        {
            // Check if time has expired.
            if (timer.MillisecondsElapsedThisTurn >= timeToUse)
                throw new TimeoutException();
            
            // Various draw checks.
            // TODO: Make this more efficient
            if (ply != 0 && board.IsDraw()) return 0;
            
            // Horizon check.
            if (depth <= 0) return Evaluate();

            // Move Generation and Mate detection.
            var moves = board.GetLegalMoves();
            if (moves.Length == 0) return board.IsInCheck() ? ply - 2147483646 : 0;

            var bestScore= -2147483646;
            var bestMove = Move.NullMove;
            foreach (var move in moves) {
                board.MakeMove(move);
                var score = -Negamax(ply + 1, depth - 1, -beta, -alpha);
                board.UndoMove(move);
                
                // Check if new best move has been found.
                if (score <= bestScore) continue;
                bestScore  = score;
                bestMove = move;
                
                // Check if new best move raises alpha.
                alpha = Math.Max(score, alpha);
                
                // Check for a beta cutoff.
                if (score >= beta) break;
            }
            
            // If this is the root negamax call, set the best root move on exit.
            if (ply == 0) bestRootMove = bestMove;
            
            // Return the score of the current node, which is the best move in it.
            return bestScore;
        }

        // Evaluate statically evaluates the current position.
        int Evaluate()
        {
            long result = 0;
            for (var sq = 0; sq < 64; sq++)
            {
                var piece = board.GetPiece(new Square(sq));
                if (piece.PieceType != 0)
                {
                    // Dear Programmer!
                    // When I wrote this code, only god and I
                    // knew how it worked.
                    // 
                    // Now, only god knows!
                    //
                    // TLDR: DO NOT TOUCH THE FORMULA
                    var value = pst[((int)piece.PieceType - 1) * 8 + (piece.IsWhite ? sq : sq ^ 56) / 8] >> (sq ^ (sq & 4) / 4 * 7) % 4 * 16 & 32767;
                    result += piece.IsWhite == board.IsWhiteToMove ? value : -value;
                }
            }
            return (int)result;
        }

        return bestRootMove;
    }
    
}