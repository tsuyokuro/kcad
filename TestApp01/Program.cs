using HalfEdgeNS;
using MyCollections;
using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalfEdgeNS
{
    public class HeFace
    {
        public HalfEdge Head; // HalfEdge link listの先頭

        public HeFace(HalfEdge he)
        {
            Head = he;
        }
    }

    public class HalfEdge
    {
        public HalfEdge Pair;
        public HalfEdge Next;
        public HalfEdge Prev;

        public HeFace Face;

        public int Vertex;      // 頂点のIndex

        public HeEdge Edge;


        public HalfEdge(int vertex)
        {
            Vertex = vertex;
        }
    }

    public class HeEdge
    {
        public CadVector P0;
        public CadVector P1;

        public HalfEdge L;
        public HalfEdge R;
    }

    public class HeModel
    {
        public AutoArray<CadVector> VertexStore = new AutoArray<CadVector>(8);
        public AutoArray<HeFace> FaceStore = new AutoArray<HeFace>(6);

        public void SetHalfEdgePair(HalfEdge he)
        {
            // すべてのFaceを巡回する
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                // 現在のFaceに含まれるハーフエッジを巡回する
                HalfEdge c = FaceStore[i].Head;

                for (;;)
                {
                    if (he.Vertex == c.Next.Vertex &&
                        he.Next.Vertex == c.Vertex)
                    {
                        //  両方の端点が共通だったらpairに登録する
                        he.Pair = c;
                        c.Pair = he;
                        return;
                    }

                    c = c.Next;

                    if (c == FaceStore[i].Head) break;
                }
            }
        }

        // 同じ座標がなければ追加してIndexを返す
        // 同じ座標があれば、そのIndexを返す
        public int AddPointWithoutSame(CadVector v)
        {
            int cnt = VertexStore.Count;
            for (int i=0; i< cnt; i++ )
            {
                ref CadVector rv = ref VertexStore.Ref(i);
                if (v.VectorEquals(rv))
                {
                    return i;
                }
            }

            return VertexStore.Add(v);
        }

        // 単純に頂点を追加
        public int AddPoint(CadVector v)
        {
            return VertexStore.Add(v);
        }

        // 三角形の追加
        // 座標は左回りで設定する
        public void AddTriangle(CadVector v0, CadVector v1, CadVector v2)
        {
            AddTriangle(
                AddPointWithoutSame(v0),
                AddPointWithoutSame(v1),
                AddPointWithoutSame(v2)
                );
        }

        // 三角形の追加
        // 座標は左回りで設定する
        public void AddTriangle(int v0, int v1, int v2)
        {
            HalfEdge he0 = new HalfEdge(v0);
            HalfEdge he1 = new HalfEdge(v1);
            HalfEdge he2 = new HalfEdge(v2);

            he0.Next = he1;
            he0.Prev = he2;
            he1.Next = he2;
            he1.Prev = he0;
            he2.Next = he0;
            he2.Prev = he1;
            HeFace face = new HeFace(he0);

            FaceStore.Add(face);

            he0.Face = face;
            he1.Face = face;
            he2.Face = face;
            SetHalfEdgePair(he0);
            SetHalfEdgePair(he1);
            SetHalfEdgePair(he2);
        }
    }
}


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

        static void Test002()
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



        static void Main(string[] args)
        {
            //Test001();
            Test002();
            Console.ReadLine();
        }
    }
}
