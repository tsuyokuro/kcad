using System;
using System.Threading;

namespace Plotter
{
    public static class DOut
    {
        public static ulong PutCount = 0;

        public static int mIndent = 0;
        public static int IndentUnit = 2;

        public static String space = "";

        public static PrintFunc PrintFunc = Console.Write;
        public static PrintFunc PrintLnFunc = Console.WriteLine;
        public static FormatPrintFunc FormatPrintFunc = Console.Write;

        public static Mutex Lock = new Mutex();

        public static int Indent
        {
            set
            {
                mIndent = value;
                space = new string(' ', mIndent * IndentUnit);
            }

            get
            {
                return mIndent;
            }
        }

        public static void reset()
        {
            Begin();
            mIndent = 0;
            IndentUnit = 2;
            space = "";
            End();
        }

        public static void Begin()
        {
            Lock.WaitOne();
        }

        public static void End()
        {
            Lock.ReleaseMutex();
        }

        public static void printIndent()
        {
            p(space);
        }

        // print without new line
        public static void p(String s)
        {
            Begin();
            PutCount++;
            PrintFunc(s);
            End();
        }

        // print with new line
        public static void pl(String s)
        {
            Begin();
            PutCount++;
            PrintLnFunc(space + s);
            End();
        }

        // print with new line
        public static void tpl(String s)
        {
            DateTime dt = DateTime.Now;

            Begin();
            PutCount++;
            PrintLnFunc(dt.ToString("HH:mm:ss.fff") + " " + space + s);
            End();
        }

        // Format print without new line
        public static void pf(String format, params object[] args)
        {
            Begin();
            PutCount++;
            FormatPrintFunc(space + format, args);
            End();
        }
    }
}
