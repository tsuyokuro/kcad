using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCollections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CadDataTypes;

namespace Plotter.Serializer
{
    public static class CadJson
    {
        public enum VersionCode
        {
            NULL = 0,
            VER_1_0_0_3 = 0x01_00_00_03,
        }

        public const VersionCode CurrentVersion = VersionCode.VER_1_0_0_3;

        private static Dictionary<string, VersionCode> VersionStrToCodeMap =
            new Dictionary<String, VersionCode>()
            {
                { "1.0.0.3", VersionCode.VER_1_0_0_3 },
            };

        private static Dictionary<VersionCode, string> VersionCodeToStrMap =
            new Dictionary<VersionCode, string>()
            {
                { VersionCode.NULL, null },
                { VersionCode.VER_1_0_0_3, "1.0.0.3" },
            };


        public static VersionCode ToVersionCode(string sv)
        {
            VersionCode ret;
            if (VersionStrToCodeMap.TryGetValue(sv, out ret))
            {
                return ret;
            }

            return VersionCode.NULL;
        }

        public static string ToVersionString(VersionCode version)
        {
            string ret;
            if (VersionCodeToStrMap.TryGetValue(version, out ret))
            {
                return ret;
            }

            return null;
        }

        public static class COMMON
        {
            public const string ID = "id";
            public const string NAME = "name";
        }

        public static class DB
        {
            public const string DATA_TYPE_KEY = "DataType";
            public const string DATA_TYPE_VAL = "CadObjectDB";

            public const string VERSION_KEY = "version";

            public const string LAYER_ID_COUNTER = "layer_id_counter";
            public const string FIG_ID_COUNTER = "fig_id_counter";
            public const string FIG_MAP = "fig_map";
            public const string LAYER_MAP = "layer_map";

            public const string LAYER_ID_LIST = "layer_id_list";
            public const string CURRENT_LAYER_ID = "current_layer_id";
            public const string GROUP_INFO = "group_info";
        }

        public static class LAYER
        {
            public const string FIG_ID_LIST = "fig_id_list";
        }

        public static class FIG
        {
            public const string TYPE = "type";
            public const string CHILD_ID_LIST = "child_id_list";
            public const string VISIBLE = "visible";
            public const string LOCKED = "locked";
            public const string CLOSED = "closed";
            public const string NORMAL = "normal";
            public const string THICKNESSS = "thickness";
            public const string VECTOR_DATA = "vdata";
        }

        public static class VECTOR
        {
            public const string TYPE = "type";
            public const string POINT_LIST = "point_list";
            public const string FLAGS = "flags";
            public const string V = "v";
        }

        public static class CLIPBOARD
        {
            public const string FIG_LIST = "fig_list";
            public const string CHILD_LIST = "child_list";
        }

        public static class ToJson
        {
            public static JObject DbToJson(CadObjectDB db)
            {
                JObject root = new JObject();

                VersionCode version = CurrentVersion;

                root.Add(DB.DATA_TYPE_KEY, DB.DATA_TYPE_VAL);
                root.Add(DB.VERSION_KEY, ToVersionString(version));

                root.Add(DB.LAYER_ID_COUNTER, db.LayerIdProvider.Counter);
                root.Add(DB.FIG_ID_COUNTER, db.FigIdProvider.Counter);

                root.Add(DB.FIG_MAP, FigMapToJson(db.FigureMap));
                root.Add(DB.LAYER_MAP, LayerMapToJson(db.LayerMap));

                root.Add(DB.LAYER_ID_LIST, LayerIdListToJson(db.LayerList));

                root.Add(DB.CURRENT_LAYER_ID, db.CurrentLayerID);

                root.Add(DB.GROUP_INFO, GroupInfoToJson(db));

                return root;
            }

            public static JArray GroupInfoToJson(CadObjectDB db)
            {
                JArray ja = new JArray();

                List<uint> ids = new List<uint>(db.FigureMap.Keys);

                foreach (CadFigure fig in db.FigureMap.Values)
                {
                    JObject jo = FigGroupInfoToJson(fig);

                    if (jo == null)
                    {
                        continue;
                    }

                    ja.Add(jo);
                }

                return ja;
            }

            public static JObject FigGroupInfoToJson(CadFigure fig)
            {
                if (fig.ChildList.Count == 0)
                {
                    return null;
                }

                JObject jo = new JObject();

                jo.Add(COMMON.ID, fig.ID);

                JArray ja = new JArray();

                fig.ChildList.ForEach(c =>
                {
                    ja.Add(c.ID);
                });

                jo.Add(FIG.CHILD_ID_LIST, ja);

                return jo;
            }

            public static JArray LayerIdListToJson(List<CadLayer> list)
            {
                JArray ja = new JArray();

                list.ForEach(layer =>
                {
                    ja.Add(layer.ID);
                });

                return ja;
            }

            public static JArray FigMapToJson(Dictionary<uint, CadFigure> dic)
            {
                JArray ja = new JArray();

                foreach (CadFigure fig in dic.Values)
                {
                    JObject jo = FigToJson(fig);
                    ja.Add(jo);
                }

                return ja;
            }

