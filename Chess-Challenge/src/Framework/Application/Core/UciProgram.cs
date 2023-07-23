using System;
using ChessChallenge.API;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

namespace ChessChallenge.Application
{
    class UciProgram
    {

        private IChessBot bot;
        private Board board;
        private static readonly string StartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; 

        public UciProgram()
        {
            board = Board.CreateBoardFromFEN(StartingPosition);
            bot = new MyBot();
        }
        
        private static string MoveToUci(Move move)
        {
            string str = move.ToString();
            string res = "";
            bool data = false;
            foreach (char c in str)
            {
                if (data && c != '\'') res += c;
                if (c == ' ') data = true;
            }
            return res;
        }

        private void LoadPosition(string[] tokens)
        {
            bool makeMoves = false, fen = false;
            foreach (string token in tokens)
            {
                if (fen) {
                    board = Board.CreateBoardFromFEN(token);
                    fen = false;
                } else if (makeMoves) {
                    Move[] moves = board.GetLegalMoves();
                    foreach (Move move in moves) {
                        if (MoveToUci(move) == token)
                        {
                            board.MakeMove(move);
                            break;
                        }
                    }
                } else if (token == "startpos") {
                    board = Board.CreateBoardFromFEN(StartingPosition);
                } else if (token == "fen") {
                    fen = true;
                } else if (token == "moves") {
                    makeMoves = true;
                }
            }
        }

        private void StartSearch(string[] tokens)
        {
            Console.WriteLine(board.GetFenString());
            int time = int.MaxValue;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "wtime" && board.IsWhiteToMove)
                {
                    time = int.Parse(tokens[i + 1]);
                    i++;
                }

                if (tokens[i] == "btime" && !board.IsWhiteToMove)
                {
                    time = int.Parse(tokens[i + 1]);
                    i++;
                }
            }

            Timer timer = new Timer(time);
            Move bestMove = bot.Think(board, timer);
            Console.WriteLine($"bestmove {MoveToUci(bestMove)}");
        }
        
        public void Start()
        {

            while (true)
            {
                string line = Console.ReadLine();
                string[] tokens = line.Split(" ");
                if (tokens[0] == "uci") {
                    Console.WriteLine("id name FindingChessBot");
                    Console.WriteLine("id author Sebastian Lague, Rak Laptudirm, Shaheryar Sohail, Balazs Szilagyi");
                    Console.WriteLine("uciok");           
                } else if (tokens[0] == "isready") {
                    Console.WriteLine("readyok");
                } else if (tokens[0] == "position") {
                    LoadPosition(tokens);
                } else if (tokens[0] == "go") {
                    StartSearch(tokens);
                } else if (tokens[0] == "quit") {
                    break;
                }
            }
        }
    }
}