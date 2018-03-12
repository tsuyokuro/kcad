using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp02
{
    struct Vertex
    {
        public double x;
        public double y;

        public Vertex(double x_, double y_)
        {
            x = x_;
            y = y_;
        }
    }


    class Program
    {
        public static List<Vertex> VList = new List<Vertex>();

        static void Main(string[] args)
        {
            int listCnt = 20000;

            for (int i=0;i<listCnt;i++)
            {
                VList.Add(new Vertex(i, i+1));
            }

            int callNum = 1000;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < callNum; i++)
            {
                Test01();
            }

            sw.Stop();
            Console.WriteLine("Test01 " + sw.ElapsedMilliseconds);


            sw.Reset();
            sw.Start();

            for (int i = 0; i < callNum; i++)
            {
                Test02();

            }

            sw.Stop();
            Console.WriteLine("Test02 " + sw.ElapsedMilliseconds);

            Console.ReadLine();
        }

        static void ForEach(Action<Vertex> action)
        {
            for (int i=0;i<VList.Count;i++)
            {
                action(VList[i]);
            }
        }

        static void Test01()
        {
            int n = 0;
            ForEach(v => { n++; });
        }

        static void Test02()
        {
            Test02_sub();
        }

        static void Test02_sub()
        {
            int n = 0;

            for (int i = 0; i < VList.Count; i++)
            {
                n++;
            }
        }
    }
}
