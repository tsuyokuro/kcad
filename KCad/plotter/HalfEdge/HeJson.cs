using Newtonsoft.Json.Linq;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace HalfEdgeNS
{
    public class HeJson
    {
        public static JObject HeModelToJson(HeModel model)
        {
            JObject jmodel = new JObject();

            JArray jvs = CadJson.ToJson.VectorListToJson(model.VertexStore);
            jmodel.Add("vertex_store", jvs);

            JArray jns = CadJson.ToJson.VectorListToJson(model.NormalStore);
            jmodel.Add("normal_store", jns);

            jmodel.Add("half_edge_id_cnt", model.HeIdProvider.Counter);

            jmodel.Add("face_id_cnt", model.FaceIdProvider.Counter);

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

            jface.Add("id", face.ID);
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

            model.FaceIdProvider.Counter = (uint)jmodel["face_id_cnt"];


            ja = (JArray)jmodel["half_edge_list"];
            TempHalfEdgeDic heDic = CreateDictionary(ja);

            MakeHalfLinks(heDic);

            ja = (JArray)jmodel["face_store"];

            foreach (JObject jo in ja)
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

                he.Pair = t_he.pair_id != 0 ? dic[t_he.pair_id].mHalfEdge : null;
                he.Next = t_he.next_id != 0 ? dic[t_he.next_id].mHalfEdge : null;
                he.Prev = t_he.prev_id != 0 ? dic[t_he.prev_id].mHalfEdge : null;
            }
        }

        public static HeFace HeFaceFromJson(JObject jo, TempHalfEdgeDic dic)
        {
            uint he_id = (uint)jo["head_he_id"];

            HalfEdge he = dic[he_id].mHalfEdge;

            HeFace face = new HeFace(he);

            face.ID = (uint)jo["id"];

            face.Normal = CadJson.FromJson.GetIntValue(jo, "normal_idx", -1);

            return face;
        }
        #endregion
    }
}
