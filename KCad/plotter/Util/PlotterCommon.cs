using System;
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
    }

    public delegate void VoidFunc();

    public class DebugOut
    {
        public static DebugOut inst = new DebugOut();

        public static DebugOut Std {
            get
            {
                inst.reset();
                return inst;
            }
        }


        protected int mIndent = 0;
        protected int IndentUnit = 2;

        protected String space = "";

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
            Console.Write(space);
        }

        public virtual void print(String s)
        {
            Console.Write(s);
        }

        public virtual void println(String s)
        {
            Console.WriteLine(space + s);
        }
    }

    public class DebugOutVS : DebugOut
    {
        public override void printIndent()
        {
            Debug.Write(space);
        }

        public override void print(String s)
        {
            Debug.Write(s);
        }

        public override void println(String s)
        {
            Debug.WriteLine(space + s);
        }
    }
}
