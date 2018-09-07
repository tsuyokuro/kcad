using CadDataTypes;
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

    public static class CadClipBoard
    {
        public const string TypeNameJson = "List.CadFiguer.Json";
        public const string TypeNameBin = "List.CadFiguer.bin";
    }

    static class CadVectorExtensions
    {
        public static void dump(this CadVector v, string prefix = nameof(CadVector))
        {
            DebugOut.println(prefix + "{");
            DebugOut.Indent++;
            DebugOut.println("x:" + v.x.ToString());
            DebugOut.println("y:" + v.y.ToString());
            DebugOut.println("z:" + v.z.ToString());
            DebugOut.Indent--;
            DebugOut.println("}");
        }
    }
}
