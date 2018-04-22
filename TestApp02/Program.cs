using HalfEdgeNS;
using MessagePack;
using MyCollections;
using Newtonsoft.Json.Linq;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace TestApp02
{
    class Program
    {
        static CadLayer getTestData_Layer(string fname)
        {
            CadObjectDB db = getTestData(fname);

            CadLayer layer = db.CurrentLayer;

            return layer;
        }

        static CadObjectDB getTestData(string fname)
        {
            StreamReader reader = new StreamReader(fname);

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jo = JObject.Parse(js);

            CadObjectDB db = CadJson.FromJson.DbFromJson(jo);

            return db;
        }


        public const int TestCant = 5000;

        static void test001()
        {
            CadLayer layer = getTestData_Layer(@"..\..\..\TestData\TestData1.txt");
            CadFigure fig = layer.FigureList[1];

            MpFigure mpFig = MpFigure.Create(fig);


            byte[] bfig = null;
            bfig = MessagePackSerializer.Serialize(mpFig);

            Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int i=0; i<TestCant; i++)
            {
                bfig = MessagePackSerializer.Serialize(mpFig);
            }

            sw.Stop();

            Console.WriteLine("Test001:" + sw.ElapsedMilliseconds.ToString());

            //string js = MessagePackSerializer.ToJson(bfig);
            //printJson(js);
            //Console.WriteLine(js);

            MpFigure rmpFig = null;
            rmpFig = MessagePackSerializer.Deserialize<MpFigure>(bfig);

            sw.Start();

            for (int i = 0; i < TestCant; i++)
            {
                rmpFig = MessagePackSerializer.Deserialize<MpFigure>(bfig);
            }

            sw.Stop();
            Console.WriteLine("Test001:" + sw.ElapsedMilliseconds.ToString());

            CadFigure rfig = rmpFig.Restore();
        }

        static void printJson(string js)
        {
            JObject jo = JObject.Parse(js);

            Console.WriteLine(jo.ToString());
        }

        static void test002()
        {
            CadLayer layer = getTestData_Layer(@"..\..\..\TestData\TestData1.txt");
            CadFigure fig = layer.FigureList[1];

            JObject jo = null;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int i=0; i<TestCant; i++)
            {
                jo = CadJson.ToJson.FigToJson(fig);
            }

            sw.Stop();

            Console.WriteLine("Test002:" + sw.ElapsedMilliseconds.ToString());

            jo = CadJson.ToJson.FigToJson(fig);

            sw.Start();

            for (int i = 0; i < TestCant; i++)
            {
                fig = CadJson.FromJson.FigFromJson(jo, CadJson.CurrentVersion);
            }

            sw.Stop();

            Console.WriteLine("Test002:" + sw.ElapsedMilliseconds.ToString());
        }


        static void test003()
        {
            CadLayer layer = getTestData_Layer(@"..\..\..\TestData\TestData1.txt");
            CadFigure tfig = layer.FigureList[1];

            if (!(tfig is CadFigureMesh))
            {
                return;
            }


            CadFigureMesh fig = (CadFigureMesh)tfig;


            DumpHeModel(fig.mHeModel);

            Console.Write("\n\n\n");

            MpFigure mpFig = MpFigure.Create(fig);

            fig = null;

            byte[] bfig = null;

            bfig = MessagePackSerializer.Serialize(mpFig);

            string js = MessagePackSerializer.ToJson(bfig);

            MpFigure rmpFig = MessagePackSerializer.Deserialize<MpFigure>(bfig);

            CadFigure rfig = rmpFig.Restore();

            fig = (CadFigureMesh)rfig;

            DumpHeModel(fig.mHeModel);
        }

        static void test004()
        {
            CadLayer layer = getTestData_Layer(@"..\..\..\TestData\TestData2.txt");
            CadFigure fig = layer.FigureList[0];

            MpFigure mpFig = MpFigure.Create(fig, true);

            fig = null;

            byte[] bfig = null;

            bfig = MessagePackSerializer.Serialize(mpFig);

            string js = MessagePackSerializer.ToJson(bfig);

            MpFigure rmpFig = MessagePackSerializer.Deserialize<MpFigure>(bfig);

            CadFigure rfig = rmpFig.Restore();

            string s =CadFigure.Util.DumpString(rfig, "");

            Console.Write(s);
        }

        static void test005()
        {
            CadObjectDB db = getTestData(@"..\..\..\TestData\TestData2.txt");

            MpCadObjectDB mpDB = MpCadObjectDB.Create(db);

            byte[] bin_db = MessagePackSerializer.Serialize(mpDB);

            MpCadObjectDB rmpDB = MessagePackSerializer.Deserialize<MpCadObjectDB>(bin_db);


            CadObjectDB rdb = rmpDB.Restore();

            rdb.dump();
        }


        static void Main(string[] args)
        {
            DebugOut.PrintFunc = Console.Write;
            DebugOut.PrintLnFunc = Console.WriteLine;
            DebugOut.FormatPrintFunc = Console.Write;

            //test002();
            //test001();

            //test003();

            //test004();

            //test005();

            
            Console.ReadLine();
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
    }
}
