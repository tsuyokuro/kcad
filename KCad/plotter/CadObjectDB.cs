using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Plotter
{
    public struct FigureBelong
    {
        public CadLayer Layer;
        public int Index;
    }

    [Serializable]
    public class CadObjectDB
    {
        #region "Manage Layer"

        public uint CurrentLayerID
        {
            get
            {
                if (mCurrentLayer == null)
                {
                    return 0;
                }
                else
                {
                    return mCurrentLayer.ID;
                }
            }

            set
            {
                mCurrentLayer = GetLayer(value);
            }
        }

        private CadLayer mCurrentLayer;

        public CadLayer CurrentLayer
        {
            get
            {
                return mCurrentLayer;
            }

            set
            {
                mCurrentLayer = value;
            }
        }

        private Dictionary<uint, CadLayer> mLayerIdMap = new Dictionary<uint, CadLayer>();
        public Dictionary<uint, CadLayer> LayerMap
        {
            get
            {
                return mLayerIdMap;
            }

            private set
            {
                mLayerIdMap = value;
            }
        }


        private IdProvider mLayerIdProvider = new IdProvider();
        public IdProvider LayerIdProvider
        {
            get
            {
                return mLayerIdProvider;
            }
        }


        private List<CadLayer> mLayerList = new List<CadLayer>();
        public List<CadLayer> LayerList
        {
            get { return mLayerList; }

            private set
            {
                mLayerList = value;
            }
        }


        public CadLayer GetLayer(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            return mLayerIdMap[id];
        }

        public CadLayer NewLayer()
        {
            CadLayer layer = new CadLayer();
            AddLayer(layer);
            return layer;
        }

        public uint AddLayer(CadLayer layer)
        {
            layer.ID = mLayerIdProvider.getNew();
            mLayerIdMap.Add(layer.ID, layer);
            return layer.ID;
        }

        public void RemoveLayer(uint id)
        {
            mLayerIdMap.Remove(id);
            mLayerList.RemoveAll(a => a.ID == id);
        }

        public int LayerIndex(uint id)
        {
            int idx = 0;
            foreach (CadLayer layer in mLayerList)
            {
                if (layer.ID == id)
                {
                    return idx;
                }

                idx++;
            }

            return -1;
        }
        
        #endregion



        #region "Manage Figure"
        private Dictionary<uint, CadFigure> mFigureIdMap = new Dictionary<uint, CadFigure>();
        public Dictionary<uint, CadFigure> FigureMap
        {
            get
            {
                return mFigureIdMap;
            }

            private set
            {
                mFigureIdMap = value;
            }
        }

        IdProvider mFigIdProvider = new IdProvider();
        public IdProvider FigIdProvider
        {
            get
            {
                return mFigIdProvider;
            }
        }


        public CadFigure GetFigure(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            return mFigureIdMap[id];
        }

        public CadFigure NewFigure(CadFigure.Types type)
        {
            CadFigure fig = new CadFigure(type);

            AddFigure(fig);
            return fig;
        }

        public uint AddFigure(CadFigure fig)
        {
            fig.ID = mFigIdProvider.getNew();
            mFigureIdMap.Add(fig.ID, fig);

            return fig.ID;
        }

        public void RelaseFigure(uint id)
        {
            mFigureIdMap.Remove(id);
        }

        #endregion Manage Figure


        
        #region Walk

        public delegate void WalkFunction(CadLayer layer, CadFigure fig);

        public delegate bool LayerFilterFunction(CadLayer layer);

        public void Walk(WalkFunction walk, LayerFilterFunction layerFilter)
        {
            foreach (CadLayer layer in mLayerList)
            {
                if (layerFilter!=null && !layerFilter(layer))
                {
                    continue;
                }

                foreach (CadFigure fig in layer.FigureList)
                {
                    walk(layer, fig);
                }
            }
        }

        public static LayerFilterFunction EditableLayerFilter = (layer) =>
        {
            if (layer.Locked) return false;
            if (!layer.Visible) return false;

            return true;
        };

        public void WalkEditable(WalkFunction walk)
        {
            foreach (CadLayer layer in mLayerList)
            {
                if (!EditableLayerFilter(layer))
                {
                    continue;
                }

                foreach (CadFigure fig in layer.FigureList)
                {
                    walk(layer, fig);
                }
            }
        }

        #endregion Walk


        public JObject ToJson()
        {
            JObject root = new JObject();

            root.Add("DataType", "CadObjectDB");
            root.Add("version", "1.0");

            root.Add("layer_id_counter", LayerIdProvider.Counter);
            root.Add("fig_id_counter", FigIdProvider.Counter);

            root.Add("fig_map", JsonUtil.DictToJsonList(FigureMap));
            root.Add("layer_map", JsonUtil.DictToJsonList(LayerMap));
            root.Add("layer_id_list", JsonUtil.ListToJsonIdList(LayerList));

            root.Add("current_layer_id", CurrentLayerID);

            root.Add("group_info", GroupInfoToJson());

            return root;
        }

        public JArray GroupInfoToJson()
        {
            JArray ja = new JArray();

            List<uint> ids = new List<uint>(FigureMap.Keys);

            foreach (CadFigure fig in FigureMap.Values)
            {
                JObject jo = fig.GroupInfoToJson();
                ja.Add(jo);
            }

            return ja;
        }


        public void FromJson(JObject jo)
        {
            LayerIdProvider.Counter = (uint)jo["layer_id_counter"];
            FigIdProvider.Counter = (uint)jo["fig_id_counter"];

            JArray ja;

            ja = (JArray)jo["fig_map"];
            FigureMap = JsonUtil.JsonListToDictionary<CadFigure>(ja);


            ja = (JArray)jo["layer_map"];
            LayerMap = JsonUtil.JsonListToDictionary<CadLayer>(this, ja);


            ja = (JArray)jo["layer_id_list"];
            List<uint> layerOrder = JsonUtil.JsonIdListToList(ja);

            LayerList = DUtil.IdListToObjList(layerOrder, LayerMap);

            uint currentLayerID = (uint)jo["current_layer_id"];

            CurrentLayer = GetLayer(currentLayerID);

            ja = (JArray)jo["group_info"];
            GroupInfoFromJson(ja);
        }

        public void GroupInfoFromJson(JArray ja)
        {
            foreach (JObject jo in ja)
            {
                uint id = (uint)jo["id"];
                CadFigure fig = GetFigure(id);

                if (fig == null)
                {
                    Log.e("CadObjectDB#GroupInfoFromJson() invalid ID=" + id);
                    continue;
                }

                fig.GroupInfoFromJson(this, jo);
            }
        }


        #region "For debug"
        public void dump(DebugOut dout)
        {
            dout.println(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            dout.Indent++;

            {
                List<uint> ids = new List<uint>(mLayerIdMap.Keys);

                dout.println("Layer map {");
                dout.Indent++;
                foreach (uint id in ids)
                {
                    CadLayer layer = mLayerIdMap[id];
                    layer.sdump(dout);
                }
                dout.Indent--;
                dout.println("}");
            }

            {
                dout.println("Layer list {");
                dout.Indent++;
                foreach (CadLayer layer in mLayerList)
                {
                    layer.sdump(dout);
                }
                dout.Indent--;
                dout.println("}");
            }

            dumpFigureMap(dout);

            dout.Indent--;
            dout.println("}");
        }

        public void dumpFigureMap(DebugOut dout)
        {
            List<uint> ids = new List<uint>(mFigureIdMap.Keys);

            dout.println("Figure map {");
            dout.Indent++;
            foreach (uint id in ids)
            {
                CadFigure fig = mFigureIdMap[id];
                fig.Dump(dout, "fig");
            }
            dout.Indent--;
            dout.println("}");
        }

        #endregion
    }

    public static class DUtil
    {
        public static Dictionary<uint, T> ListToDict<T>(List<T> list)
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (T item in list)
            {
                dynamic d = item;
                dict.Add(d.ID, item);
            }

            return dict;
        }

        public static List<T> IdListToObjList<T>(List<uint> list, Dictionary<uint, T> dict)
        {
            var objList = new List<T>();

            foreach (uint id in list)
            {
                T obj = dict[id];

                if (obj == null) continue;

                objList.Add(obj);
            }

            return objList;
        }
    }

    public static class JsonUtil
    {
        public static JArray DictToJsonList<Tkey, TValue>(Dictionary<Tkey, TValue> map)
        {
            JArray ja = new JArray();

            List<Tkey> ids = new List<Tkey>(map.Keys);

            foreach (Tkey id in ids)
            {
                dynamic x = map[id];
                ja.Add(x.ToJson());
            }

            return ja;
        }

        public static JArray ListToJsonList<T>(IReadOnlyList<T> list)
        {
            JArray ja = new JArray();

            foreach (T item in list)
            {
                dynamic x = item;
                ja.Add(x.ToJson());
            }

            return ja;
        }

        public static JArray ListToJsonIdList<T>(List<T> list)
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

        public static List<T> JsonListToObjectList<T>(JArray ja) where T : new()
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

                d.FromJson(jo);

                obj = d;

                list.Add(obj);
            }

            return list;
        }

        public static List<T> JsonListToObjectList<T>(CadObjectDB db, JArray ja) where T : new()
        {
            List<T> list = new List<T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(db, jo);

                list.Add(obj);
            }

            return list;
        }


        public static Dictionary<uint, T> JsonListToDictionary<T>(JArray ja) where T : new()
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(jo);

                dict.Add(d.ID, obj);
            }

            return dict;
        }

        public static Dictionary<uint, T> JsonListToDictionary<T>(CadObjectDB db, JArray ja) where T : new()
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(db, jo);

                dict.Add(d.ID, obj);
            }

            return dict;
        }
    }
}