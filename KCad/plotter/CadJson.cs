using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Plotter
{
    public class CadJson
    {
        public enum VersionCode
        {
            NULL = 0,
            VER_1_0_0_0 = 0x01000000,
            VER_1_0_0_1 = 0x01000001,
        }

        public const VersionCode CurrentVersion = VersionCode.VER_1_0_0_1;

        // public static uint VersionCode1_0 = 0x00010000;


        public static VersionCode ToVersionCode(string sv)
        {
            if (sv == "1.0")
            {
                return VersionCode.VER_1_0_0_0;
            }
            else if (sv == "1.0.0.1")
            {
                return VersionCode.VER_1_0_0_1;
            }

            return VersionCode.NULL;
        }

        public static string ToVersionString(VersionCode version)
        {
            if (version == VersionCode.VER_1_0_0_0)
            {
                return "1.0";
            }
            else if (version == VersionCode.VER_1_0_0_1)
            {
                return "1.0.0.1";
            }

            return "0";
        }


        public static JObject DbToJson(CadObjectDB db)
        {
            JObject root = new JObject();

            VersionCode version = VersionCode.VER_1_0_0_1;

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

        public static JArray GroupInfoToJson(CadObjectDB db, VersionCode version)
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

        public static JObject FigGroupInfoToJson(CadFigure fig, VersionCode version)
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

        public static JArray LayerListToIDJArray(List<CadLayer> list)
        {
            JArray ja = new JArray();

            list.ForEach(layer =>
            {
                ja.Add(layer.ID);
            });

            return ja;
        }

        public static JArray FigMapToJson(Dictionary<uint, CadFigure> dic, VersionCode version)
        {
            JArray ja = new JArray();

            foreach (CadFigure fig in dic.Values)
            {
                JObject jo = FigToJson(fig, version);
                ja.Add(jo);
            }

            return ja;
        }

        public static JArray LayerMapToJson(Dictionary<uint, CadLayer> dic, VersionCode version)
        {
            JArray ja = new JArray();

            foreach (CadLayer layer in dic.Values)
            {
                JObject jo = LayerToJson(layer, version);
                ja.Add(jo);
            }

            return ja;
        }

        public static JObject LayerToJson(CadLayer layer, VersionCode version)
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

        public static JObject FigToJson(CadFigure fig, VersionCode version)
        {
            JObject jo = new JObject();

            jo.Add("id", fig.ID);
            jo.Add("type", (byte)fig.Type);
            jo.Add("closed", fig.IsLoop);
            jo.Add("locked", fig.Locked);
            jo.Add("normal", VectorToJson(fig.Normal, version));
            jo.Add("thickness", fig.Thickness);

            JArray ja = new JArray();

            fig.PointList.ForEach(v =>
            {
                ja.Add(VectorToJson(v, version));
            });

            jo.Add("point_list", ja);

            return jo;
        }

        public static JObject VectorToJson(CadVector v, VersionCode version)
        {
            var jo = new JObject();

            jo.Add("type", (byte)v.Type);
            jo.Add("flags", v.Flag);

            if (version == VersionCode.VER_1_0_0_0)
            {
                jo.Add("x", v.x);
                jo.Add("y", v.y);
                jo.Add("z", v.z);
            }
            else if (version == VersionCode.VER_1_0_0_1)
            {
                JArray va = new JArray();

                va.Add(v.x);
                va.Add(v.y);
                va.Add(v.z);
                jo.Add("v", va);
            }

            return jo;
        }

        public static CadObjectDB DbFromJson(JObject jo)
        {
            CadObjectDB db = new CadObjectDB();

            string sv = (string)jo["version"];

            VersionCode version = ToVersionCode(sv);

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

        public static void GroupInfoFromJson(CadObjectDB db, JArray ja, VersionCode version)
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

        public static void FigGroupInfoFromJson(CadFigure fig, CadObjectDB db, JObject jo, VersionCode version)
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

        public static Dictionary<uint, CadFigure> JArrayToFigMap(JArray ja, VersionCode version)
        {
            var figMap = new Dictionary<uint, CadFigure>();

            foreach (JObject jo in ja)
            {
                CadFigure fig = FigFromJson(jo, version);
                figMap.Add(fig.ID, fig);
            }

            return figMap;
        }

        public static Dictionary<uint, CadLayer> JArrayToLayerMap(CadObjectDB db, JArray ja, VersionCode version)
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

        public static CadLayer LayerFromJson(CadObjectDB db, JObject jo, VersionCode version)
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

        public static CadFigure FigFromJson(JObject jo, VersionCode version)
        {
            CadFigure fig = new CadFigure();

            fig.ID = (uint)jo["id"];
            fig.Type = (CadFigure.Types)(byte)jo["type"];
            fig.IsLoop = (bool)jo["closed"];
            fig.Locked = (bool)jo["locked"];

            fig.Normal = VectorFromJson((JObject)jo["normal"], version);

            fig.SetThickness(jo.GetDouble("thickness", 0));

            List <CadVector> list = new List<CadVector>();

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

        public static CadVector VectorFromJson(JObject jo, VersionCode version)
        {
            CadVector v = default(CadVector);

            if (jo == null)
            {
                return v;
            }

            v.Type = (CadVector.Types)(byte)jo["type"];
            v.Flag = (byte)jo["flags"];

            if (version == VersionCode.VER_1_0_0_0)
            {
                v.x = (double)jo["x"];
                v.y = (double)jo["y"];
                v.z = (double)jo["z"];
            }
            else if (version == VersionCode.VER_1_0_0_1)
            {
                JArray va = (JArray)jo["v"];

                if (va.Count >= 3)
                {
                    v.x = (double)va[0];
                    v.y = (double)va[1];
                    v.z = (double)va[2];
                }
            }

            return v;
        }

        public static JObject FigListToJsonForClipboard(List<CadFigure> figList, VersionCode version = CurrentVersion)
        {
            JArray ja = new JArray();


            foreach (CadFigure fig in figList)
            {
                ja.Add(FigToJsonForClipboard(fig, version));
            }

            JObject jo = new JObject();

            jo.Add("fig_list", ja);

            return jo;
        }

        public static JObject FigToJsonForClipboard(CadFigure fig, VersionCode version = CurrentVersion)
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

            JArray jchildArray = new JArray();

            if (fig.ChildList != null)
            {
                foreach (CadFigure child in fig.ChildList)
                {
                    JObject jchild = FigToJsonForClipboard(child, version);

                    jchildArray.Add(jchild);
                }
            }

            jo.Add("child_list", jchildArray);

            return jo;
        }

        public static List<CadFigure> FigListFromJsonForClipboard(JObject jo, VersionCode version = CurrentVersion)
        {
            List<CadFigure> figList = new List<CadFigure>();

            JArray ja =(JArray)jo["fig_list"];

            foreach (JObject jfig in ja)
            {
                figList.Add(
                    FigFromJsonForClipboard(jfig, version));
            }

            return figList;
        }

        public static CadFigure FigFromJsonForClipboard(JObject jo, VersionCode version = CurrentVersion)
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

            JArray jchildArray = (JArray)jo["child_list"];

            if (jchildArray != null)
            {
                foreach (JObject jchild in jchildArray)
                {
                    CadFigure child = FigFromJsonForClipboard(jchild, version);
                    fig.AddChild(child);
                }
            }

            return fig;
        }
    }

    public static class JsonExtends
    {
        public static double GetDouble(this JObject jo, string key, double defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            return (double)jt;
        }

    }
}
