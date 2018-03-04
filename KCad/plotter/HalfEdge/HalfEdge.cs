
using MyCollections;
using Plotter;

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


        public HalfEdge(int vertex)
        {
            Vertex = vertex;
        }
    }

    public class HeModel
    {
        public VectorList VertexStore;
        public AutoArray<HeFace> FaceStore;

        public HeModel()
        {
            VertexStore = new VectorList(8);
            FaceStore = new AutoArray<HeFace>(6);
        }

        public HeModel(VectorList vectorList)
        {
            VertexStore = vectorList;
            FaceStore = new AutoArray<HeFace>(vectorList.Capacity);
        }

        public void Clear()
        {
            VertexStore.Clear();
            FaceStore.Clear();
        }

        public void SetHalfEdgePair(HalfEdge he)
        {
            // すべてのFaceを巡回する
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                // 現在のFaceに含まれるハーフエッジを巡回する
                HalfEdge c = FaceStore[i].Head;

                for (; ; )
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
        public int AddVertexWithoutSame(CadVector v)
        {
            int cnt = VertexStore.Count;
            for (int i = 0; i < cnt; i++)
            {
                ref CadVector rv = ref VertexStore.Ref(i);
                if (v.Equals(rv))
                {
                    return i;
                }
            }

            return VertexStore.Add(v);
        }

        // 単純に頂点を追加
        public int AddVertex(CadVector v)
        {
            return VertexStore.Add(v);
        }

        // 三角形の追加
        // 左右回り方を統一して追加するようにする
        public void AddTriangle(CadVector v0, CadVector v1, CadVector v2)
        {
            AddTriangle(
                AddVertexWithoutSame(v0),
                AddVertexWithoutSame(v1),
                AddVertexWithoutSame(v2)
                );
        }

        // 三角形の追加
        // 左右回り方を統一して追加するようにする
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
