using System;

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
    }

    public delegate void VoidFunc();

    public class DebugOut
    {
        private int mIndent = 0;
        private int IndentUnit = 2;

        private String space = "";

        public int Indent
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

        public void printIndent()
        {
            Console.Write(space);
        }

        public void print(String s)
        {
            Console.Write(s);
        }

        public void println(String s)
        {
            Console.WriteLine(space + s);
        }
    }
}
