using HalfEdgeNS;
using MyCollections;
using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestApp01
{
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

        static void Test002()
        {
            // 左回り

            HeModel model = new HeModel();
            model.AddTriangle(
                CadVector.Create(10, 5, 0),
                CadVector.Create(15, 10, 0),
                CadVector.Create(5, 15, 0)
                );

            model.AddTriangle(
                CadVector.Create(15, 10, 0),
                CadVector.Create(13, 30, 0),
                CadVector.Create(5, 15, 0)
                );

            HeFace f = model.FaceStore[0];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            CadVector v;


            for (;;)
            {
                v = model.VertexStore.Ref(c.Vertex);
                Console.Write("{0},{1},{2}", v.x, v.y, v.z);

                v = model.VertexStore.Ref(c.Next.Vertex);
                Console.WriteLine("-{0},{1},{2}", v.x, v.y, v.z);

                if (c.Pair != null)
                {
                    v = model.VertexStore.Ref(c.Pair.Vertex);
                    Console.Write("  pair: {0},{1},{2}", v.x, v.y, v.z);

                    v = model.VertexStore.Ref(c.Pair.Next.Vertex);
                    Console.WriteLine("-{0},{1},{2}", v.x, v.y, v.z);
                }


                c = c.Next;

                if (c == head) break;
            }
        }

        static void Test002_01()
        {
            // 右回り

            HeModel model = new HeModel();
            model.AddTriangle(
                CadVector.Create(5, 15, 0),
                CadVector.Create(15, 10, 0),
                CadVector.Create(10, 5, 0)
                );

            model.AddTriangle(
                CadVector.Create(5, 15, 0),
                CadVector.Create(13, 30, 0),
                CadVector.Create(15, 10, 0)
                );

            HeFace f = model.FaceStore[0];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            CadVector v;


            for (; ; )
            {
                v = model.VertexStore.Ref(c.Vertex);
                Console.Write("{0},{1},{2}", v.x, v.y, v.z);

                v = model.VertexStore.Ref(c.Next.Vertex);
                Console.WriteLine("-{0},{1},{2}", v.x, v.y, v.z);

                if (c.Pair != null)
                {
                    v = model.VertexStore.Ref(c.Pair.Vertex);
                    Console.Write("  pair: {0},{1},{2}", v.x, v.y, v.z);

                    v = model.VertexStore.Ref(c.Pair.Next.Vertex);
                    Console.WriteLine("-{0},{1},{2}", v.x, v.y, v.z);
                }


                c = c.Next;

                if (c == head) break;
            }
        }

        static void Test003()
        {
            var v1 = CadVector.Create(10, 5, 0);
            var v2 = CadVector.Create(10, 5, 0);

            v2.Selected = true; ;

            if (v1 == v2)
            {
                Console.WriteLine("v1 == v2");
            }

            v1 = CadVector.Create(10, 5, 0);
            v2 = CadVector.Create(10, 5, 1);

            if (v1 != v2)
            {
                Console.WriteLine("v1 != v2");
            }

        }


        static void Main(string[] args)
        {
            //Test001();
            Test002();
            Test002_01();
            //Test003();
            Console.ReadLine();
        }
    }
}
