using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Plotter
{
    public static class CadJson
    {
        public enum VersionCode
        {
            NULL = 0,
            VER_1_0_0_0 = 0x01_00_00_00,
            VER_1_0_0_1 = 0x01_00_00_01,
            VER_1_0_0_2 = 0x01_00_00_02,
        }

        public const VersionCode CurrentVersion = VersionCode.VER_1_0_0_2;

        private static Dictionary<string, VersionCode> VersionStrToCodeMap =
            new Dictionary<String, VersionCode>()
            {
                { "1.0", VersionCode.VER_1_0_0_0 },
                { "1.0.0.1", VersionCode.VER_1_0_0_1 },
                { "1.0.0.2", VersionCode.VER_1_0_0_2 },
            };

        private static Dictionary<VersionCode, string> VersionCodeToStrMap =
            new Dictionary<VersionCode, string>()
            {
                { VersionCode.VER_1_0_0_0, "1.0" },
                { VersionCode.VER_1_0_0_1, "1.0.0.1" },
                { VersionCode.VER_1_0_0_2, "1.0.0.2" },
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

            return "0";
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
            public const string VECTOR_LIST = "vlist";
        }

        public static class VECTOR
        {
            public const string TYPE = "type";
            public const string POINT_LIST = "point_list";
            public const string FLAGS = "flags";

            // VER_1_0_0_1以上用
            public const string V = "v";

            // VER_1_0_0_1未満用
            // VER_1_0_0_1から v: [10,20,30] のように配列で保存
            public const string X = "x";
            public const string Y = "y";
            public const string Z = "z";
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

                root.Add(DB.FIG_MAP, FigMapToJson(db.FigureMap, version));
                root.Add(DB.LAYER_MAP, LayerMapToJson(db.LayerMap, version));

                root.Add(DB.LAYER_ID_LIST, LayerIdListToJson(db.LayerList));

                root.Add(DB.CURRENT_LAYER_ID, db.CurrentLayerID);

                root.Add(DB.GROUP_INFO, GroupInfoToJson(db, version));

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

            public static JObject FigToJson(CadFigure fig, VersionCode version)
            {
                JObject jo = new JObject();

                jo.Add(COMMON.ID, fig.ID);
                jo.Add(FIG.TYPE, (byte)fig.Type);
                jo.Add(FIG.CLOSED, fig.IsLoop);
                jo.Add(FIG.LOCKED, fig.Locked);
                jo.Add(FIG.NORMAL, VectorToJson(fig.Normal, version));
                jo.Add(FIG.THICKNESSS, fig.Thickness);

                JObject jvl = VectorListToJson(fig.PointList, version);

                jo.Add(FIG.VECTOR_LIST, jvl);

                return jo;
            }

            public static JObject VectorListToJson(VectorList vl, VersionCode version)
            {
                JObject jvl = new JObject();

                JArray ja = new JArray();

                vl.ForEach(v =>
                {
                    ja.Add(VectorToJson(v, version));
                });

                jvl.Add(VECTOR.POINT_LIST, ja);

                return jvl;
            }

            public static JObject VectorToJson(CadVector v, VersionCode version)
            {
                var jo = new JObject();

                jo.Add(VECTOR.FLAGS, v.Flag);

                if (version == VersionCode.VER_1_0_0_0)
                {
                    jo.Add(VECTOR.X, v.x);
                    jo.Add(VECTOR.Y, v.y);
                    jo.Add(VECTOR.Z, v.z);
                }
                else if (version >= VersionCode.VER_1_0_0_1)
                {
                    JArray va = new JArray();

                    va.Add(v.x);
                    va.Add(v.y);
                    va.Add(v.z);
                    jo.Add(VECTOR.V, va);
                }

                return jo;
            }

            public static JObject FigListToJsonForClipboard(List<CadFigure> figList, VersionCode version = CurrentVersion)
            {
                JArray ja = new JArray();


                foreach (CadFigure fig in figList)
                {
                    ja.Add(FigToJsonForClipboard(fig, version));
                }

                JObject jo = new JObject();

                jo.Add(CLIPBOARD.FIG_LIST, ja);

                return jo;
            }

            public static JObject FigToJsonForClipboard(CadFigure fig, VersionCode version = CurrentVersion)
            {
                JObject jo = new JObject();

                jo.Add(COMMON.ID, fig.ID);
                jo.Add(FIG.TYPE, (byte)fig.Type);
                jo.Add(FIG.CLOSED, fig.IsLoop);
                jo.Add(FIG.LOCKED, fig.Locked);
                jo.Add(FIG.NORMAL, VectorToJson(fig.Normal, version));
                jo.Add(FIG.THICKNESSS, fig.Thickness);

                JObject jvl = VectorListToJson(fig.PointList, version);

                jo.Add(FIG.VECTOR_LIST, jvl);

                JArray jchildArray = new JArray();

                if (fig.ChildList != null)
                {
                    foreach (CadFigure child in fig.ChildList)
                    {
                        JObject jchild = FigToJsonForClipboard(child, version);

                        jchildArray.Add(jchild);
                    }
                }

                jo.Add(CLIPBOARD.CHILD_LIST, jchildArray);

                return jo;
            }
        }

        public static class FromJson
        {
            public static CadObjectDB DbFromJson(JObject jo)
            {
                CadObjectDB db = new CadObjectDB();

                string sv;
                
                sv = (string)jo[DB.DATA_TYPE_KEY];

                if (sv==null || sv != DB.DATA_TYPE_VAL)
                {
                    return db;
                }

                sv = (string)jo[DB.VERSION_KEY];

                VersionCode version = ToVersionCode(sv);

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
                //fig.Type = (CadFigure.Types)(byte)jo[FIG.TYPE];
                fig.IsLoop = (bool)jo[FIG.CLOSED];
                fig.Locked = (bool)jo[FIG.LOCKED];

                fig.Normal = VectorFromJson((JObject)jo[FIG.NORMAL], version);

                fig.Thickness = jo.GetDouble(FIG.THICKNESSS, 0);

                VectorList vl = VectorListFromJson(jo, version);

                fig.SetPointList(vl);

                return fig;
            }

            public static VectorList VectorListFromJson(JObject jo, VersionCode version)
            {
                VectorList vl = new VectorList();

                if (version <= VersionCode.VER_1_0_0_1)
                {
                    JArray jpl = (JArray)jo[VECTOR.POINT_LIST];

                    if (jpl != null)
                    {
                        foreach (JObject jv in jpl)
                        {
                            vl.Add(VectorFromJson(jv, version));
                        }
                    }

                    return vl;
                }

                JObject jvl = (JObject)jo[FIG.VECTOR_LIST];

                JArray ja = (JArray)jvl[VECTOR.POINT_LIST];

                if (ja != null)
                {
                    foreach (JObject jv in ja)
                    {
                        vl.Add(VectorFromJson(jv, version));
                    }
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

                #region for old type
                /*
                 * 古いCadVectorは、下記のTypeを持っていたが
                 * 廃止した
                 * 
                 * public enum Types : byte
                 * {
                 *     STD = 0,
                 *     BREAK = 1,
                 *     HANDLE = 2,
                 * }
                 * 
                 * TypeがHANDLEの場合は、Handle flagをONにする
                 * 
                 */

                JToken jt;
                if (jo.TryGetValue(VECTOR.TYPE, out jt))
                {
                    byte type = (byte)jt;

                    if (type == 2)
                    {
                        v.IsHandle = true;
                    }
                }
                #endregion

                if (version == VersionCode.VER_1_0_0_0)
                {
                    v.x = (double)jo[VECTOR.X];
                    v.y = (double)jo[VECTOR.Y];
                    v.z = (double)jo[VECTOR.Z];
                }
                else if (version >= VersionCode.VER_1_0_0_1)
                {
                    JArray va = (JArray)jo[VECTOR.V];

                    if (va.Count >= 3)
                    {
                        v.x = (double)va[0];
                        v.y = (double)va[1];
                        v.z = (double)va[2];
                    }
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

            public static CadFigure FigFromJsonForClipboard(JObject jo, VersionCode version = CurrentVersion)
            {
                CadFigure.Types type = (CadFigure.Types)(byte)jo[FIG.TYPE];

                CadFigure fig = CadFigure.Create(type);

                fig.ID = (uint)jo[COMMON.ID];
                //fig.Type = (CadFigure.Types)(byte)jo[FIG.TYPE];
                fig.IsLoop = (bool)jo[FIG.CLOSED];
                fig.Locked = (bool)jo[FIG.LOCKED];

                fig.Normal = VectorFromJson((JObject)jo[FIG.NORMAL], version);
                fig.Thickness = jo.GetDouble(FIG.THICKNESSS, 0);

                VectorList vl = VectorListFromJson(jo, version);

                fig.SetPointList(vl);

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
        }
    }

}
