using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLServerBackupTool
{
    public class ConsoleHelper
    {
        public static void WriteColor(ConsoleColor c, string text)
        {
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = c;
            Console.Write(text);
            Console.ForegroundColor = previousColor;
        }

        public static void WriteStatus(int ident, OutputStatusType s, string text)
        {
            for (var i = 0; i < ident; i++)
            {
                Console.Write(" ");
            }

            Console.Write("[ ");

            switch (s)
            {
                case OutputStatusType.Info:
                    WriteColor(ConsoleColor.Yellow, "INFO");
                    break;
                case OutputStatusType.OK:
                    WriteColor(ConsoleColor.Green, "OK");
                    break;
                case OutputStatusType.Error:
                    WriteColor(ConsoleColor.Red, "ERROR");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("s");
            }

            Console.Write(" ] {0}", text);
            Console.WriteLine();
        }
    }
}
