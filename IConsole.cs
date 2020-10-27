using System;

namespace ScoreConverter
{    
    public interface IConsole
    {        
        bool ConfirmChoice();
        void Write(string line);
        void WriteLine(string line);
    }

    public class ConsoleWrapper : IConsole
    {
        private bool _interactive;
        private bool _silent;
        public ConsoleWrapper(bool interactive, bool silent)
        {
            _interactive = interactive;
            _silent = silent;
        }

        public void WriteLine(string line)
        {
            if (_silent) return;
            Console.WriteLine(line);
        }

        public void Write(string line)
        {
            if (_silent) return;
            Console.Write(line);
        }

        public bool ConfirmChoice()
        {
            if (!_interactive) return true;
            return Console.ReadKey().Key.ToString().ToLower() == "y";
        }
    }
}
