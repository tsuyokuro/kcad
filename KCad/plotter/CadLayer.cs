using System.Collections.Generic;

namespace Plotter
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;

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

                return "layer" + mId;
            }

            set
            {
                mName = value;
            }
        }

        private bool mLocked = false;

        public bool Locked
        {
            set
            {
                mLocked = value;
                mFigureList.ForEach(a => a.Locked = value);
            }

            get
            {
                return mLocked;
            }
        }

        private bool mVisible = true;

        public bool Visible
        {
            set
            {
                mVisible = value;
            }

            get
            {
                return mVisible;
            }
        }

        private List<CadFigure> mFigureList = new List<CadFigure>();



        public IReadOnlyList<CadFigure> FigureList
        {
            get
            {
                return mFigureList;
            }
        }


        public CadLayer()
        {
        }

        public void addFigure(CadFigure fig)
        {
            fig.LayerID = ID;
            mFigureList.Add(fig);
        }

        public CadFigure getFigureByID(uint id)
        {
            return mFigureList.Find(fig => fig.ID == id);
        }

        public void insertFigure(int index, CadFigure fig)
        {
            fig.LayerID = ID;
            mFigureList.Insert(index, fig);
        }

        public void removeFigureByID(CadObjectDB db, uint id)
        {
            CadFigure fig = db.getFigure(id);
            mFigureList.Remove(fig);
            fig.LayerID = 0;
        }

        public void removeFigureByID(uint id)
        {
            int index = getFigureIndex(id);

            if (index < 0)
            {
                return;
            }

            mFigureList[index].LayerID = 0;

            mFigureList.RemoveAt(index);
        }


        public void removeFigureByIndex(int index)
        {
            mFigureList[index].LayerID = 0;
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
            foreach (CadFigure fig in FigureList)
            {
                fig.ClearSelectFlags();
            }
        }

        public CadOpeList clear()
        {
            CadOpeList opeList = CadOpe.CreateListOpe();

            CadOpe ope;

            IEnumerable<CadFigure> figList = mFigureList;
            var revFig = figList.Reverse();

            foreach (CadFigure fig in revFig)
            {
                ope = CadOpe.CreateRemoveFigureOpe(this, fig.ID);
                opeList.OpeList.Add(ope);
            }

            mFigureList.Clear();

            return opeList;
        }

        public JObject ToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);

            if (mName != null)
            {
                jo.Add("name", mName);
            }

            jo.Add("visible", mVisible);
            jo.Add("locked", mLocked);

            jo.Add("fig_id_list", JsonUtil.ListToJsonIdList(mFigureList));

            return jo;
        }

        public void FromJson(CadObjectDB db, JObject jo)
        {
            ID = (uint)jo["id"];
            List<uint> idList;

            mName = (String)jo["name"];

            if (jo["visible"] == null)
            {
                mVisible = true;
            }
            else
            {
                mVisible = (bool)jo["visible"];
            }

            mLocked = (bool)jo["locked"];

            JArray ja;
                
            ja = (JArray)jo["fig_id_list"];
            idList = JsonUtil.JsonIdListToList(ja);

            List<CadFigure> figList = DUtil.IdListToObjList(idList, db.FigureMap);
            figList.ForEach(a => {
                    a.LayerID = ID;
                    mFigureList.Add(a);
                });
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
                fig.Dump(dout);
            }

            dout.Indent--;
            dout.println("}");
        }
    }
}