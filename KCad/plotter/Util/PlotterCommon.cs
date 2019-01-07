using CadDataTypes;
using KCad;
using KCad.Properties;
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
        public static Action clear = () => { };

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

        public static void printError(string s)
        {
            println(
                    AnsiEsc.RedBG +
                    Resources.error_title + ": " + s
                    );
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
            DOut.pl(prefix + "{");
            DOut.Indent++;
            DOut.pl("x:" + v.x.ToString());
            DOut.pl("y:" + v.y.ToString());
            DOut.pl("z:" + v.z.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
