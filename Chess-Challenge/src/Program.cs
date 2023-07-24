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
                // Make sure we always flush:
                Console.Out.Flush();
                StreamWriter standardOutput = new(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
                
                UciProgram uci = new();
                uci.Start();
            }
        }
    }    
}
