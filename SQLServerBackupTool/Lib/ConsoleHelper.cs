using System;

namespace SQLServerBackupTool.Lib
{
    public static class ConsoleHelper
    {
        /// <summary>
        /// Enable/disable all coloring while using those helpers
        /// </summary>
        public static bool DisableColoring
        {
            get;
            set;
        }

        /// <summary>
        /// Writes a string to the console using the given color
        /// </summary>
        /// <param name="c">Foreground color to use</param>
        /// <param name="text">Text to write</param>
        public static void WriteColor(ConsoleColor c, string text)
        {
            if (DisableColoring)
            {
                Console.Write(text);
                return;
            }

            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = c;
            Console.Write(text);
            Console.ForegroundColor = previousColor;
        }

        /// <summary>
        /// Utility function for writing nice log output to console with identation and color
        /// </summary>
        /// <param name="indent">Number of space to prepend</param>
        /// <param name="s">Type of message</param>
        /// <param name="text">Content of message</param>
        public static void WriteStatus(int indent, OutputStatusType s, string text)
        {
            for (var i = 0; i < indent; i++)
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

                case OutputStatusType.Warning:
                    WriteColor(ConsoleColor.Magenta, "WARN");
                    break;

                default:
                    throw new ArgumentOutOfRangeException("s");
            }

            Console.Write(" ] {0}", text);
            Console.WriteLine();
        }
    }
}