            public static JArray LayerMapToJson(Dictionary<uint, CadLayer> dic)
            {
                JArray ja = new JArray();

                foreach (CadLayer layer in dic.Values)
                {
                    JObject jo = LayerToJson(layer);
                    ja.Add(jo);
                }

                return ja;
            }

            public static JObject LayerToJson(CadLayer layer)
            {
                JObject jo = new JObject();

                jo.Add(COMMON.ID, layer.ID);

                if (layer.Name != null)
                {
                    jo.Add(COMMON.NAME, layer.Name);
                }

                jo.Add(FIG.VISIBLE, layer.Visible);
                jo.Add(FIG.LOCKED, layer.Locked);

                JArray ja = new JArray();

                foreach (CadFigure fig in layer.FigureList)
                {
                    ja.Add(fig.ID);
                }

                jo.Add(LAYER.FIG_ID_LIST, ja);

                return jo;
            }

            public static JObject FigToJson(CadFigure fig)
            {
                JObject jo = new JObject();

                jo.Add(COMMON.ID, fig.ID);
                jo.Add(FIG.TYPE, (byte)fig.Type);
                jo.Add(FIG.CLOSED, fig.IsLoop);
                jo.Add(FIG.LOCKED, fig.Locked);
                jo.Add(FIG.NORMAL, VectorToJson(fig.Normal));
                //jo.Add(FIG.THICKNESSS, fig.Thickness);

                JObject jvl = fig.GeometricDataToJson();

                jo.Add(FIG.VECTOR_DATA, jvl);

                return jo;
            }

            public static JObject FigToJsonForClipboard(CadFigure fig)
            {
                JObject jo = FigToJson(fig);


                // Clip boardには子Figureも含めて保存します
                // 参照だけでは、コピー後に参照先が修正されると、コピーした時点のデータと
                // 異なってしまうため
                JArray jchildArray = new JArray();

                if (fig.ChildList != null)
                {
                    foreach (CadFigure child in fig.ChildList)
                    {
                        JObject jchild = FigToJsonForClipboard(child);

                        jchildArray.Add(jchild);
                    }
                }

                jo.Add(CLIPBOARD.CHILD_LIST, jchildArray);

                return jo;
            }

            public static JArray VectorListToJson(VectorList vl)
            {
                JArray ja = new JArray();

                vl.ForEach(v =>
                {
                    ja.Add(VectorToJson(v));
                });

                return ja;
            }

            public static JObject VectorToJson(CadVector v)
            {
                var jo = new JObject();

                jo.Add(VECTOR.FLAGS, v.Flag);

                JArray va = new JArray();

                va.Add(v.x);
                va.Add(v.y);
                va.Add(v.z);
                jo.Add(VECTOR.V, va);

                return jo;
            }

            public static JObject FigListToJsonForClipboard(List<CadFigure> figList, VersionCode version = CurrentVersion)
            {
                JArray ja = new JArray();


                foreach (CadFigure fig in figList)
                {
                    ja.Add(FigToJsonForClipboard(fig));
                }

                JObject jo = new JObject();

                jo.Add(CLIPBOARD.FIG_LIST, ja);

                return jo;
            }

            public static JArray IntArrayToJson(FlexArray<int> array)
            {
                JArray ja = new JArray();

                for (int i=0;i<array.Count;i++)
                {
                    ja.Add(array[i]);
                }

                return ja;
            }

            public static JArray UintArrayToJson(FlexArray<uint> array)
            {
                JArray ja = new JArray();

                for (int i = 0; i < array.Count; i++)
                {
                    ja.Add(array[i]);
                }

                return ja;
            }
        }

        public static class FromJson
        {
            public static VersionCode VersionCodeFromJson(JObject jo)
            {
                string sv;

                sv = (string)jo[DB.DATA_TYPE_KEY];

                if (sv == null || sv != DB.DATA_TYPE_VAL)
                {
                    return VersionCode.NULL;
                }

                sv = (string)jo[DB.VERSION_KEY];

                VersionCode version = ToVersionCode(sv);

                return version;
            }

            public static string VersionStringFromJson(JObject jo)
            {
                VersionCode version = VersionCodeFromJson(jo);
                return ToVersionString(version);
            }

            public static CadObjectDB DbFromJson(JObject jo)
            {
                CadObjectDB db = new CadObjectDB();

                VersionCode version = VersionCodeFromJson(jo);

                if (version == VersionCode.NULL)
                {
                    return db;
                }

                db.LayerIdProvider.Counter = (uint)jo[DB.LAYER_ID_COUNTER];
                db.FigIdProvider.Counter = (uint)jo[DB.FIG_ID_COUNTER];

                JArray ja;

                ja = (JArray)jo[DB.FIG_MAP];
                db.FigureMap = JArrayToFigMap(ja, version);


                ja = (JArray)jo[DB.LAYER_MAP];
                db.LayerMap = JArrayToLayerMap(db, ja, version);


                ja = (JArray)jo[DB.LAYER_ID_LIST];
                db.LayerList = LayerListFromIDJArray(db, ja);

                uint currentLayerID = (uint)jo[DB.CURRENT_LAYER_ID];

                db.CurrentLayer = db.GetLayer(currentLayerID);

                ja = (JArray)jo[DB.GROUP_INFO];
                GroupInfoFromJson(db, ja, version);

                return db;
            }

