using System;
using System.Collections.Generic;
using System.Text;

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
        private bool silent;
        public ConsoleWrapper(bool silent)
        {
            this.silent = silent;
        }

        public void WriteLine(string line)
        {
            if (silent) return;
            Console.WriteLine(line);
        }

        public void Write(string line)
        {
            if (silent) return;
            Console.Write(line);
        }

        public bool ConfirmChoice()
        {
            if (silent) return true;
            return Console.ReadKey().Key.ToString().ToLower() == "y";
        }
    }
}
