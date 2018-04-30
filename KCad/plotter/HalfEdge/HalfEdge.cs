
using MyCollections;
using CadDataTypes;
using Newtonsoft.Json.Linq;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;

namespace HalfEdgeNS
{
    public class HeFace
    {
        public uint ID;

        public HalfEdge Head; // HalfEdge link listの先頭

        public int Normal = HeModel.INVALID_INDEX;

        public HeFace(HalfEdge he)
        {
            Head = he;
        }
    }

    public class HalfEdge
    {
        public uint ID;

        public HalfEdge Pair;

        public HalfEdge Next;

        public HalfEdge Prev;

        public int Vertex = HeModel.INVALID_INDEX;

        // FaceのIndex(IDではない)
        public int Face = HeModel.INVALID_INDEX;

        public int Normal = HeModel.INVALID_INDEX;

        public HalfEdge(int vertex)
        {
            Vertex = vertex;
        }

        public HalfEdge()
        {
        }
    }

    public class HeModel
    {
        public const int INVALID_INDEX = -1;

        public IdProvider HeIdProvider = new IdProvider();

        public IdProvider FaceIdProvider = new IdProvider();

        public VectorList VertexStore;
        public FlexArray<HeFace> FaceStore;
        public VectorList NormalStore;

        public HeModel()
        {
            VertexStore = new VectorList(8);
            FaceStore = new FlexArray<HeFace>(6);
            NormalStore = new VectorList(8);
        }

        public void Clear()
        {
            VertexStore.Clear();
            FaceStore.Clear();
            NormalStore.Clear();
        }

        public static HeModel Create(CadMesh src)
        {
            HeModel m = new HeModel();

            m.VertexStore = src.VertexStore;

            for (int fi=0; fi<src.FaceStore.Count; fi++)
            {
                CadFace f = src.FaceStore[fi];

                int vi = f.VList[0];
                HalfEdge head = m.CreateHalfEdge(vi);
                HalfEdge current_he = head;

                HeFace face = m.CreateFace(head);
                int faceIndex = m.FaceStore.Add(face);

                current_he.Face = faceIndex;

                HalfEdge next_he;

                for (int pi=1; pi<f.VList.Count; pi++)
                {
                    vi = f.VList[pi];
                    next_he = m.CreateHalfEdge(vi);

                    current_he.Next = next_he;
                    next_he.Prev = current_he;

                    next_he.Face = faceIndex;

                    current_he = next_he;
                }

                head.Prev = current_he;
                current_he.Next = head;


                HalfEdge c = head;

                for (; ; )
                {
                    m.SetHalfEdgePair(c);

                    c = c.Next;
                    if (c == head) break;
                }
            }

            m.RecreateNormals();

            return m;
        }

        public void SetHalfEdgePair(HalfEdge he)
        {
            // すべてのFaceを巡回する
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                // Faceに含まれるHalfEdgeを巡回する
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

        public HalfEdge CreateHalfEdge(int vindex)
        {
            HalfEdge he = new HalfEdge(vindex);
            he.ID = HeIdProvider.getNew();
            return he;
        }

        public HeFace CreateFace(HalfEdge head)
        {
            HeFace face = new HeFace(head);
            face.ID = FaceIdProvider.getNew();
            return face;
        }

        // 三角形の追加
        // 左右回り方を統一して追加するようにする
        public void AddTriangle(int v0, int v1, int v2)
        {
            HalfEdge he0 = CreateHalfEdge(v0);
            HalfEdge he1 = CreateHalfEdge(v1);
            HalfEdge he2 = CreateHalfEdge(v2);

            he0.Next = he1;
            he0.Prev = he2;
            he1.Next = he2;
            he1.Prev = he0;
            he2.Next = he0;
            he2.Prev = he1;

            // 法線の設定
            CadVector normal = CadMath.Normal(VertexStore[v0], VertexStore[v1], VertexStore[v2]);
            int normalIndex = INVALID_INDEX;

            if (!normal.Invalid)
            {
                normalIndex = NormalStore.Add(normal);
            }

            he0.Normal = normalIndex;
            he1.Normal = normalIndex;
            he2.Normal = normalIndex;

            // Faceの設定
            HeFace face = CreateFace(he0);
            face.Normal = normalIndex;

            int faceIndex = FaceStore.Add(face);

            he0.Face = faceIndex;
            he1.Face = faceIndex;
            he2.Face = faceIndex;

            // Pairの設定
            SetHalfEdgePair(he0);
            SetHalfEdgePair(he1);
            SetHalfEdgePair(he2);
        }

        public List<HalfEdge> GetHalfEdgeList()
        {
            int tag = 0;

            List<HalfEdge> list = new List<HalfEdge>();

            // すべてのFaceを巡回する
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                // Faceに含まれるHalfEdgeを巡回する
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    list.Add(c);

                    c = c.Next;

                    if (c == head) break;
                }
            }

            return list;
        }

        public void RecreateNormals()
        {
            VectorList newNormalStore = new VectorList(VertexStore.Count);

            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HeFace face = FaceStore[i];

                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                CadVector n = CadMath.Normal(
                    VertexStore[c.Vertex],
                    VertexStore[c.Next.Vertex],
                    VertexStore[c.Next.Next.Vertex]
                    );

                int nidx = newNormalStore.Add(n);

                face.Normal = nidx;

                for (; ; )
                {
                    c.Normal = nidx;

                    c = c.Next;

                    if (c == head) break;
                }
            }

