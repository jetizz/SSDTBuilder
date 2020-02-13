using System;

namespace SSDTBuilder
{
    internal class StatusWriter
    {
        public enum Level
        {
            Verbose = 1,
            Info,
            Success,
            Error,
            None
        }

        private readonly Level _level;
        private static readonly object _lock = new object();

        public StatusWriter(Level level)
        {
            _level = level;
        }
        public StatusWriter(bool isSilent, bool isVerbose)
        {
            if (isSilent) _level = Level.None;
            else if (isVerbose) _level = Level.Verbose;
            else _level = Level.Info;
        }

        public void Verbose(string format, params object[] args) => Write(Level.Verbose, format, args);
        public void Info(string format, params object[] args) => Write(Level.Info, format, args);
        public void Error(string format, params object[] args) => Write(Level.Error, format, args);
        public void Success(string format, params object[] args) => Write(Level.Success, format, args);
        public void Write(Level level, string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                return;

            if (level < _level)
                return;

            string message = args != null && args.Length > 0 ? string.Format(format, args) : format;

            lock (_lock)
            {
                ConsoleColor original = Console.ForegroundColor;
                Console.ForegroundColor = GetColor(level);
                Console.WriteLine(message);
                Console.ForegroundColor = original;
            }
        }
        private static ConsoleColor GetColor(Level level)
        {
            switch (level)
            {
                default:
                case Level.Info: return ConsoleColor.Gray;
                case Level.Verbose: return ConsoleColor.DarkGray;
                case Level.Error: return ConsoleColor.Red;
                case Level.Success: return ConsoleColor.Green;
            }
        }
    }
}
