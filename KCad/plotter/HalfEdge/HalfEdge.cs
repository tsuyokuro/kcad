
using MyCollections;
using Newtonsoft.Json.Linq;
using Plotter;
using System.Collections.Generic;

namespace HalfEdgeNS
{
    public class HeFace
    {
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

        public VectorList VertexStore;
        public FlexArray<HeFace> FaceStore;
        public VectorList NormalStore;

        public HeModel()
        {
            VertexStore = new VectorList(8);
            FaceStore = new FlexArray<HeFace>(6);
            NormalStore = new VectorList(8);
        }

        public HeModel(VectorList vectorList)
        {
            VertexStore = vectorList;
            FaceStore = new FlexArray<HeFace>(vectorList.Count);
            NormalStore = new VectorList(vectorList.Count);
        }

        public void Clear()
        {
            VertexStore.Clear();
            FaceStore.Clear();
            NormalStore.Clear();
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
            HeFace face = new HeFace(he0);
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
                HeFace face = FaceStore[i];

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
    }

    public class HeUtil
    {
        public static JObject HeModelToJson(HeModel model)
        {
            JObject jmodel = new JObject();

            JArray jvs = CadJson.ToJson.VectorListToJson(model.VertexStore);
            jmodel.Add("vertex_store", jvs);

            JArray jns = CadJson.ToJson.VectorListToJson(model.NormalStore);
            jmodel.Add("normal_store", jns);

            jmodel.Add("half_edge_id_cnt", model.HeIdProvider.Counter);

            List<HalfEdge> heList = model.GetHalfEdgeList();

            JArray jheList = new JArray();

            for (int i = 0; i < heList.Count; i++)
            {
                HalfEdge he = heList[i];
                JObject jhe = HalfEdgeToJson(he);

                jheList.Add(jhe);
            }

            jmodel.Add("half_edge_list", jheList);


            JArray jfaceList = new JArray();

            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace face = model.FaceStore[i];
                JObject jface = HeFaceToJson(face);

                jfaceList.Add(jface);
            }

            jmodel.Add("face_store", jfaceList);

            return jmodel;
        }

        public static JObject HeFaceToJson(HeFace face)
        {
            JObject jface = new JObject();

            jface.Add("head_he_id", face.Head.ID);
            jface.Add("normal_idx", face.Normal);

            return jface;
        }

        public static JObject HalfEdgeToJson(HalfEdge he)
        {
            JObject jhe = new JObject();

            jhe.Add("id", he.ID);
            jhe.Add("pair_id", he.Pair != null ? he.Pair.ID : 0);
            jhe.Add("next_id", he.Next != null ? he.Next.ID : 0);
            jhe.Add("prev_id", he.Prev != null ? he.Prev.ID : 0);
            jhe.Add("vertex_idx", he.Vertex);
            jhe.Add("face_idx", he.Face);
            jhe.Add("normal_idx", he.Normal);

            return jhe;
        }


        #region From JSON
        public class TempHalfEdgeDic : Dictionary<uint, TempHalfEdge>
        {
        }

        public class TempHalfEdge
        {
            public uint id;
            public uint pair_id;
            public uint next_id;
            public uint prev_id;

            public HalfEdge mHalfEdge;
        }

        public static HeModel HeModelFromJson(JObject jmodel, CadJson.VersionCode version)
        {
            HeModel model = new HeModel();

            JArray ja = (JArray)jmodel["vertex_store"];

            VectorList vlist = CadJson.FromJson.VectorListFromJson(ja, version);
            model.VertexStore = vlist;

            bool normalExist = false;
            ja = (JArray)jmodel["normal_store"];

            if (ja != null)
            {
                normalExist = true;
                VectorList nlist = CadJson.FromJson.VectorListFromJson(ja, version);
                model.NormalStore = nlist;
            }

            model.HeIdProvider.Counter = (uint)jmodel["half_edge_id_cnt"];

            ja = (JArray)jmodel["half_edge_list"];
            TempHalfEdgeDic heDic = CreateDictionary(ja);

            MakeHalfLinks(heDic);

            ja = (JArray)jmodel["face_store"];

            foreach(JObject jo in ja)
            {
                HeFace face = HeFaceFromJson(jo, heDic);
                model.FaceStore.Add(face);
            }

            if (!normalExist)
            {
                model.RecreateNormals();
            }

            return model;
        }

        public static TempHalfEdgeDic CreateDictionary(JArray ja)
        {
            TempHalfEdgeDic heDic = new TempHalfEdgeDic();

            foreach (JObject jo in ja)
            {
                TempHalfEdge he = TempHalfEdgeFromJson(jo);
                heDic.Add(he.id, he);
            }

            return heDic;
        }

        public static TempHalfEdge TempHalfEdgeFromJson(JObject jo)
        {
            TempHalfEdge he = new TempHalfEdge();

            he.id = (uint)jo["id"];
            he.pair_id = (uint)jo["pair_id"];
            he.next_id = (uint)jo["next_id"];
            he.prev_id = (uint)jo["prev_id"];

            he.mHalfEdge = new HalfEdge();
            he.mHalfEdge.ID = he.id;
            he.mHalfEdge.Vertex = (int)jo["vertex_idx"];
            he.mHalfEdge.Face = (int)jo["face_idx"];
            he.mHalfEdge.Normal = CadJson.FromJson.GetIntValue(jo, "normal_idx", -1);

            return he;
        }

        public static void MakeHalfLinks(TempHalfEdgeDic dic)
        {
            foreach (TempHalfEdge t_he in dic.Values)
            {
                HalfEdge he = t_he.mHalfEdge;

                he.Pair = t_he.pair_id!=0 ? dic[t_he.pair_id].mHalfEdge : null;
                he.Next = t_he.next_id!=0 ? dic[t_he.next_id].mHalfEdge : null;
                he.Prev = t_he.prev_id!=0 ? dic[t_he.prev_id].mHalfEdge : null;
            }
        }

        public static HeFace HeFaceFromJson(JObject jo, TempHalfEdgeDic dic)
        {
            uint he_id = (uint)jo["head_he_id"];

            HalfEdge he = dic[he_id].mHalfEdge;

            HeFace face = new HeFace(he);

            face.Normal = CadJson.FromJson.GetIntValue(jo, "normal_idx", -1);

            return face;
        }
        #endregion
    }
}
