﻿using HalfEdgeNS;
using MyCollections;
using Newtonsoft.Json.Linq;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

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
            FlexArray<Dummy> vl = new FlexArray<Dummy>(2);

            vl.Add(new Dummy(1));
            vl.Add(new Dummy(2));
            vl.Add(new Dummy(3));

            Dummy retv = vl.Find(v => { return v.ID == 100; });


            List<Dummy> list = new List<Dummy>();

            list.AddRange(vl);

            vl.AddRange(list);

            foreach (Dummy dummy in vl)
            {
                Console.WriteLine("dummy id={0}", dummy.ID);
            }
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


        static void Test004()
        {
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


            DumpHeModel(model);

            JObject jo = HeJson.HeModelToJson(model);


            string s = jo.ToString();

            Console.WriteLine("");
            Console.Write(s);
            Console.WriteLine("");

            HeModel rmodel = HeJson.HeModelFromJson(jo, CadJson.CurrentVersion);

            DumpHeModel(rmodel);
        }

        static void Test005()
        {
            CadMesh cm = new CadMesh();
            cm.VertexStore = new VectorList();

            cm.VertexStore.Add(CadVector.Create(10, 5, 0));
            cm.VertexStore.Add(CadVector.Create(15, 10, 0));
            cm.VertexStore.Add(CadVector.Create(13, 30, 0));
            cm.VertexStore.Add(CadVector.Create(5, 15, 0));

            cm.FaceStore = new FlexArray<CadFace>();

            cm.FaceStore.Add( new CadFace(0, 1, 3));
            cm.FaceStore.Add( new CadFace(1, 2, 3));

            HeModel hem = HeModel.Create(cm);

            DumpHeModel(hem);

        }

        static void DumpHeModel(HeModel model)
        {
            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                CadVector v;
                CadVector n;

                HeFace f = model.FaceStore[i];

                n = model.NormalStore.Ref(f.Normal);


                Console.WriteLine("face Normal {0} - {1},{2},{3}", f.Normal, n.x, n.y, n.z);


                HalfEdge head = f.Head;

                HalfEdge c = head;


                for (; ; )
                {
                    v = model.VertexStore.Ref(c.Vertex);
                    n = model.NormalStore.Ref(c.Normal);
                    Console.WriteLine("{0} - {1},{2},{3} Normal:{4} - {5},{6},{7}",
                        c.ID,
                        v.x, v.y, v.z,
                        c.Normal,
                        n.x, n.y, n.z);

                    HalfEdge pair = c.Pair;

                    if (pair != null)
                    {
                        v = model.VertexStore.Ref(pair.Vertex);
                        Console.WriteLine("  pair: {0} - {1},{2},{3}", pair.ID, v.x, v.y, v.z);
                    }

                    c = c.Next;

                    if (c == head) break;
                }
            }
        }

        static void Main(string[] args)
        {
            //Test001();
            //Test002();
            //Test002_01();
            //Test003();
            //Test004();

            Test005();
            Console.ReadLine();
        }
    }
}
