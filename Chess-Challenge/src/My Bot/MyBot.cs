using System;
using System.Linq;
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
           28147927174348900,    31807101871718493,    28147905699446876,    26177567977832547, 
           27584994401910901,    43910753508786360,    69525315697312031,    28147927174348900, 
           78251254722724076,    83880745668772096,    87258518403940630,    88947454165123354, 
           90636278255845658,    84162267890385155,    81910399356895480,    75717941276770505, 
           84725230730281253,    88384435491635483,    90917761822163239,    89228942026932525, 
           91199266864300344,    86695650056864058,    86414093475512616,    85288210747687198, 
          139332205153878496,   138206322425922033,   138769259494638052,   139332235217863145, 
          141302590120067574,   141865522893357553,   139895223827038711,   144117387132404217, 
          251923832245322587,   248827646155162468,   255864692374635401,   262057214878614427, 
          269375590044074920,   266560818799641485,   269657000594506628,   260931250545361816, 
          273597912264475581,   285419938596979671,   287953264927769567,   289079194899514333, 
          288797762873590763,   287108964554114037,   286264509557834739,   278383111424443351, 
    };

    private static readonly long[] PieceWeights =
    {
        0, 0, 1, 1, 2, 4, 0
    };

    public ulong Nodes;
    
    public Move Think(Board board, Timer timer)
    {
        var bestMove = Move.NullMove;
        var timeToUse = timer.MillisecondsRemaining / 20 + 75;

        Nodes = 0;

        try {
            // Main iterative deepening loop.
            // NOTE: Replace !timer.Stop(depth, timeToUse) with: timer.MillisecondsElapsedThisTurn < timeToUse before
            // submitting.
            for (var depth = 1; !timer.Stop(depth, timeToUse); depth++)
                Search(0, depth, -1000000, 1000000);
        } catch { /* Catch clause to catch timeout error. */ }

        // Alpha-Beta + QSearch (Negamax) search function.
        int Search(int ply, int depth, int alpha, int beta)
        {
            // Check if time has expired every 4096 nodes.
            if ((Nodes & 4095) == 0 && timer.MillisecondsElapsedThisTurn >= timeToUse)
                throw new TimeoutException();

            // Check if we're in quiescence search so that we may avoid the horizon effect.
            var quiescence = depth <= 0;

            // Set the best evaluation to the lowest possible value, or the evaluation of the position
            // if we're in quiescence.
            var bestEvaluation = -1000000;
            if (quiescence) {
                bestEvaluation = Evaluate();
                
                // If we're in quiescence and the evaluation is too high, return it, as it is a branch guaranteed
                // to be a winning.
                if (bestEvaluation >= beta) return bestEvaluation;
                
                // If we're in quiescence and the evaluation is higher than alpha, set alpha to it as our lower bound
                // cannot be lower than this.
                alpha = Math.Max(alpha, bestEvaluation);
            }
            
            // Generate all legal moves (or only capture moves if we're in quiescence).
            var moves = board.GetLegalMoves(quiescence)
                .OrderByDescending(move => move.CapturePieceType)
                .ThenBy(move => move.MovePieceType);
            
            // If we're not in quiescence and there are no legal moves, the game is over.
            // Return a mate score if we're in check, or a draw score if we're not (stalemate).
            if (!quiescence && !moves.Any()) return board.IsInCheck() ? -1000000 + ply : 0;
            
            var currentBestMove = Move.NullMove;
            foreach (var move in moves) {
                board.MakeMove(move);
                Nodes++;
                var evaluation = -Search(ply + 1, depth - 1, -beta, -alpha);
                board.UndoMove(move);
                
                // Check if the evaluation is better than the current best evaluation.
                if (evaluation <= bestEvaluation) continue;
                
                // If it is, then we have a new best evaluation and a new best move.
                bestEvaluation  = evaluation;
                currentBestMove = move;
                
                // Check if the evaluation is better than alpha.
                if (evaluation <= alpha) continue;
                
                // If it is, then we have a new lower bound.
                alpha = evaluation;
                
                // Check if the evaluation is better than beta, and if it is, then we have a beta cutoff.
                if (evaluation >= beta) break;
            }
            
            // If we're at the root, set the best move to the current best move.
            if (ply == 0) bestMove = currentBestMove;
            
            // Return the best evaluation for this branch point.
            return bestEvaluation;
        }

        long GetValue(Piece piece, int sq, int offset)
        {
            // Dear Programmer!
            // When I wrote this code, only god and I
            // knew how it worked.
            // 
            // Now, only god knows!
            //
            // TLDR: DO NOT TOUCH THE FORMULA
            return (pst[((int)piece.PieceType - 1) * 8 + (piece.IsWhite ? sq : sq ^ 56) / 8 + offset] >>
                (sq ^ (sq & 4) / 4 * 7) % 4 * 16 & 32767) * (piece.IsWhite == board.IsWhiteToMove ? 1 : -1);
        }

        // Evaluate statically evaluates the current position.
        int Evaluate()
        {
            long mgResult = 0L, egResult = 0L, phase = 0L;
            for (var sq = 0; sq < 64; sq++)
            {
                var piece = board.GetPiece(new Square(sq));
                if (piece.PieceType == 0) continue;

                phase += PieceWeights[(int)piece.PieceType];

                mgResult += GetValue(piece, sq, 0);
                egResult += GetValue(piece, sq, 48);
            }

            return (int)(mgResult * phase + egResult * (24 - phase)) / 24;
        }

        return bestMove;
    }
    
}