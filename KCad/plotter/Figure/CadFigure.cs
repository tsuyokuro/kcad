//#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json.Linq;
using CadDataTypes;

namespace Plotter
{
    using Plotter.Serializer;
    using static CadFigure;

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
            MESH,
            MAX,
        }
        #endregion

        #region  "public properties"
        public uint ID { get; set; }

        public Types Type {
            get;
            protected set;
        }

        public bool IsLoop { get; set; }

        public CadVector Normal;

        /*
        public double Thickness
        {
            get;
            set;
        }

        public CadVector ThicknessV
        {
            get
            {
                if (Thickness == 0)
                {
                    return CadVector.Zero;
                }

                return (Normal * -1.0) * Thickness;
            }
        }
        */

        public virtual VectorList PointList
        {
            get
            {
                return mPointList;
            }
        }

        public virtual int PointCount
        {
            get
            {
                return PointList.Count;
            }
        }

        public VectorList StoreList
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

        protected VectorList mPointList = new VectorList();

        protected VectorList mStoreList = null;


        #region Group management
        protected CadFigure mParent = null;

        public CadFigure Parent
        {
            set { mParent = value; }
            get { return mParent; }
        }


        protected List<CadFigure> mChildList = new List<CadFigure>();

        public List<CadFigure> ChildList
        {
            get
            {
                return mChildList;
            }

            set
            {
                mChildList = value;
            }
        }

        /// <summary>
        /// 自分とその下にあるFigureを全て列挙(中止可能版)
        /// </summary>
        /// <param name="d"></param>
        /// <returns>true:列挙を継続</returns>
        public virtual bool ForEachFig(ForEachDelegate<CadFigure> d)
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
        public virtual bool ForEachNode(ForEachDelegate<CadFigure> d)
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
        public virtual void ForEachFig(Action<CadFigure> d)
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
        public virtual void ForEachNode(Action<CadFigure> d)
        {
            int i;
            for (i = 0; i < mChildList.Count; i++)
            {
                CadFigure c = mChildList[i];
                c.ForEachFig(d);
            }
        }

        #endregion
        protected CadFigure()
        {
            ID = 0;
            IsLoop = false;
            Type = Types.NONE;
        }

        public static CadFigure Create()
        {
            return new CadFigure();
        }

        public static CadFigure Create(Types type)
        {
            CadFigure fig = null;

            switch (type)
            {
                case Types.LINE:
                    fig = new CadFigurePolyLines();
                    break;

                case Types.RECT:
                    fig = new CadFigurePolyLines();
                    break;

                case Types.POLY_LINES:
                    fig = new CadFigurePolyLines();
                    break;

                case Types.CIRCLE:
                    fig = new CadFigureCircle();
                    break;

                case Types.POINT:
                    fig = new CadFigurePoint();
                    break;

                case Types.GROUP:
                    fig = new CadFigure();
                    fig.Type = Types.GROUP;
                    break;

                case Types.DIMENTION_LINE:
                    fig = new CadFigureDimLine();
                    break;

                case Types.MESH:
                    fig = new CadFigureMesh();
                    break;

                default:
                    break;
            }

            return fig;
        }

        public virtual void ClearPoints()
        {
            mPointList.Clear();
        }

        public virtual void CopyPoints(CadFigure fig)
        {
            if (Locked) return;
            mPointList.Clear();
            mPointList.AddRange(fig.mPointList);
        }

        public virtual void AddPoints(VectorList points, int sp, int num)
        {
            for (int i = 0; i < num; i++)
            {
                CadVector p = points[i + sp];
                AddPoint(p);
            }
        }

        public virtual void AddPoints(VectorList points, int sp)
        {
            AddPoints(points, sp, points.Count - sp);
        }

        public virtual void AddPoints(VectorList points)
        {
            foreach (CadVector p in points)
            {
                AddPoint(p);
            }
        }

        public virtual void AddPointsReverse(VectorList points)
        {
            int cnt = points.Count;
            int i = cnt - 1;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public virtual void AddPointsReverse(VectorList points, int sp)
        {
            int cnt = points.Count;
            int i = cnt - 1 - sp;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public virtual void InsertPointAt(int index, CadVector pt)
        {
            if (index >= mPointList.Count)
            {
                mPointList.Add(pt);
                return;
            }

            mPointList.Insert(index, pt);
        }

        public virtual void RemovePointAt(int index)
        {
            /*
            if (mPointList == null)
            {
                return;
            }
            */
            mPointList.RemoveAt(index);
        }

        public virtual void RemovePointsRange(int index, int count)
        {
            mPointList.RemoveRange(index, count);
        }

        public virtual void InsertPointsRange(int index, VectorList collection)
        {
            mPointList.InsertRange(index, collection);
        }

        public virtual bool HasSelectedPoint()
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

        public virtual bool HasSelectedPointInclueChild()
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

        public virtual CadVector GetPointAt(int idx)
        {
            return mPointList[idx];
        }

        public virtual void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

        public virtual void SelectPointAt(int index, bool sel)
        {
            CadVector p = mPointList[index];
            p.Selected = sel;
            mPointList[index] = p;
        }

        public virtual void ClearSelectFlags()
        {
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                SelectPointAt(i, false);
            }
        }

        public virtual void Select()
        {
            // Set select flag to all points
            int i;
            for (i = 0; i < mPointList.Count; i++)
            {
                SelectPointAt(i, true);
            }
        }

        public virtual void StartEdit()
        {
            if (Locked) return;

            if (mStoreList != null)
            {
                return;
            }

            mStoreList = new VectorList();
            mStoreList.AddRange(mPointList);
        }

        public virtual DiffData EndEditWithDiff()
        {
            if (Locked) return null;


            DiffData diff = DiffData.create(this);
            mStoreList = null;
            return diff;
        }

        public virtual void EndEdit()
        {
            if (mStoreList != null)
            {
                mStoreList.Clear();
                mStoreList = null;
            }
        }

        public virtual void CancelEdit()
        {
            if (Locked) return;

            if (mStoreList == null)
            {
                return;
            }

            mPointList.Clear();
            mStoreList.ForEach(a => mPointList.Add(a));
            mStoreList = null;
        }

        public virtual int FindPoint(CadVector t)
        {
            int i = 0;
            foreach (CadVector p in mPointList)
            {
                if (t.Equals(p))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public virtual void AddChild(CadFigure fig)
        {
            if (Locked) return;

            mChildList.Add(fig);
            fig.SetParent(this);
        }

        public virtual void ReleaseAllChildlen()
        {
            if (Locked) return;

            foreach (CadFigure fig in mChildList)
            {
                fig.Parent = null;
            }

            mChildList.Clear();
        }

        #region Group
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

        public virtual void CopyFrom(CadFigure fig)
        {
            Type = fig.Type;
            IsLoop = fig.IsLoop;
            Locked = fig.Locked;
            Normal = fig.Normal;
            //Thickness = fig.Thickness;

            mPointList.Clear();
            mPointList.AddRange(fig.mPointList);

            mParent = fig.mParent;

            mChildList.Clear();
            mChildList.AddRange(fig.mChildList);
        }

        public virtual void SetPointList(VectorList list)
        {
            mPointList = list;
        }

        #region "Dump" 
        public void SimpleDump(string prefix = nameof(CadFigure))
        {
            DebugOut.println(
                prefix +
                "(" + this.GetHashCode().ToString() + ")" +
                "ID=" + ID.ToString());
        }

        public void Dump(string prefix = nameof(CadFigure))
        {
            DebugOut.println(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            DebugOut.Indent++;
            DebugOut.println("ID=" + ID.ToString());
            DebugOut.println("LayerID=" + LayerID.ToString());
            DebugOut.println("Type=" + Type.ToString());

            DebugOut.println("PointList [");
            DebugOut.Indent++;
            foreach (CadVector point in PointList)
            {
                point.dump("");
            }
            DebugOut.Indent--;
            DebugOut.println("]");


            DebugOut.println("ParentID=" + (mParent != null ? mParent.ID : 0));

            DebugOut.println("Child [");
            DebugOut.Indent++;
            foreach (CadFigure fig in mChildList)
            {
                DebugOut.println("" + fig.ID);
            }
            DebugOut.Indent--;
            DebugOut.println("]");

            DebugOut.Indent--;
            DebugOut.println("}");
        }

        #endregion

        public virtual void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;

            //Log.d("moveSelectedPoints" + 
            //    " dx=" + delta.x.ToString() +
            //    " dy=" + delta.y.ToString() +
            //    " dz=" + delta.z.ToString()
            //    );

            Util.MoveSelectedPoint(this, dc, delta);

            mChildList.ForEach(c =>
            {
               c.MoveSelectedPoints(dc, delta);
            });
        }

        public virtual void MoveAllPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;

            Util.MoveAllPoints(this, dc, delta);
        }

        public virtual void AddPoint(CadVector p)
        {
            mPointList.Add(p);
        }

        public virtual void AddPointInCreating(DrawContext dc, CadVector p)
        {
            mPointList.Add(p);
        }

        public virtual void RemoveSelected()
        {
            if (Locked) return;

            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 2)
            {
                mPointList.Clear();
            }
        }

        public virtual void Draw(DrawContext dc, int pen)
        {
        }

        public virtual void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
        }

        public virtual void DrawSelected(DrawContext dc, int pen)
        {
        }

        public virtual void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
        }

        public virtual void StartCreate(DrawContext dc)
        {
        }

        public virtual void EndCreate(DrawContext dc)
        {
        }

        public virtual CadRect GetContainsRect()
        {
            return Util.GetContainsRect(this);
        }

        public virtual CadRect GetContainsRectScrn(DrawContext dc)
        {
            return Util.GetContainsRectScrn(this, dc);
        }

        public virtual VectorList GetPoints(int curveSplitNum)
        {
            return Util.GetPoints(this, curveSplitNum);
        }

        public virtual Centroid GetCentroid()
        {
            return default(Centroid);
        }

        public virtual void RecalcNormal()
        {
            Normal = Util.CalcNormal(this);
        }

        public virtual CadSegment GetSegmentAt(int n)
        {
            return Util.GetSegmentAt(this, n);
        }

        public virtual FigureSegment GetFigSegmentAt(int n)
        {
            return Util.GetFigSegmentAt(this, n);
        }


        public virtual int SegmentCount
        {
            get
            {
                return Util.SegmentCount(this);
            }
        }

        public virtual void ForEachFigureSegment(Func<FigureSegment, bool> dg)
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

        public virtual bool IsSelectedAll()
        {
            int i;
            for (i=0; i<mPointList.Count; i++)
            {
                if (!mPointList[i].Selected)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void ForEachSegment(Func<CadSegment, bool> dg)
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

        public virtual void ForEachPoint(Action<CadVector> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                dg(GetPointAt(i));
            }
        }

        public virtual void ForEachPointB(Func<CadVector, bool> dg)
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

        public virtual void ForEachPoint(Action<CadVector, int> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                dg(GetPointAt(i), i);
            }
        }

        public virtual void ForEachPointB(Func<CadVector, int, bool> dg)
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

        //public virtual void ForEachThicknessPoint(Action<CadVector, int> dg)
        //{
        //    if (Thickness == 0)
        //    {
        //        return;
        //    }

        //    int cnt = mPointList.Count;

        //    CadVector t = (Normal * -1.0) * Thickness;

        //    for (int i = 0; i < cnt; i++)
        //    {
        //        CadVector v = mPointList[i] + t;

        //        dg(v, i);
        //    }
        //}

        public virtual void InvertDir()
        {
        }

        public virtual bool IsGarbage()
        {
            if (mPointList.Count > 0)
            {
                return false;
            }

            if (mChildList.Count > 0)
            {
                return false;
            }

            return true;
        }

        public void RemoveGarbageChildren()
        {
            for (int i = mChildList.Count - 1; i >= 0; i--)
            {
                CadFigure fig = mChildList[i];

                fig.RemoveGarbageChildren();

                if (fig.IsGarbage())
                {
                    mChildList.RemoveAt(i);
                }
            }
        }

        public virtual JObject GeometricDataToJson()
        {
            JArray pointArray = CadJson.ToJson.VectorListToJson(PointList);

            JObject jvdata = new JObject();
            jvdata.Add(CadJson.VECTOR.POINT_LIST, pointArray);

            return jvdata;
        }

        public virtual void GeometricDataFromJson(JObject jvdata, CadJson.VersionCode version)
        {
            JArray jarray = (JArray)jvdata[CadJson.VECTOR.POINT_LIST];

            VectorList vl = CadJson.FromJson.VectorListFromJson(jarray, version);
            mPointList = vl;
        }


        public virtual MpGeometricData GeometricDataToMp()
        {
            MpSimpleGeometricData g = new MpSimpleGeometricData();
            g.PointList = MpUtil.VectortListToMp(PointList);
            return g;
        }

        public virtual void GeometricDataFromMp(MpGeometricData geo)
        {
            if (!(geo is MpSimpleGeometricData))
            {
                return;
            }

            MpSimpleGeometricData g = (MpSimpleGeometricData)geo;

            mPointList = MpUtil.VectortListFromMp(g.PointList);
        }

            
    } // End of class CadFigure
}