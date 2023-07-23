using System;
using System.IO;
using ChessChallenge.Application;

namespace ChessChallenge
{
    static class Program
    {

        private static int CountTokens()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "src", "My Bot", "MyBot.cs");
            return TokenCounter.CountTokens(File.ReadAllText(path));
        }
        
        public static void Main(string[] args)
        {
            Console.WriteLine("FindingChessBot by Sebastian Lague, Rak Laptudirm, Shaheryar Sohail, Balazs Szilagyi");
            Console.WriteLine($"Token count: {CountTokens()}");
            if (args.Length == 1 && args[0] == "gui")
            {
                GuiProgram.Start();
            }
            else
            {
                UciProgram uci = new UciProgram();
                uci.Start();
            }
        }
    }    
}
