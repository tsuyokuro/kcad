using MyCollections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp02
{
    class Program
    {
        static void Main(string[] args)
        {
            FlexArray<int> a = new FlexArray<int>();

            a.Add(1);
            a.Add(2);
            a.Add(3);
            a.Add(4);

            a.Reverse();
        }
    }
}
