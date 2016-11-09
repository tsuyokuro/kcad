using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Plotter
{

    [Serializable]
    public class CadObjectDB
    {
        #region "Manage Layer"

        private uint mCurrentLayerID = 0;

        public uint CurrentLayerID
        {
            get { return mCurrentLayerID; }
            set { mCurrentLayerID = value; }
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


        public CadLayer getLayer(uint id)
        {
            return mLayerIdMap[id];
        }

        public CadLayer newLayer()
        {
            CadLayer layer = new CadLayer();
            addLayer(layer);
            return layer;
        }

        public uint addLayer(CadLayer layer)
        {
            layer.ID = mLayerIdProvider.getNew();
            mLayerIdMap.Add(layer.ID, layer);
            return layer.ID;
        }

        public void relaseLayer(uint id)
        {
            mLayerIdMap.Remove(id);
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


        public CadFigure getFigure(uint id)
        {
            if (id == 0)
            {
                return null;
            }

            return mFigureIdMap[id];
        }

        public CadFigure newFigure(CadFigure.Types type)
        {
            CadFigure fig = new CadFigure(type);

            addFigure(fig);
            return fig;
        }

        public uint addFigure(CadFigure fig)
        {
            fig.ID = mFigIdProvider.getNew();
            mFigureIdMap.Add(fig.ID, fig);

            return fig.ID;
        }

        public void relaseFigure(uint id)
        {
            mFigureIdMap.Remove(id);
        }
        #endregion



        #region "Manage Relative point"
        private Dictionary<uint, CadRelativePoint> mRelPointIdMap = new Dictionary<uint, CadRelativePoint>();
        public Dictionary<uint, CadRelativePoint> RelPointMap
        {
            get
            {
                return mRelPointIdMap;
            }

            private set
            {
                mRelPointIdMap = value;
            }
        }

        private IdProvider mRelPointIdProvider = new IdProvider();
        public IdProvider RelPointIdProvider
        {
            get
            {
                return mRelPointIdProvider;
            }
        }

        public CadRelativePoint getRelPoint(uint id)
        {
            return mRelPointIdMap[id];
        }

        public CadRelativePoint newRelPoint()
        {
            CadRelativePoint rp = new CadRelativePoint();
            addRelPoint(rp);
            return rp;
        }

        public uint addRelPoint(CadRelativePoint rp)
        {
            rp.ID = mRelPointIdProvider.getNew();
            mRelPointIdMap.Add(rp.ID, rp);
            return rp.ID;
        }

        public void relaseRelPoint(uint id)
        {
            mLayerIdMap.Remove(id);
        }
        #endregion




        public JObject ToJson()
        {
            JObject root = new JObject();

            root.Add("DataType", "CadObjectDB");
            root.Add("version", "1.0");

            root.Add("layer_id_counter", LayerIdProvider.Counter);
            root.Add("fig_id_counter", FigIdProvider.Counter);
            root.Add("relpoint_id_counter", RelPointIdProvider.Counter);

            root.Add("fig_map", JsonUtil.DictToJsonList(FigureMap));
            root.Add("relpt_map", JsonUtil.DictToJsonList(RelPointMap));
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
            RelPointIdProvider.Counter = (uint)jo["relpoint_id_counter"];

            JArray ja;

            ja = (JArray)jo["fig_map"];
            FigureMap = JsonUtil.JsonListToDictionary<CadFigure>(ja);


            ja = (JArray)jo["relpt_map"];
            RelPointMap = JsonUtil.JsonListToDictionary<CadRelativePoint>(ja);


            ja = (JArray)jo["layer_map"];
            LayerMap = JsonUtil.JsonListToDictionary<CadLayer>(this, ja);


            ja = (JArray)jo["layer_id_list"];
            List<uint> layerOrder = JsonUtil.JsonIdListToList(ja);

            LayerList = DUtil.IdListToObjList(layerOrder, LayerMap);

            CurrentLayerID = (uint)jo["current_layer_id"];

            ja = (JArray)jo["group_info"];
            GroupInfoFromJson(ja);
        }

        public void GroupInfoFromJson(JArray ja)
        {
            foreach (JObject jo in ja)
            {
                uint id = (uint)jo["id"];
                CadFigure fig = getFigure(id);

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

            {
                List<uint> ids = new List<uint>(mFigureIdMap.Keys);

                dout.println("Figure map {");
                dout.Indent++;
                foreach (uint id in ids)
                {
                    CadFigure fig = mFigureIdMap[id];
                    fig.dump(dout);
                }
                dout.Indent--;
                dout.println("}");
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

        public static JArray ListToJsonList<T>(List<T> list)
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