using System;
using System.Diagnostics;
using System.Threading;

namespace Plotter
{
    public class IdProvider
    {
        private uint mCounter = 0;

        public uint Counter
        {
            get { return mCounter; }
            set { mCounter = value; }
        }

        public uint getNew()
        {
            return ++mCounter;
        }

        public void Reset()
        {
            mCounter = 0;
        }
    }

    public delegate void VoidFunc();

    public delegate void PrintFunc(string s);
    public delegate void FormatPrintFunc(string format, params object[] args);

    public class ItConsole
    {
        public static PrintFunc PrintFunc = (s)=>{};
        public static PrintFunc PrintLnFunc = (s) => { };
        public static FormatPrintFunc FormatPrintFunc = (s, args) => { };

        public static void print(string s)
        {
            PrintFunc(s);
        }

        public static void println(string s)
        {
            PrintLnFunc(s);
        }

        public static void printf(string s, params object[] args)
        {
            FormatPrintFunc(s, args);
        }
    }

    public class DebugOut
    {
        public static ulong PutCount = 0;

        public static int mIndent = 0;
        public static int IndentUnit = 2;

        public static String space = "";

        public static PrintFunc PrintFunc = (s) => { };
        public static PrintFunc PrintLnFunc = (s) => { };
        public static FormatPrintFunc FormatPrintFunc = (s, args) => { };

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
            print(space);
        }

        public static void print(String s)
        {
            Begin();
            PutCount++;
            PrintFunc(s);
            End();
        }

        public static void println(String s)
        {
            Begin();
            PutCount++;
            PrintLnFunc(space + s);
            End();
        }

        public static void printf(String format, params object[] args)
        {
            Begin();
            PutCount++;
            FormatPrintFunc(space + format, args);
            End();
        }
    }
}
