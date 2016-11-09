using System.Collections.Generic;

namespace Plotter
{
    using Newtonsoft.Json.Linq;
    using System;

    [Serializable]
    public class CadLayer
    {
        private uint mId = 0;

        public uint ID {
            get { return mId; }
            set { mId = value; }
        }

        private String mName = null;

        public String Name
        {
            get
            {
                if (mName != null)
                {
                    return mName;
                }

                return "Layer-" + mId;
            }

            set
            {
                mName = value;
            }
        }

        private List<CadFigure> mFigureList = new List<CadFigure>();
        private List<CadFigure> mStoreFigureList = null;

        private List<CadRelativePoint> mRelPointList = new List<CadRelativePoint>();
        private List<CadRelativePoint> mStoreRelPointList = null;


        public List<CadFigure> FigureList { get { return mFigureList; } }
        public List<CadFigure> StoreFigureList { get { return mStoreFigureList; } }

        public List<CadRelativePoint> RelPointList { get { return mRelPointList; } }
        public List<CadRelativePoint> StoreRelPointList { get { return mStoreRelPointList; } }


        public CadLayer()
        {
        }

        public void addFigure(CadFigure fig)
        {
            mFigureList.Add(fig);
        }

        public CadFigure getFigureByID(uint id)
        {
            return mFigureList.Find(fig => fig.ID == id);
        }

        public void insertFigure(int index, CadFigure fig)
        {
            mFigureList.Insert(index, fig);
        }

        public void removeFigureByID(CadObjectDB db, uint id)
        {
            CadFigure fig = db.getFigure(id);
            mFigureList.Remove(fig);
        }

        public void removeFigureByID(uint id)
        {
            int index = getFigureIndex(id);

            if (index < 0)
            {
                return;
            }

            mFigureList.RemoveAt(index);
        }

        public void removeFigureByIndex(int index)
        {
            Log.d("Layer#removeFigure index={0:d}", index);

            mFigureList.RemoveAt(index);
        }

        public int getFigureIndex(uint figID)
        {
            int index = 0;
            foreach (CadFigure fig in mFigureList)
            {
                if (fig.ID == figID)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void clearSelectedFlags()
        {
            List<CadFigure> figList = FigureList;

            foreach (CadFigure fig in figList)
            {
                fig.clearSelectFlags();
            }


            List<CadRelativePoint> rpList = RelPointList;

            foreach (CadRelativePoint rp in rpList)
            {
                rp.Selected = false;
            }
        }

        public JObject ToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);

            if (mName != null)
            {
                jo.Add("name", mName);
            }

            jo.Add("fig_id_list", JsonUtil.ListToJsonIdList(mFigureList));
            jo.Add("rel_point_id_list", JsonUtil.ListToJsonIdList(mRelPointList));

            return jo;
        }

        public void FromJson(CadObjectDB db, JObject jo)
        {
            ID = (uint)jo["id"];
            List<uint> idList;

            mName = (String)jo["name"];

            JArray ja;
                
            ja = (JArray)jo["fig_id_list"];
            idList = JsonUtil.JsonIdListToList(ja);
            mFigureList = DUtil.IdListToObjList(idList, db.FigureMap);

            ja = (JArray)jo["rel_point_id_list"];
            idList = JsonUtil.JsonIdListToList(ja);
            mRelPointList = DUtil.IdListToObjList(idList, db.RelPointMap);
        }

        public void sdump(DebugOut dout)
        {
            dout.println(
                this.GetType().Name + 
                "(" + this.GetHashCode().ToString() + ")" +
                "ID=" + ID.ToString());
        }

        public void dump(DebugOut dout)
        {
            dout.println(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            dout.Indent++;
            dout.println("ID=" + ID.ToString());

            foreach (CadFigure fig in FigureList)
            {
                fig.dump(dout);
            }

            dout.Indent--;
            dout.println("}");
        }
    }
}