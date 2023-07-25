using System;

namespace ChessChallenge.API
{
    public sealed class Timer
    {
        /// <summary>
        /// Amount of time left on clock for current player (in milliseconds)
        /// </summary>
        public int MillisecondsRemaining => Math.Max(0, initialMillisRemaining - (int)sw.ElapsedMilliseconds);
        /// <summary>
        /// Amount of time elapsed since current player started thinking (in milliseconds)
        /// </summary>
        public int MillisecondsElapsedThisTurn => (int)sw.ElapsedMilliseconds;

        System.Diagnostics.Stopwatch sw;
        readonly int initialMillisRemaining;
        private readonly int maxDepth;

        public Timer(int millisRemaining)
        {
            initialMillisRemaining = millisRemaining;
            maxDepth = 64;
            sw = System.Diagnostics.Stopwatch.StartNew();
        }

        public Timer(int millisRemaining, int depth)
        {
            initialMillisRemaining = millisRemaining;
            maxDepth = depth;
            sw = System.Diagnostics.Stopwatch.StartNew();
        }

        public bool Stop(int currentDepth, int timeLimit)
        {
            return currentDepth > maxDepth || MillisecondsElapsedThisTurn >= timeLimit;
        }
        
    }
}