//#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Plotter
{
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
            POINT,
            GROUP,
            DIMENTION_LINE,
            SOLID,
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
                SetBehavior(mType);
            }
        }

        public bool IsLoop { get; set; }

        public CadVector Normal;

        private double mThickness = 0;

        public double Thickness
        {
            get
            {
                return mThickness;
            }

            private set
            {
                mThickness = value;
            }
        }

        public List<CadVector> PointList
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

        public IReadOnlyList<CadVector> StoreList
        {
            get
            {
                return mStoreList;
            }
        }

        public bool Locked  { set; get; } = false;

        public uint LayerID { set; get; } = 0;
        
        public bool Current { set; get; } = false;

        public int FontID { set; get; } = DrawTools.FONT_SMALL;

        public int BrushID { set; get; } = DrawTools.BRUSH_TEXT;

        #endregion

        private List<CadVector> mPointList = new List<CadVector>();

        private List<CadVector> mStoreList = null;


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

        public bool IsEmpty
        {
            get
            {
                return mPointList.Count == 0 && mChildList.Count == 0;
            }
        }


        /// <summary>
        /// 自分とその下にあるFigureを全て列挙(中止可能版)
        /// </summary>
        /// <param name="d"></param>
        /// <returns>true:列挙を継続</returns>
        public bool ForEachFig(ForEachDelegate<CadFigure> d)
        {
            int i;

            if (!d(this))
            {
                return false;
            }

            for (i=0; i< mChildList.Count; i++)
            {
                CadFigure c = mChildList[i];

                if (!c.ForEachFig(d))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 自分の下にあるFigureを全て列挙(中止可能版)
        /// </summary>
        /// <param name="d"></param>
        /// <returns>true:列挙を継続</returns>
        public bool ForEachNode(ForEachDelegate<CadFigure> d)
        {
            int i;
            for (i = 0; i < mChildList.Count; i++)
            {
                CadFigure c = mChildList[i];

                if (!c.ForEachFig(d))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// 自分とその下にあるFigureを全て列挙(中止不可版)
        /// </summary>
        /// <param name="d"></param>
        /// <returns>true:列挙を継続</returns>
        public void ForEachFig(Action<CadFigure> d)
        {
            d(this);

            int i;
            for (i = 0; i < mChildList.Count; i++)
            {
                CadFigure c = mChildList[i];
                c.ForEachFig(d);
            }
        }

        /// <summary>
        /// 自分の下にあるFigureを全て列挙(中止不可版)
        /// </summary>
        /// <param name="d"></param>
        /// <returns>true:列挙を継続</returns>
        public void ForEachNode(Action<CadFigure> d)
        {
            int i;
            for (i = 0; i < mChildList.Count; i++)
            {
                CadFigure c = mChildList[i];
                c.ForEachFig(d);
            }
        }

        #endregion

        private CadFigureBehavior Behavior = null;

        private static CadFigureBehavior[] BehaviorTbl = null;

        public CadFigure()
        {
            ID = 0;
            IsLoop = false;
            Type = Types.NONE;
        }

        public CadFigure(Types type)
        {
            ID = 0;
            IsLoop = false;
            Type = type;
        }

        private void SetBehavior(Types type)
        {
            if (type > Types.NONE && type < Types.MAX)
            {
                Behavior = NewBehavior(type);
            }
        }

        private static CadFigureBehavior NewBehavior(Types type)
        {
            switch (type)
            {
                case Types.LINE:
                    return new CadFigureLine();

                case Types.RECT:
                    return new CadFigureRect();

                case Types.POLY_LINES:
                    return new CadFigurePolyLines();

                case Types.CIRCLE:
                    return new CadFigureCircle();

                case Types.POINT:
                    return new CadFigurePoint();

                case Types.GROUP:
                    return new CadNopBehavior();

                case Types.DIMENTION_LINE:
                    return new CadFigureDimLine();

                default:
                    return null;
            }
        }

        public void ClearPoints()
        {
            mPointList.Clear();
        }

        public void CopyPoints(CadFigure fig)
        {
            if (Locked) return;
            mPointList.Clear();
            mPointList.AddRange(fig.mPointList);
        }

        public void AddPoints(IReadOnlyList<CadVector> points, int sp, int num)
        {
            for (int i = 0; i < num; i++)
            {
                CadVector p = points[i + sp];
                AddPoint(p);
            }
        }

        public void AddPoints(IReadOnlyList<CadVector> points, int sp)
        {
            AddPoints(points, sp, points.Count - sp);
        }

        public void AddPoints(IReadOnlyList<CadVector> points)
        {
            foreach (CadVector p in points)
            {
                AddPoint(p);
            }
        }

        public void AddPointsReverse(IReadOnlyList<CadVector> points)
        {
            int cnt = points.Count;
            int i = cnt - 1;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public void AddPointsReverse(IReadOnlyList<CadVector> points, int sp)
        {
            int cnt = points.Count;
            int i = cnt - 1 - sp;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public void InsertPointAt(int index, CadVector pt)
        {
            if (index >= mPointList.Count)
            {
                mPointList.Add(pt);
                return;
            }

            mPointList.Insert(index, pt);
        }

        public void RemovePointAt(int index)
        {
            if (mPointList == null)
            {
                return;
            }

            mPointList.RemoveAt(index);
        }

        public void RemovePointsRange(int index, int count)
        {
            mPointList.RemoveRange(index, count);
        }

        public void InsertPointsRange(int index, IEnumerable<CadVector> collection)
        {
            mPointList.InsertRange(index, collection);
        }

        public bool HasSelectedPoint()
        {
            int i;
            for (i=0; i<mPointList.Count; i++)
            {
                if (mPointList[i].Selected)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasSelectedPointInclueChild()
        {
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                if (mPointList[i].Selected)
                {
                    return true;
                }
            }

            if (ChildList == null)
            {
                return false;
            }

            for (i=0; i<ChildList.Count; i++)
            {
                CadFigure c = ChildList[i];
                if (c.HasSelectedPointInclueChild())
                {
                    return true;
                }
            }

            return false;
        }

        public CadVector GetPointAt(int index)
        {
            return  Behavior.GetPointAt(this, index);
        }

        public void SetPointAt(int index, CadVector pt)
        {
            Behavior.SetPointAt(this, index, pt);
        }

        public void SelectPointAt(int index, bool sel)
        {
            Behavior.SelectPointAt(this, index, sel);
        }

        public void ClearSelectFlags()
        {
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                SelectPointAt(i, false);
            }
        }

        public void Select()
        {
            // Set select flag to all points
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                SelectPointAt(i, true);
            }
        }

        public void StartEdit()
        {
            if (Locked) return;

            Behavior.StartEdit(this);

            if (mStoreList != null)
            {
                return;
            }

            mStoreList = new List<CadVector>();
            mPointList.ForEach(a => mStoreList.Add(a));
        }

        public DiffData EndEdit()
        {
            if (Locked) return null;

            Behavior.EndEdit(this);

            DiffData diff = DiffData.create(this);
            mStoreList = null;
            return diff;
        }

        public void CancelEdit()
        {
            if (Locked) return;

            Behavior.CancelEdit(this);

            if (mStoreList == null)
            {
                return;
            }

            mPointList.Clear();
            mStoreList.ForEach(a => mPointList.Add(a));
            mStoreList = null;
        }

        public List<CadVector> GetPointListCopy()
        {
            return new List<CadVector>(mPointList);
        }

        public int FindPoint(CadVector t)
        {
            int i = 0;
            foreach (CadVector p in mPointList)
            {
                if (t.VectorEquals(p))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public void ReversePointList()
        {
            if (Locked) return;

            mPointList.Reverse();
        }

        public void AddChild(CadFigure fig)
        {
            if (Locked) return;

            mChildList.Add(fig);
            fig.SetParent(this);
        }

        public void ReleaseAllChildlen()
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
            CadFigure root = GetGroupRoot();
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

        public void SetParent(CadFigure fig)
        {
            mParent = fig;
        }

        public CadFigure GetParent()
        {
            return mParent;
        }

        public CadFigure GetGroupRoot()
        {
            CadFigure fig = this;
            CadFigure parent = null;

            while (fig != null)
            {
                parent = fig.GetParent();

                if (parent == null)
                {
                    break;
                }

                fig = parent;
            }

            return fig;
        }
        #endregion

        public void CopyFrom(CadFigure fig)
        {
            Type = fig.Type;
            IsLoop = fig.IsLoop;
            Locked = fig.Locked;
            Normal = fig.Normal;

            mPointList.Clear();
            mPointList.AddRange(fig.mPointList);

            mParent = fig.mParent;

            mChildList.Clear();
            mChildList.AddRange(fig.mChildList);
        }

        public void SetPointList(List<CadVector> list)
        {
            mPointList = list;
        }

        #region "Dump" 
        public void SimpleDump(DebugOut dout, string prefix = nameof(CadFigure))
        {
            dout.println(
                prefix +
                "(" + this.GetHashCode().ToString() + ")" +
                "ID=" + ID.ToString());
        }

        public void Dump(DebugOut dout, string prefix = nameof(CadFigure))
        {
            dout.println(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            dout.Indent++;
            dout.println("ID=" + ID.ToString());
            dout.println("LayerID=" + LayerID.ToString());
            dout.println("Type=" + Type.ToString());
            dout.println("State=" + State.ToString());

            dout.println("PointList [");
            dout.Indent++;
            foreach (CadVector point in PointList)
            {
                point.dump(dout, "");
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
                States st = Behavior.GetState(this);
                return st;
            }
        }

        public System.Type GetBehaviorType()
        {
            return Behavior.GetType();
        }

        public void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;
            Log.d("moveSelectedPoints" + 
                " dx=" + delta.x.ToString() +
                " dy=" + delta.y.ToString() +
                " dz=" + delta.z.ToString()
                );
            Behavior.MoveSelectedPoint(this, dc, delta);

            mChildList.ForEach(c =>
            {
               c.MoveSelectedPoints(dc, delta);
            });
        }

        public void MoveAllPoints(CadVector delta)
        {
            if (Locked) return;

            Behavior.MoveAllPoints(this, delta);
        }

        public void AddPoint(CadVector p)
        {
            Behavior.AddPoint(this, p);
        }

        public void AddPointInCreating(DrawContext dc, CadVector p)
        {
            Behavior.AddPointInCreating(this, dc, p);
        }

        public void RemoveSelected()
        {
            if (Locked) return;

            Behavior.RemoveSelected(this);
        }

        public void Draw(DrawContext dc, int pen)
        {
            Behavior.Draw(this, dc, pen);
        }

        public void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
            Behavior.DrawSeg(this, dc, pen, idxA, idxB);
        }

        public void DrawSelected(DrawContext dc, int pen)
        {
            Behavior.DrawSelected(this, dc, pen);
        }

        public void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            Behavior.DrawTemp(this, dc, tp, pen);
        }

        public void StartCreate(DrawContext dc)
        {
            Behavior.StartCreate(this, dc);
        }

        public Types EndCreate(DrawContext dc)
        {
            return Behavior.EndCreate(this, dc);
        }

        public CadRect GetContainsRect()
        {
            return Behavior.GetContainsRect(this);
        }

        public CadRect GetContainsRectScrn(DrawContext dc)
        {
            return Behavior.GetContainsRectScrn(this, dc);
        }

        public List<CadVector> GetPoints(int curveSplitNum)
        {
            return Behavior.GetPoints(this, curveSplitNum);
        }

        public Centroid GetCentroid()
        {
            return Behavior.GetCentroid(this);
        }

        public void RecalcNormal()
        {
            Behavior.RecalcNormal(this);
        }

        public void SetThickness(double t)
        {
            double prevThick = Thickness;

            Thickness = t;

            if (Normal.IsZero())
            {
                Normal = CadUtil.RepresentativeNormal(mPointList);
            }

            if (Thickness == 0 && prevThick !=0)
            {
                int cnt = mPointList.Count / 2;

                mPointList.RemoveRange(cnt, cnt);
                return;
            }

            if (Thickness != 0 && prevThick == 0)
            {
                CadVector d = Normal * Thickness;
                int cnt = mPointList.Count;

                for (int i=0; i<cnt; i++)
                {
                    CadVector v = mPointList[i] + d;
                    mPointList.Add(v);
                }
            }
        }


        public CadSegment GetSegmentAt(int n)
        {
            return Behavior.GetSegmentAt(this, n);
        }

        public FigureSegment GetFigSegmentAt(int n)
        {
            return Behavior.GetFigSegmentAt(this, n);
        }


        public int SegmentCount
        {
            get
            {
                return Behavior.SegmentCount(this);
            }
        }

        public void ForEachFigureSegment(Func<FigureSegment, bool> dg)
        {
            int cnt = SegmentCount;
            for (int i=0; i<cnt; i++)
            {
                FigureSegment fseg = GetFigSegmentAt(i);

                if (!dg( fseg ))
                {
                    break;
                }
            }
        }

        public void ForEachSegment(Func<CadSegment, bool> dg)
        {
            int cnt = SegmentCount;
            for (int i = 0; i < cnt; i++)
            {
                if (!dg( GetSegmentAt(i) ))
                {
                    break;
                }
            }
        }

        public void ForEachPoint(Action<CadVector> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                dg(GetPointAt(i));
            }
        }

        public void ForEachPointB(Func<CadVector, bool> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                if (!dg(GetPointAt(i)))
                {
                    break;
                }
            }
        }

        public void ForEachPoint(Action<CadVector, int> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                dg(GetPointAt(i), i);
            }
        }

        public void ForEachPointB(Func<CadVector, int, bool> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                if (!dg(GetPointAt(i), i))
                {
                    break;
                }
            }
        }

        #endregion
    }
}