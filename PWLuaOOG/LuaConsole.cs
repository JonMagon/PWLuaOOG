using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWLuaOOG
{
    class LuaConsole
    {
        // Запись любого текста с указанным цветом
        private void Print(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Print(string message, bool line = true)
        {
            if (line)
                Console.WriteLine(message);
            else
                Console.Write(message);
        }

        public void Log(string message)
        {
            Print("Log: " + message, ConsoleColor.White);
        }

        public void Warning(string message)
        {
            Print("Warning: " + message, ConsoleColor.Yellow);
        }

        public void Error(string message)
        {
            Print("Error: " + message, ConsoleColor.Red);
        }

        public void Success(string message)
        {
            Print("Success: " + message, ConsoleColor.Green);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }

}
