﻿using System;
using System.Diagnostics;

namespace Plotter
{
    [Serializable]
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

    public class DebugOut
    {
        public delegate void PrintFunc(string s);
        public delegate void FormatPrintFunc(string format, params object[] args);

        public static DebugOut StdInstance = new DebugOut();

        public static DebugOut Std {
            get
            {
                return StdInstance;
            }
        }

        public static PrintFunc sStdPrintLn = Console.WriteLine;

        public static PrintFunc sStdPrint = Console.Write;

        public static FormatPrintFunc sStdPrintf = Console.Write;

        public static PrintFunc StdPrintLn
        {
            get
            {
                return sStdPrintLn;
            }
            set
            {
                sStdPrintLn = value;
                Std.DelegatePrintLn = value;
            }
        }

        public static PrintFunc StdPrint
        {
            get
            {
                return sStdPrint;
            }

            set
            {
                sStdPrint = value;
                Std.DelegatePrint = value;
            }
        }

        public static FormatPrintFunc StdPrintf
        {
            get
            {
                return sStdPrintf;
            }

            set
            {
                sStdPrintf = value;
                Std.DelegatePrintf = value;
            }
        }

        public ulong PutCount = 0;

        protected int mIndent = 0;
        protected int IndentUnit = 2;

        protected String space = "";

        public PrintFunc DelegatePrint = StdPrint;

        public PrintFunc DelegatePrintLn = StdPrintLn;

        public FormatPrintFunc DelegatePrintf = StdPrintf;


        public virtual int Indent
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

        public void reset()
        {
            mIndent = 0;
            IndentUnit = 2;
            space = "";
        }

        public virtual void printIndent()
        {
            DelegatePrint(space);
        }

        public virtual void print(String s)
        {
            PutCount++;
            DelegatePrint(s);
        }

        public virtual void println(String s)
        {
            PutCount++;
            DelegatePrintLn(space + s);
        }

        public virtual void printf(String format, params object[] args)
        {
            PutCount++;
            DelegatePrintf(space + format, args);
        }
    }
}
