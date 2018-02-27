using MyCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp01
{
    public struct Vector
    {
        public double x;
        public double y;
        public double z;

        public static Vector Create(double x, double y, double z)
        {
            var ret = default(Vector);

            ret.x = x;
            ret.y = y;
            ret.z = z;

            return ret;
        }
    }

    public class Dummy
    {
        public int ID;

        public Dummy(int id)
        {
            ID = id;
        }
    }

    class Program
    {
        static void Test001()
        {
            AutoArray<Dummy> vl = new AutoArray<Dummy>();

            vl.Add(new Dummy(1));
            vl.Add(new Dummy(2));
            vl.Add(new Dummy(3));

            Dummy retv = vl.Find(v => { return v.ID == 100; });
        }


        static void Main(string[] args)
        {
            Test001();
            Console.ReadLine();
        }
    }
}