            NormalStore = newNormalStore;
        }

        public void InvertAllFace()
        {
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HeFace face = FaceStore[i];

                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    c.Next = c.Prev;
                    c.Prev = next;

                    c = next;
                    if (c == head) break;
                }
            }

            for (i = 0; i<NormalStore.Count; i++)
            {
                NormalStore[i] = -NormalStore[i];
            }
        }

        // 頂点番号に関連づいたFaceを削除
        public void RemoveVertexRelationFace(int vindex)
        {
            int[] indexMap = new int[FaceStore.Count];

            var rmFaceList = FindFaceAll(vindex);

            for (int i=0; i<rmFaceList.Count; i++)
            {
                int rmFace = rmFaceList[i];

                RemoveFaceLink(rmFace);
                indexMap[rmFace] = -1;
            }

            int p = 0;

            for (int i=0; i<FaceStore.Count; i++)
            {
                if (indexMap[i] == -1)
                {

                }
                else
                {
                    indexMap[i] = p;
                    p++;
                }
            }

            for (int i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    c.Face = indexMap[c.Face];

                    c = c.Next;
                    if (c == head) break;
                }
            }

            for (int i = indexMap.Length-1; i >= 0; i--)
            {
                if (indexMap[i] == -1)
                {
                    FaceStore.RemoveAt(i);
                }
            }
        }

        public void RemoveVertexs(List<int> idxList)
        {
            int[] indexMap = new int[VertexStore.Count];

            for (int i=0; i<idxList.Count; i++)
            {
                indexMap[idxList[i]] = -1;
            }

            int r = 0;

            for (int i = 0; i < VertexStore.Count; i++)
            {
                if (indexMap[i] != -1)
                {
                    indexMap[i] = r;
                    r++;
                }
            }

            ForEachHalfEdge(he =>
            {
                he.Vertex = indexMap[he.Vertex];
                if (he.Vertex == -1)
                {
                    DebugOut.println("HeModel.RemoveVertexs error. he.Vertex == -1");
                }
            });

            for (int i=VertexStore.Count-1; i>=0; i--)
            {
                if (indexMap[i] == -1)
                {
                    VertexStore.RemoveAt(i);
                }
            }
        }

        private void RemoveFaceLink(int idx)
        {
            HeFace face = FaceStore[idx];

            HalfEdge head = face.Head;
            HalfEdge c = head;

            for (; ; )
            {
                if (c.Pair != null)
                {
                    c.Pair.Pair = null;
                    c.Pair = null;
                }

                c = c.Next;
                if (c == head) break;
            }
        }

        public List<int> FindFaceAll(int vertexIndex)
        {
            var faceList = new List<int>();

            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    if (c.Vertex == vertexIndex)
                    {
                        faceList.Add(i);
                        break;
                    }

                    c = c.Next;
                    if (c == head) break;
                }
            }

            return faceList;
        }

        public FlexArray<int> GetOuterEdge()
        {
            // TODO 全てのHalfEdgeをチェックしているので遅い
            // HalfEdgeのリンクをたどる方式にいづれ変更する必要がある


            // Pairを持たないHalfEdgeのリストを作成
            List<HalfEdge> heList = new List<HalfEdge>();
            
            ForEachHalfEdge(he => {
                if (he.Pair == null)
                {
                    heList.Add(he);
                }
            });

            FlexArray<int> ret = new FlexArray<int>();

            if (heList.Count <= 1)
            {
                return ret;
            }

            int s = FindMaxDistantHalfEdge(CadVector.Zero, heList);

            if (s == -1)
            {
                DebugOut.println("HeModel.GetOuterEdge not found start HalfEdge");
                return ret;
            }


            int t = s;
            HalfEdge whe = heList[t];

            int vi = whe.Vertex;

            heList.RemoveAt(t);

            while (true)
            {
                ret.Add(vi);
                vi = whe.Next.Vertex;

                t = FindHalfEdge(vi, heList);

                if (t == -1)
                {
                    break;
                }

                whe = heList[t];
                heList.RemoveAt(t);
            }

            return ret;
        }

        public int FindHalfEdge(int idx, List<HalfEdge> list)
        {
            for (int i=0; i<list.Count; i++)
            {
                if (list[i].Vertex == idx)
                {
                    return i;
                }
            }

            return -1;
        }

        // 指定された座標から最も遠いHalfEdgeを求める
        public int FindMaxDistantHalfEdge(CadVector p0, List<HalfEdge> heList)
        {
            CadVector t;

            double maxd = 0;

            int ret = -1;

            for (int i=0; i<heList.Count; i++)
            {
                int vi = heList[i].Vertex;

                if (vi == -1)
                {
                    continue;
                }

                CadVector fp = VertexStore[vi];

                t = fp - p0;
                double d = t.Norm();

                if (d > maxd)
                {
                    maxd = d;
                    ret = i;
                }
            }

            return ret;
        }


        public void ForEachHalfEdge(Func<HalfEdge, bool> func)
        {
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    if (!func(c))
                    {
                        return;
                    }

                    c = c.Next;
                    if (c == head) break;
                }
            }
        }

        public void ForEachHalfEdge(Action<HalfEdge> action)
        {
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    action(c);

                    c = c.Next;
                    if (c == head) break;
                }
            }
        }

    }
}