            public static void GroupInfoFromJson(CadObjectDB db, JArray ja, VersionCode version)
            {
                foreach (JObject jo in ja)
                {
                    uint id = (uint)jo[COMMON.ID];
                    CadFigure fig = db.GetFigure(id);

                    if (fig == null)
                    {
                        continue;
                    }

                    FigGroupInfoFromJson(fig, db, jo, version);
                }
            }

            public static void FigGroupInfoFromJson(CadFigure fig, CadObjectDB db, JObject jo, VersionCode version)
            {
                uint joid = (uint)jo[COMMON.ID];

                JArray ja = (JArray)jo[FIG.CHILD_ID_LIST];

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

                layer.ID = (uint)jo[COMMON.ID];
                List<uint> idList;

                layer.Name = (String)jo[COMMON.NAME];

                if (jo[FIG.VISIBLE] == null)
                {
                    layer.Visible = true;
                }
                else
                {
                    layer.Visible = (bool)jo[FIG.VISIBLE];
                }

                layer.Locked = (bool)jo[FIG.LOCKED];

                JArray ja;

                ja = (JArray)jo[LAYER.FIG_ID_LIST];

                foreach (uint id in ja)
                {
                    CadFigure fig = db.GetFigure(id);
                    layer.AddFigure(fig);
                }

                return layer;
            }

            public static CadFigure FigFromJson(JObject jo, VersionCode version)
            {
                CadFigure.Types type = (CadFigure.Types)(byte)jo[FIG.TYPE];

                CadFigure fig = CadFigure.Create(type);

                fig.ID = (uint)jo[COMMON.ID];
                fig.IsLoop = (bool)jo[FIG.CLOSED];
                fig.Locked = (bool)jo[FIG.LOCKED];

                fig.Normal = VectorFromJson((JObject)jo[FIG.NORMAL], version);

                //fig.Thickness = jo.GetDouble(FIG.THICKNESSS, 0);

                JObject jvdata = (JObject)jo[FIG.VECTOR_DATA];

                fig.GeometricDataFromJson(jvdata, version);

                return fig;
            }

            public static CadFigure FigFromJsonForClipboard(JObject jo, VersionCode version)
            {
                CadFigure fig = FigFromJson(jo, version);


                // Clip boardには子Figureも含めて保存されているので
                // 取り出します
                JArray jchildArray = (JArray)jo[CLIPBOARD.CHILD_LIST];

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

            public static VectorList VectorListFromJson(JArray jarray, VersionCode version)
            {
                VectorList vl = new VectorList();

                foreach (JObject jv in jarray)
                {
                    vl.Add(VectorFromJson(jv, version));
                }

                return vl;
            }

            public static CadVector VectorFromJson(JObject jo, VersionCode version)
            {
                CadVector v = default(CadVector);

                if (jo == null)
                {
                    return v;
                }

                v.Flag = (byte)jo[VECTOR.FLAGS];

                JToken jtk = jo[VECTOR.V];

                JArray va = (JArray)jtk;
                if (va.Count >= 3)
                {
                    v.x = (double)va[0];
                    v.y = (double)va[1];
                    v.z = (double)va[2];
                }

                return v;
            }

            public static List<CadFigure> FigListFromJsonForClipboard(JObject jo, VersionCode version = CurrentVersion)
            {
                List<CadFigure> figList = new List<CadFigure>();

                JArray ja = (JArray)jo[CLIPBOARD.FIG_LIST];

                foreach (JObject jfig in ja)
                {
                    figList.Add(
                        FigFromJsonForClipboard(jfig, version));
                }

                return figList;
            }

            public static FlexArray<int> IntArrayFromJson(JArray ja)
            {
                FlexArray<int> list = new FlexArray<int>(ja.Count);
                
                for (int i = 0; i < ja.Count; i++)
                {
                    list.Add((int)ja[i]);
                }

                return list;
            }

            public static FlexArray<uint> UintArrayFromJson(JArray ja)
            {
                FlexArray<uint> list = new FlexArray<uint>(ja.Count);

                for (int i = 0; i < ja.Count; i++)
                {
                    list.Add((uint)ja[i]);
                }

                return list;
            }

            public static int GetIntValue(JObject jo, string name, int naValue)
            {
                JToken jv;

                if (jo.TryGetValue(name, out jv))
                {
                    return (int)jv;
                }

                return naValue;
            }

            public static uint GetUintValue(JObject jo, string name, uint naValue)
            {
                JToken jv;

                if (jo.TryGetValue(name, out jv))
                {
                    return (uint)jv;
                }

                return naValue;
            }
        }
    }

}
