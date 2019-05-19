using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Vector3d iv = VectorUtil.InvalidVector3d;

            bool b;

            b = iv.IsInvalid();

            if (b)
            {
                Console.WriteLine("iv is invalid");
            }

            Vector3d v = default;

            VectorUtil.Set(out v, 1, 2, 3);

            if (v.IsValid())
            {
                Console.WriteLine("v is valid");
            }

            Console.WriteLine("end");

            Console.ReadLine();
        }
    }
}
