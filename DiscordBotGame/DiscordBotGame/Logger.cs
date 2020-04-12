using System;
using System.Diagnostics;

namespace DiscordBotGame
{
    public static class Logger
    {
        //@Improvement add back the verbose levels at some point
        public static void Log(string s)
        {
            Log(1, s);
        }

        public static void Log(int level, string s)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("LOG");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ResetColor();

            Console.WriteLine(s);
        }

        public static void Warn(string s)
        {
            Warn(1, s);
        }

        public static void Warn(int level, string s)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARNING");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ResetColor();

            Console.WriteLine(s);
        }

        public static void Error(string s)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ResetColor();
            Console.WriteLine(s);
        }

        public static void Debug(string s)
        {
            try
            {
                var stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(2);
                var meth = frame.GetMethod();

                var args = "";

                foreach (var info in meth.GetParameters())
                {
                    args += info.ParameterType.Name + ",";
                }

                s = "[" + meth.DeclaringType.Name + "::" + meth.Name + "(" +
                    args.Trim(',') + ")]" + s;
            }
            catch (Exception)
            {
            }


            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("DEBUG");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ResetColor();
            Console.WriteLine(s);
        }
    }
}