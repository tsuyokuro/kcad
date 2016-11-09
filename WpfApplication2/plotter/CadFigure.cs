using System;
using System.Collections.Generic;
using System.Drawing;


namespace Plotter
{
    using Newtonsoft.Json.Linq;
    using static CadFigure;

    [Serializable]
    public class CadFigure
    {
        public enum Types : byte
        {
            NONE,
            LINE,
            RECT,
            POLY_LINES,
            CIRCLE,
            GROUP,
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

        public List<CadPoint> PointList
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

        public List<CadPoint> StoreList
        {
            get
            {
                return mStoreList;
            }
        }
        #endregion

        private List<CadPoint> mPointList = new List<CadPoint>();

        private List<CadPoint> mStoreList = null;


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

        private CadFigureBehavior Behavior = null;

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
            switch (type)
            {
                case Types.LINE:
                    Behavior = new CadFigureLine(this);
                    break;
                case Types.RECT:
                    Behavior = new CadFigureRect(this);
                    break;
                case Types.POLY_LINES:
                    Behavior = new CadFigurePolyLines(this);
                    break;
                case Types.CIRCLE:
                    Behavior = new CadFigureCircle(this);
                    break;
                case Types.GROUP:
                    Behavior = new CadNopBehavior(this, type);
                    break;
                default:
                    Behavior = null;
                    break;
            }
        }

        public void copyPoints(CadFigure fig)
        {
            copyPointList(mPointList, fig.mPointList);
        }

        public void addPoints(List<CadPoint> points, int sp, int num)
        {
            for (int i = 0; i < num; i++)
            {
                CadPoint p = points[i + sp];
                addPoint(p);
            }
        }

        public void addPoints(List<CadPoint> points, int sp)
        {
            addPoints(points, sp, points.Count - sp);
        }

        public void addPoints(List<CadPoint> points)
        {
            foreach (CadPoint p in points)
            {
                addPoint(p);
            }
        }

        public void addPointsReverse(List<CadPoint> points)
        {
            int cnt = points.Count;
            int i = cnt - 1;

            for (; i >= 0; i--)
            {
                addPoint(points[i]);
            }
        }

        public void addPointsReverse(List<CadPoint> points, int sp)
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

        public CadPoint getPointAt(int index)
        {
            return mPointList[index];
        }

        public CadPoint setPointTypeAt(int index)
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
            if (mStoreList != null)
            {
                return;
            }

            mStoreList = new List<CadPoint>();
            mPointList.ForEach(a => mStoreList.Add(a));
        }

        public DiffData endEdit()
        {
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
            mPointList.Reverse();
        }

        public void addChild(CadFigure fig)
        {
            mChildList.Add(fig);
            fig.setParent(this);
        }

        public void releaseAllChildlen()
        {
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

            jo.Add("point_list", JsonUtil.ListToJsonList(PointList));
            return jo;
        }

        public void FromJson(JObject jo)
        {
            ID = (uint)jo["id"];
            Type = (Types)(byte)jo["type"];
            Closed = (bool)jo["closed"];

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
        #endregion

        #region "Behavior"
        public virtual States State
        {
            get
            {
                States st = Behavior.State;
                return st;
            }
        }

        public System.Type getBehaviorType()
        {
            return Behavior.GetType();
        }

        public void moveSelectedPoints(CadPoint delta)
        {
            Behavior.moveSelectedPoint(delta);
        }

        public void moveAllPoints(CadPoint delta)
        {
            Behavior.moveAllPoints(delta);
        }

        public void addPoint(CadPoint p)
        {
            Behavior.addPoint(p);
        }

        public void removeSelected()
        {
            Behavior.removeSelected();
        }

        public void setPointAt(int index, CadPoint pt)
        {
            Behavior.setPointAt(index, pt);
        }

        public void draw(DrawContext dc, Pen pen)
        {
            Behavior.draw(dc, pen);
        }

        public void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
            Behavior.drawSeg(dc, pen, idxA, idxB);
        }

        public void drawSelected(DrawContext dc, Pen pen)
        {
            Behavior.drawSelected(dc, pen);
        }

