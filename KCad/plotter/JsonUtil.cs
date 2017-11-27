using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Plotter
{
    public static class JsonUtil
    {
        public static JArray DictToJsonList<Tkey, TValue>(Dictionary<Tkey, TValue> map, uint version)
        {
            JArray ja = new JArray();

            List<Tkey> ids = new List<Tkey>(map.Keys);

            foreach (Tkey id in ids)
            {
                dynamic x = map[id];
                ja.Add(x.ToJson(version));
            }

            return ja;
        }

        public static JArray ListToJsonList<T>(IReadOnlyList<T> list, uint version)
        {
            JArray ja = new JArray();

            foreach (T item in list)
            {
                dynamic x = item;
                ja.Add(x.ToJson(version));
            }

            return ja;
        }

        public static JArray ListToJsonIdList<T>(List<T> list, uint version)
        {
            JArray ja = new JArray();

            foreach (T item in list)
            {
                dynamic x = item;
                ja.Add(x.ID);
            }

            return ja;
        }

        public static List<uint> JsonIdListToList(JArray ja)
        {
            List<uint> list = new List<uint>();

            foreach (uint id in ja)
            {
                list.Add(id);
            }

            return list;
        }

        public static List<T> JsonListToObjectList<T>(JArray ja, uint version) where T : new()
        {
            List<T> list = new List<T>();

            if (ja == null)
            {
                return list;
            }

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(jo, version);

                obj = d;

                list.Add(obj);
            }

            return list;
        }

        public static List<T> JsonListToObjectList<T>(CadObjectDB db, JArray ja, uint version) where T : new()
        {
            List<T> list = new List<T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(db, jo, version);

                list.Add(obj);
            }

            return list;
        }


        public static Dictionary<uint, T> JsonListToDictionary<T>(JArray ja, uint version) where T : new()
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(jo, version);

                dict.Add(d.ID, obj);
            }

            return dict;
        }

        public static Dictionary<uint, T> JsonListToDictionary<T>(CadObjectDB db, JArray ja, uint version) where T : new()
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(db, jo, version);

                dict.Add(d.ID, obj);
            }

            return dict;
        }
    }

    public class CadJson
    {
        public static uint VersionCode1_0 = 0x00010000;


        public static uint ToVersionCode(string sv)
        {
            if (sv == "1.0")
            {
                return VersionCode1_0;
            }

            return 0;
        }

        public static string ToVersionString(uint version)
        {
            if (version == VersionCode1_0)
            {
                return "1.0";
            }

            return "0";
        }


        public static JObject DbToJson(CadObjectDB db)
        {
            JObject root = new JObject();

            uint version = VersionCode1_0;

            root.Add("DataType", "CadObjectDB");
            root.Add("version", ToVersionString(version));

            root.Add("layer_id_counter", db.LayerIdProvider.Counter);
            root.Add("fig_id_counter", db.FigIdProvider.Counter);

            root.Add("fig_map", FigMapToJson(db.FigureMap, version));
            root.Add("layer_map", LayerMapToJson(db.LayerMap, version));

            root.Add("layer_id_list", LayerListToIDJArray(db.LayerList));

            root.Add("current_layer_id", db.CurrentLayerID);

            root.Add("group_info", GroupInfoToJson(db, version));

            return root;
        }

        public static JArray GroupInfoToJson(CadObjectDB db, uint version)
        {
            JArray ja = new JArray();

            List<uint> ids = new List<uint>(db.FigureMap.Keys);

            foreach (CadFigure fig in db.FigureMap.Values)
            {
                JObject jo = FigGroupInfoToJson(fig, version);

                if (jo == null)
                {
                    continue;
                }

                ja.Add(jo);
            }

            return ja;
        }

        public static JArray LayerListToIDJArray(List<CadLayer> list)
        {
            JArray ja = new JArray();

            list.ForEach(layer =>
            {
                ja.Add(layer.ID);
            });

            return ja;
        }

        public static JArray FigMapToJson(Dictionary<uint, CadFigure> dic, uint version)
        {
            JArray ja = new JArray();

            foreach (CadFigure fig in dic.Values)
            {
                JObject jo = FigToJson(fig, version);
                ja.Add(jo);
            }

            return ja;
        }

        public static JArray LayerMapToJson(Dictionary<uint, CadLayer> dic, uint version)
        {
            JArray ja = new JArray();

            foreach (CadLayer layer in dic.Values)
            {
                JObject jo = LayerToJson(layer, version);
                ja.Add(jo);
            }

            return ja;
        }

        public static JObject LayerToJson(CadLayer layer, uint version)
        {
            JObject jo = new JObject();

            jo.Add("id", layer.ID);

            if (layer.Name != null)
            {
                jo.Add("name", layer.Name);
            }

            jo.Add("visible", layer.Visible);
            jo.Add("locked", layer.Locked);

            JArray ja = new JArray();

            foreach (CadFigure fig in layer.FigureList)
            {
                ja.Add(fig.ID);
            }

            jo.Add("fig_id_list", ja);

            return jo;
        }

        public static JObject FigToJson(CadFigure fig, uint version)
        {
            JObject jo = new JObject();

            jo.Add("id", fig.ID);
            jo.Add("type", (byte)fig.Type);
            jo.Add("closed", fig.IsLoop);
            jo.Add("locked", fig.Locked);
            jo.Add("normal", VectorToJson(fig.Normal, version));

            JArray ja = new JArray();

            fig.PointList.ForEach(v =>
            {
                ja.Add(VectorToJson(v, version));
            });

            jo.Add("point_list", ja);

            return jo;
        }

        public static JObject VectorToJson(CadVector v, uint version)
        {
            var jo = new JObject();

            jo.Add("type", (byte)v.Type);
            jo.Add("flags", v.Flag);
            jo.Add("x", v.x);
            jo.Add("y", v.y);
            jo.Add("z", v.z);

            return jo;
        }

        public static JObject FigGroupInfoToJson(CadFigure fig, uint version)
        {
            if (fig.ChildList.Count == 0)
            {
                return null;
            }

            JObject jo = new JObject();

            jo.Add("id", fig.ID);

            JArray ja = new JArray();

            fig.ChildList.ForEach(c =>
            {
                ja.Add(c.ID);                
            });

            jo.Add("child_id_list", ja);

            return jo;
        }



        public static CadObjectDB DbFromJson(JObject jo)
        {
            CadObjectDB db = new CadObjectDB();

            string sv = (string)jo["version"];

            uint version = ToVersionCode(sv);

            db.LayerIdProvider.Counter = (uint)jo["layer_id_counter"];
            db.FigIdProvider.Counter = (uint)jo["fig_id_counter"];

            JArray ja;

            ja = (JArray)jo["fig_map"];
            db.FigureMap = JArrayToFigMap(ja, version);


            ja = (JArray)jo["layer_map"];
            db.LayerMap = JArrayToLayerMap(db, ja, version);


            ja = (JArray)jo["layer_id_list"];
            db.LayerList = LayerListFromIDJArray(db, ja);

            uint currentLayerID = (uint)jo["current_layer_id"];

            db.CurrentLayer = db.GetLayer(currentLayerID);

            ja = (JArray)jo["group_info"];
            GroupInfoFromJson(db, ja, version);

            return db;
        }

        public static void GroupInfoFromJson(CadObjectDB db, JArray ja, uint version)
        {
            foreach (JObject jo in ja)
            {
                uint id = (uint)jo["id"];
                CadFigure fig = db.GetFigure(id);

                if (fig == null)
                {
                    Log.e("CadObjectDB#GroupInfoFromJson() invalid ID=" + id);
                    continue;
                }

                FigGroupInfoFromJson(fig, db, jo, version);
            }
        }

        public static Dictionary<uint, CadFigure> JArrayToFigMap(JArray ja, uint version)
        {
            var figMap = new Dictionary<uint, CadFigure>();

            foreach (JObject jo in ja)
            {
                CadFigure fig = FigFromJson(jo, version);
                figMap.Add(fig.ID, fig);
            }

            return figMap;
        }

        public static Dictionary<uint, CadLayer> JArrayToLayerMap(CadObjectDB db, JArray ja, uint version)
        {
            var layerMap = new Dictionary<uint, CadLayer>();

            foreach (JObject jo in ja)
            {
                CadLayer layer = LayerFromJson(db, jo, version);
                layerMap.Add(layer.ID, layer);
            }

            return layerMap;
        }

        public static List<CadLayer> LayerListFromIDJArray(CadObjectDB db, JArray ja)
        {
            var layerList = new List<CadLayer>();

            foreach (JValue val in ja)
            {
                uint id = (uint)val;

                CadLayer layer = db.GetLayer(id);

                layerList.Add(layer);
            }

            return layerList;
        }

        public static CadLayer LayerFromJson(CadObjectDB db, JObject jo, uint version)
        {
            CadLayer layer = new CadLayer();

            layer.ID = (uint)jo["id"];
            List<uint> idList;

            layer.Name = (String)jo["name"];

            if (jo["visible"] == null)
            {
                layer.Visible = true;
            }
            else
            {
                layer.Visible = (bool)jo["visible"];
            }

            layer.Locked = (bool)jo["locked"];

            JArray ja;

            ja = (JArray)jo["fig_id_list"];

            foreach (uint id in ja)
            {
                CadFigure fig = db.GetFigure(id);
                layer.AddFigure(fig);
            }

            return layer;
        }

        public static CadFigure FigFromJson(JObject jo, uint version)
        {
            CadFigure fig = new CadFigure();

            fig.ID = (uint)jo["id"];
            fig.Type = (CadFigure.Types)(byte)jo["type"];
            fig.IsLoop = (bool)jo["closed"];
            fig.Locked = (bool)jo["locked"];

            fig.Normal = VectorFromJson((JObject)jo["normal"], version);

            List<CadVector> list = new List<CadVector>();

            JArray ja = (JArray)jo["point_list"];

            if (ja != null)
            {
                foreach (JObject jv in ja)
                {
                    list.Add(VectorFromJson(jv, version));
                }
            }

            fig.SetPointList(list);

            return fig;
        }

        public static CadVector VectorFromJson(JObject jo, uint version)
        {
            CadVector v = default(CadVector);

            if (jo == null)
            {
                return v;
            }

            v.Type = (CadVector.Types)(byte)jo["type"];
            v.Flag = (byte)jo["flags"];
            v.x = (double)jo["x"];
            v.y = (double)jo["y"];
            v.z = (double)jo["z"];

            return v;
        }

        public static void FigGroupInfoFromJson(CadFigure fig, CadObjectDB db, JObject jo, uint version)
        {
            uint joid = (uint)jo["id"];

            JArray ja = (JArray)jo["child_id_list"];

            fig.ChildList.Clear();

            foreach (uint id in ja)
            {
                CadFigure c = db.GetFigure(id);
                c.Parent = fig;
                fig.ChildList.Add(c);
            }
        }
    }
}
