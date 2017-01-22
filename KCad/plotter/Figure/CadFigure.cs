//#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    using Newtonsoft.Json.Linq;
    using static CadFigure;

    [Serializable]
    public partial class CadFigure
    {
        #region Enums
        public enum Types : byte
        {
            NONE,
            LINE,
            RECT,
            POLY_LINES,
            CIRCLE,
            GROUP,
            MAX,
        }

        public enum States : byte
        {
            NONE,
            NOT_ENOUGH,
            ENOUGH,
            WAIT_LAST_POINT,
            CONTINUE,
            FULL,
        }
        #endregion

        #region  "public properties"
        public uint ID { get; set; }

        private Types mType;

        public Types Type {
            get
            {
                return mType;
            }

            set
            {
                mType = value;
                setBehavior(mType);
            }
        }

        public bool Closed { get; set; }

        public IReadOnlyList<CadPoint> PointList
        {
            get
            {
                return mPointList;
            }
        }

        public int PointCount
        {
            get
            {
                return mPointList.Count;
            }
        }

        public IReadOnlyList<CadPoint> StoreList
        {
            get
            {
                return mStoreList;
            }
        }

        public bool Locked  { set; get; } = false;

        public uint LayerID { set; get; } = 0;
        #endregion

        private List<CadPoint> mPointList = new List<CadPoint>();

        private List<CadPoint> mStoreList = null;


        #region Group management
        private CadFigure mParent = null;

        public CadFigure Parent
        {
            set { mParent = value; }
            get { return mParent; }
        }


        private List<CadFigure> mChildList = new List<CadFigure>();

        public List<CadFigure> ChildList
        {
            get
            {
                return mChildList;
            }
        }
        #endregion

        private CadFigureBehavior Behavior = null;

        private static CadFigureBehavior[] BehaviorTbl = null;

        // Static initializer
        static CadFigure()
        {
            BehaviorTbl = new CadFigureBehavior[(int)Types.MAX];
            BehaviorTbl[(int)Types.LINE] = new CadFigureLine();
            BehaviorTbl[(int)Types.RECT] = new CadFigureRect();
            BehaviorTbl[(int)Types.POLY_LINES] = new CadFigurePolyLines();
            BehaviorTbl[(int)Types.CIRCLE] = new CadFigureCircle();
            BehaviorTbl[(int)Types.GROUP] = new CadNopBehavior();
        }

        public CadFigure()
        {
            ID = 0;
            Closed = false;
            Type = Types.NONE;
        }

        public CadFigure(Types type)
        {
            ID = 0;
            Closed = false;
            Type = type;
        }

        private void setBehavior(Types type)
        {
            if (type > Types.NONE && type < Types.MAX)
            {
                Behavior = BehaviorTbl[(int)type];
            }
        }

        public void clearPoints()
        {
            mPointList.Clear();
        }

        public void copyPoints(CadFigure fig)
        {
            if (Locked) return;
            copyPointList(mPointList, fig.mPointList);
        }

        public void addPoints(IReadOnlyList<CadPoint> points, int sp, int num)
        {
            for (int i = 0; i < num; i++)
            {
                CadPoint p = points[i + sp];
                addPoint(p);
            }
        }

        public void addPoints(IReadOnlyList<CadPoint> points, int sp)
        {
            addPoints(points, sp, points.Count - sp);
        }

        public void addPoints(IReadOnlyList<CadPoint> points)
        {
            foreach (CadPoint p in points)
            {
                addPoint(p);
            }
        }

        public void addPointsReverse(IReadOnlyList<CadPoint> points)
        {
            int cnt = points.Count;
            int i = cnt - 1;

            for (; i >= 0; i--)
            {
                addPoint(points[i]);
            }
        }

        public void addPointsReverse(IReadOnlyList<CadPoint> points, int sp)
        {
            int cnt = points.Count;
            int i = cnt - 1 - sp;

            for (; i >= 0; i--)
            {
                addPoint(points[i]);
            }
        }

        public void insertPointAt(int index, CadPoint pt)
        {
            if (index >= mPointList.Count)
            {
                mPointList.Add(pt);
                return;
            }

            mPointList.Insert(index, pt);
        }

        public void removePointAt(int index)
        {
            if (mPointList == null)
            {
                return;
            }

            mPointList.RemoveAt(index);
        }

        public void removePointsRange(int index, int count)
        {
            mPointList.RemoveRange(index, count);
        }

        public void insertPointsRange(int index, IEnumerable<CadPoint> collection)
        {
            mPointList.InsertRange(index, collection);
        }

        public CadPoint getPointAt(int index)
        {
            return mPointList[index];
        }

        public void selectPointAt(int index, bool sel)
        {
            CadPoint p = mPointList[index];
            p.Selected = sel;
            mPointList[index] = p;
        }

        public void clearSelectFlags()
        {
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                selectPointAt(i, false);
            }
        }

        public void Select()
        {
            // Set select flag to all points
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                selectPointAt(i, true);
            }
        }

        public void startEdit()
        {
            if (Locked) return;

            if (mStoreList != null)
            {
                return;
            }

            mStoreList = new List<CadPoint>();
            mPointList.ForEach(a => mStoreList.Add(a));
        }

        public DiffData endEdit()
        {
            if (Locked) return null;

            DiffData diff = DiffData.create(this);
            mStoreList = null;
            return diff;
        }

        public List<CadPoint> getPointListCopy()
        {
            return new List<CadPoint>(mPointList);
        }

        public int findPoint(CadPoint t)
        {
            int i = 0;
            foreach (CadPoint p in mPointList)
            {
                if (t.coordEquals(p))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public void reversePointList()
        {
            if (Locked) return;

            mPointList.Reverse();
        }

        public void addChild(CadFigure fig)
        {
            if (Locked) return;

            mChildList.Add(fig);
            fig.setParent(this);
        }

        public void releaseAllChildlen()
        {
            if (Locked) return;

            foreach (CadFigure fig in mChildList)
            {
                fig.Parent = null;
            }

            mChildList.Clear();
        }

        #region "Group"
        public void SelectWithGroup()
        {
            CadFigure root = getGroupRoot();
            root.Select();
            root.SelectChildren();
        }

        public void SelectChildren()
        {
            foreach (CadFigure fig in mChildList)
            {
                fig.Select();
                fig.SelectChildren();
            }
        }

        public void setParent(CadFigure fig)
        {
            mParent = fig;
        }

        public CadFigure getParent()
        {
            return mParent;
        }

        public CadFigure getGroupRoot()
        {
            CadFigure fig = this;
            CadFigure parent = null;

            while (fig != null)
            {
                parent = fig.getParent();

                if (parent == null)
                {
                    break;
                }

                fig = parent;
            }

            return fig;
        }
        #endregion

        #region "JSON"
        public JObject ToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);
            jo.Add("type", (byte)Type);
            jo.Add("closed", Closed);
            jo.Add("locked", Locked);

            jo.Add("point_list", JsonUtil.ListToJsonList(PointList));
            return jo;
        }

        public void FromJson(JObject jo)
        {
            ID = (uint)jo["id"];
            Type = (Types)(byte)jo["type"];
            Closed = (bool)jo["closed"];
            Locked = (bool)jo["locked"];

            mPointList = JsonUtil.JsonListToObjectList<CadPoint>((JArray)jo["point_list"]);
        }


        public JObject GroupInfoToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);
            jo.Add("parent_id", mParent != null ? mParent.ID : 0);
            jo.Add("child_id_list", JsonUtil.ListToJsonIdList<CadFigure>(mChildList));

            return jo;
        }

        public void GroupInfoFromJson(CadObjectDB db, JObject jo)
        {
            uint joid = (uint)jo["id"];

            if (ID != joid)
            {
                Log.e("CadFigure#GroupInfoFromJson() invalid JObject. ID missmatch");
            }

            uint parentID = (uint)jo["parent_id"];
            List<uint> childList = JsonUtil.JsonIdListToList((JArray)jo["child_id_list"]);

            mParent = db.getFigure(parentID);

            mChildList.Clear();

            foreach (uint id in childList)
            {
                CadFigure fig = db.getFigure(id);
                mChildList.Add(fig);
            }
        }

        #endregion

        #region "Dump" 
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
            dout.println("LayerID=" + LayerID.ToString());
            dout.println("Type=" + Type.ToString());
            dout.println("State=" + State.ToString());

            dout.println("PointList [");
            dout.Indent++;
            foreach (CadPoint point in PointList)
            {
                point.dump(dout);
            }
            dout.Indent--;
            dout.println("]");


            dout.println("ParentID=" + (mParent != null ? mParent.ID : 0));

            dout.println("Child [");
            dout.Indent++;
            foreach (CadFigure fig in mChildList)
            {
                dout.println("" + fig.ID);
            }
            dout.Indent--;
            dout.println("]");

            dout.Indent--;
            dout.println("}");
        }

        public void dumpv(DebugOut dout, DrawContext dc)
        {
            dout.println(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            dout.Indent++;
            dout.println("ID=" + ID.ToString());
            dout.println("LayerID=" + LayerID.ToString());
            dout.println("Type=" + Type.ToString());
            dout.println("State=" + State.ToString());

            dout.println("PointList (View)[");
            dout.Indent++;
            foreach (CadPoint point in PointList)
            {
                CadPoint vp = dc.CadPointToUnitPoint(point);
                vp.dump(dout);
            }
            dout.Indent--;
            dout.println("]");


            dout.println("ParentID=" + (mParent != null ? mParent.ID : 0));

            dout.println("Child [");
            dout.Indent++;
            foreach (CadFigure fig in mChildList)
            {
                dout.println("" + fig.ID);
            }
            dout.Indent--;
            dout.println("]");

            dout.Indent--;
            dout.println("}");
        }
        #endregion

        #region "Behavior"
        public virtual States State
        {
            get
            {
                States st = Behavior.getState(this);
                return st;
            }
        }

        public System.Type getBehaviorType()
        {
            return Behavior.GetType();
        }

        public void moveSelectedPoints(CadPoint delta)
        {
            if (Locked) return;
            Log.d("moveSelectedPoints" + 
                " dx=" + delta.x.ToString() +
                " dy=" + delta.y.ToString() +
                " dz=" + delta.z.ToString()
                );
            Behavior.moveSelectedPoint(this, delta);
        }

        public void moveAllPoints(CadPoint delta)
        {
            if (Locked) return;

            Behavior.moveAllPoints(this, delta);
        }

        public void addPoint(CadPoint p)
        {
            Behavior.addPoint(this, p);
        }

        public void addPointInCreating(DrawContext dc, CadPoint p)
        {
            Behavior.addPointInCreating(this, dc, p);
        }

        public void removeSelected()
        {
            if (Locked) return;

            Behavior.removeSelected(this);
        }

        public void setPointAt(int index, CadPoint pt)
        {
            Behavior.setPointAt(this, index, pt);
        }

        public void draw(DrawContext dc, Pen pen)
        {
            Behavior.draw(this, dc, pen);
        }

        public void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
            Behavior.drawSeg(this, dc, pen, idxA, idxB);
        }

        public void drawSelected(DrawContext dc, Pen pen)
        {
            Behavior.drawSelected(this, dc, pen);
        }

        public void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            Behavior.drawTemp(this, dc, tp, pen);
        }

        public void startCreate()
        {
            Behavior.startCreate(this);
        }

        public Types endCreate()
        {
            return Behavior.endCreate(this);
        }

        public CadRect getContainsRect()
        {
            return Behavior.getContainsRect(this);
        }

        public IReadOnlyList<CadPoint> getPoints(int curveSplitNum)
        {
            return Behavior.getPoints(this, curveSplitNum);
        }

        public Centroid getCentroid()
        {
            return Behavior.getCentroid(this);
        }
        #endregion

        #region "Utilities"
        private static void copyPointList(List<CadPoint> dest, IReadOnlyList<CadPoint> src)
        {
            foreach (CadPoint p in src)
            {
                dest.Add(p);
            }
        }
        #endregion
    }
}