        public void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            Behavior.drawTemp(dc, tp, pen);
        }

        public void startCreate()
        {
            Behavior.startCreate();
        }

        public Types endCreate()
        {
            return Behavior.endCreate();
        }

        public CadRect getContainsRect()
        {
            return Behavior.getContainsRect();
        }
        #endregion

        #region "Utilities"
        private static void copyPointList(List<CadPoint> dest, List<CadPoint> src)
        {
            foreach (CadPoint p in src)
            {
                dest.Add(p);
            }
        }
        #endregion
    }

    [Serializable]
    public abstract class CadFigureBehavior
    {
        protected CadFigure Fig;

        public CadFigureBehavior(CadFigure fig)
        {
            Fig = fig;
        }

        public abstract States State { get; }
        public abstract void addPoint(CadPoint p);
        public abstract void setPointAt(int index, CadPoint pt);
        public abstract void removeSelected();
        public abstract void draw(DrawContext dc, Pen pen);
        public abstract void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB);
        public abstract void drawSelected(DrawContext dc, Pen pen);
        public abstract void drawTemp(DrawContext dc, CadPoint tp, Pen pen);
        public abstract void startCreate();
        public abstract Types endCreate();

        public virtual void moveSelectedPoint(CadPoint delta)
        {
            for (int i = 0; i < Fig.StoreList.Count; i++)
            {
                CadPoint op = Fig.StoreList[i];

                if (!op.Selected)
                {
                    continue;
                }

                if (i < Fig.PointList.Count)
                {
                    Fig.PointList[i] = op + delta;
                }
            }
        }

        public virtual void moveAllPoints(CadPoint delta)
        {
            CadUtil.movePoints(Fig.PointList, delta);
        }

        public virtual CadRect getContainsRect()
        {
            return CadUtil.getContainsRect(Fig.PointList);
        }
    }

    public class CadNopBehavior : CadFigureBehavior
    {
        CadFigure.Types Type;

        public CadNopBehavior(CadFigure fig, Types t) : base(fig)
        {
            Type = t;
        }

        public override States State
        {
            get
            {
                return States.NONE;
            }
        }

        public override void addPoint(CadPoint p)
        {
        }

        public override void draw(DrawContext dc, Pen pen)
        {
        }

        public override void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
        }

        public override void drawSelected(DrawContext dc, Pen pen)
        {
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
        }

        public override Types endCreate()
        {
            return Type;
        }

        public override void removeSelected()
        {
        }

        public override void setPointAt(int index, CadPoint pt)
        {
        }

        public override void startCreate()
        {
        }
    }


    [Serializable]
    public class CadFigurePolyLines : CadFigureBehavior
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 2)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count == 2)
                {
                    return States.ENOUGH;
                }
                else if (Fig.PointList.Count > 2)
                {
                    return States.CONTINUE;
                }

                return States.NONE;
            }
        }

        public CadFigurePolyLines(CadFigure fig) : base(fig)
        {
        }

        public override void removeSelected()
        {
            Fig.PointList.RemoveAll(a => a.Selected);

            if (Fig.PointCount < 2)
            {
                Fig.PointList.Clear();
            }
        }

        public override void addPoint(CadPoint p)
        {
            Fig.PointList.Add(p);
        }

        public override void draw(DrawContext dc, Pen pen)
        {
            drawLines(dc, pen);
        }

        public override void drawSelected(DrawContext dc, Pen pen)
        {
            drawSelected_Lines(dc, pen);
        }

        public override void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
            CadPoint a = Fig.PointList[idxA];
            CadPoint b = Fig.PointList[idxB];

            Drawer.drawLine(dc, pen, a, b);
        }

        protected void drawLines(DrawContext dc, Pen pen)
        {
            List<CadPoint> pl = Fig.PointList;

            if (pl.Count <= 0)
            {
                return;
            }

            CadPoint a;
            CadPoint b;

            int i = 0;
            a = pl[i];

            // If the count of point is 1, draw + mark.  
            if (pl.Count == 1)
            {
                Drawer.drawCross(dc, pen, a, 2);
                if (a.Selected)
                {
                    Drawer.drawHighlitePoint(dc, a);
                }

                return;
            }

            for (; true;)
            {
                if (i + 3 < pl.Count)
                {
                    if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                        pl[i + 2].Type == CadPoint.Types.HANDLE)
                    {
                        Drawer.drawBezier(dc, pen,
                            pl[i], pl[i + 1], pl[i + 2], pl[i + 3]);

                        i += 3;
                        a = pl[i];
                        continue;
                    }
                    else if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                        pl[i + 2].Type == CadPoint.Types.STD)
                    {
                        Drawer.drawBezier(dc, pen,
                            pl[i], pl[i + 1], pl[i + 2]);

                        i += 2;
                        a = pl[i];
                        continue;
                    }
                }

                if (i + 2 < pl.Count)
                {
                    if (pl[i + 1].Type == CadPoint.Types.HANDLE &&
                                            pl[i + 2].Type == CadPoint.Types.STD)
                    {
                        Drawer.drawBezier(dc, pen,
                            pl[i], pl[i + 1], pl[i + 2]);

                        i += 2;
                        a = pl[i];
                        continue;
                    }
                }

                if (i + 1 < pl.Count)
                {
                    b = pl[i + 1];
                    Drawer.drawLine(dc, pen, a, b);

                    a = b;
                    i++;

                    continue;
                }

                break;
            }

            if (Fig.Closed)
            {
                b = pl[0];
                Drawer.drawLine(dc, pen, a, b);
            }
        }

        private void drawSelected_Lines(DrawContext dc, Pen pen)
        {
            int i;
            int num = Fig.PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadPoint p = Fig.PointList[i];

                if (!p.Selected) continue;

                Drawer.drawSelectedPoint(dc, p);


                if (p.Type == CadPoint.Types.HANDLE)
                {
                    int idx = i + 1;

                    if (idx < Fig.PointCount)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type != CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type != CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }
                }
                else
                {
                    int idx = i + 1;

                    if (idx < Fig.PointCount)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type == CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }

                    idx = i - 1;

                    if (idx >= 0)
                    {
                        CadPoint np = Fig.getPointAt(idx);
                        if (np.Type == CadPoint.Types.HANDLE)
                        {
                            Drawer.drawLine(dc, dc.Tools.MatchSegPen, p, np);
                            Drawer.drawSelectedPoint(dc, np);
                        }
                    }
                }
            }
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            if (Fig.PointCount == 0)
            {
                return;
            }

            CadPoint lastPt = Fig.PointList[Fig.PointCount-1];

            Drawer.drawLine(dc, pen, lastPt, tp);
        }

        public override void setPointAt(int index, CadPoint pt)
        {
            Fig.PointList[index] = pt;
        }

        public override void startCreate()
        {
            // NOP
        }

        public override CadFigure.Types endCreate()
        {
            return Fig.Type;
        }
    }

    [Serializable]
    public class CadFigureLine : CadFigurePolyLines
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count < 2)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }
        }

        public CadFigureLine(CadFigure fig) : base(fig)
        {
        }

        public override void addPoint(CadPoint p)
        {
            Fig.PointList.Add(p);
        }

        public override CadFigure.Types endCreate()
        {
            Fig.Type = Types.POLY_LINES;
            return Fig.Type;
        }
    }

    [Serializable]
    public class CadFigureRect : CadFigurePolyLines
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count < 4)
                {
                    return States.WAIT_LAST_POINT;
                }
                return States.FULL;
            }
        }

        public CadFigureRect(CadFigure fig) : base(fig)
        {
        }

        public override void addPoint(CadPoint p)
        {
            if (Fig.PointList.Count == 0)
            {
                Fig.PointList.Add(p);
            }
            else
            {
                CadPoint p0 = Fig.PointList[0];
                CadPoint p2 = p;

                CadPoint p1 = p0;
                p1.x = p2.x;

                CadPoint p3 = p0;
                p3.y = p2.y;

                Fig.PointList.Add(p1);
                Fig.PointList.Add(p2);
                Fig.PointList.Add(p3);

                Fig.Closed = true;
            }
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            if (Fig.PointList.Count <= 0)
            {
                return;
            }

            Drawer.drawRect(dc, pen, Fig.PointList[0], tp);
        }

        public override CadFigure.Types endCreate()
        {
            Fig.Type = Types.POLY_LINES;
            return Fig.Type;
        }
    }

    [Serializable]
    public class CadFigureCircle : CadFigureBehavior
    {
        public override States State
        {
            get
            {
                if (Fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }
                else if (Fig.PointList.Count < 2)
                {
                    return States.WAIT_LAST_POINT;
                }

                return States.FULL;
            }
        }

        public CadFigureCircle(CadFigure fig) : base(fig)
        {
        }

        public override void addPoint(CadPoint p)
        {
            p.Type = CadPoint.Types.BREAK;
            Fig.PointList.Add(p);
        }

        public override void setPointAt(int index, CadPoint pt)
        {
            pt.Type = CadPoint.Types.BREAK;
            Fig.PointList[index] = pt;
        }

        public override void removeSelected()
        {
            Fig.PointList.RemoveAll(a => a.Selected);

            if (Fig.PointCount < 2)
            {
                Fig.PointList.Clear();
            }
        }



        public override void draw(DrawContext dc, Pen pen)
        {
            drawCircle(dc, pen);
        }

        public override void drawSeg(DrawContext dc, Pen pen, int idxA, int idxB)
        {
            drawCircle(dc, pen);
        }

        public override void drawSelected(DrawContext dc, Pen pen)
        {
            drawSelected_Circle(dc, pen);
        }

        public override void drawTemp(DrawContext dc, CadPoint tp, Pen pen)
        {
            if (Fig.PointList.Count <= 0)
            {
                return;
            }

            CadPoint cp = Fig.PointList[0];


            Drawer.drawCircle(dc, pen, cp, tp);
        }



        private void drawCircle(DrawContext dc, Pen pen)
        {
            if (Fig.PointList.Count == 0)
            {
                return;
            }

            if (Fig.PointList.Count == 1)
            {
                Drawer.drawCross(dc, pen, Fig.PointList[0], 2);
                if (Fig.PointList[0].Selected) Drawer.drawSelectedPoint(dc, Fig.PointList[0]);
                return;
            }

            Drawer.drawCircle(dc, pen, Fig.PointList[0], Fig.PointList[1]);
        }

        private void drawSelected_Circle(DrawContext dc, Pen pen)
        {
            if (Fig.PointList[0].Selected) Drawer.drawSelectedPoint(dc, Fig.PointList[0]);
            if (Fig.PointList[1].Selected) Drawer.drawSelectedPoint(dc, Fig.PointList[1]);
        }

        public override void startCreate()
        {
            // NOP
        }

        public override Types endCreate()
        {
            return Fig.Type;
        }

        public override void moveSelectedPoint(CadPoint delta)
        {
            CadPoint cp = Fig.StoreList[0];
            CadPoint rp = Fig.StoreList[1];

            if (cp.Selected)
            {
                Fig.PointList[0] = cp + delta;
                Fig.PointList[1] = rp + delta;
                return;
            }

            Fig.PointList[1] = rp + delta;
        }
    }
}