using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder
{
    public static class ConsoleMethods
    {
        public static void PrintError(string message, bool sameLine = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Print(message, sameLine);
            Console.ResetColor();
        }

        public static void PrintSuccess(string message, bool sameLine = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Print(message, sameLine);
            Console.ResetColor();
        }

        public static void PrintProgress(string message, bool sameLine = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Print(message, sameLine);
            Console.ResetColor();
        }

        public static void PrintInfo(string message, ConsoleColor color = ConsoleColor.Cyan, bool sameLine = false)
        {
            Console.ForegroundColor = color;
            Print(message, sameLine);
            Console.ResetColor();
        }

        public static void Print(string message, bool sameLine = false)
        {
            if (sameLine)
                Console.Write(message);
            else
                Console.WriteLine(message);
        }
    }
}
