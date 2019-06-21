﻿//#define LOG_DEBUG

using System;
using System.Collections.Generic;
using CadDataTypes;
using OpenTK;
using Plotter.Serializer.v1001;

namespace Plotter
{
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
            NURBS_LINE,
            NURBS_SURFACE,
            MAX,
        }
        #endregion


        private static Dictionary<CadFigure.Types, string> TypeNames;

        static CadFigure()
        {
            TypeNames = new Dictionary<CadFigure.Types, string>()
            {
                { Types.NONE, "NONE" },
                { Types.LINE, "LINE" },
                { Types.RECT, "RECT" },
                { Types.POLY_LINES, "LINES" },
                { Types.CIRCLE, "CIRCLE" },
                { Types.POINT, "POINT" },
                { Types.GROUP, "GROUP" },
                { Types.DIMENTION_LINE, "DIM" },
                { Types.MESH, "MESH" },
                { Types.NURBS_LINE, "NURBS-LINE" },
                { Types.NURBS_SURFACE, "NURBS-SURFACE" },
            };
        }

        public static string TypeName(Types type)
        {
            string s;

            if (TypeNames.TryGetValue(type, out s))
            {
                return s;
            }

            return TypeNames[Types.NONE];

            //string s = Enum.GetName(typeof(Types), type);
            //if (s == null) return "UNKNOWN";

            //return s;
        }


        #region  "public properties"
        public uint ID { get; set; }

        public Types Type {
            get;
            protected set;
        }

        public bool IsLoop { get; set; }

        public Vector3d Normal;

        public virtual VertexList PointList => mPointList;

        public virtual int PointCount => PointList.Count;

        public VertexList StoreList => mStoreList;

        public bool Locked  { set; get; } = false;

        public uint LayerID { set; get; } = 0;
        
        public bool Current { set; get; } = false;

        public int FontID { set; get; } = DrawTools.FONT_SMALL;

        public int BrushID { set; get; } = DrawTools.BRUSH_TEXT;

        public bool IsSelected { get; set; } = false;

        #endregion

        protected VertexList mPointList = new VertexList();

        protected VertexList mStoreList = null;


        #region Group management
        protected CadFigure mParent = null;

        public CadFigure Parent
        {
            set => mParent = value;
            get => mParent;
        }


        protected List<CadFigure> mChildList = new List<CadFigure>();

        public List<CadFigure> ChildList
        {
            get => mChildList;
            set => mChildList = value;
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

                case Types.NURBS_LINE:
                    fig = new CadFigureNurbsLine();
                    break;

                case Types.NURBS_SURFACE:
                    fig = new CadFigureNurbsSurface();
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

        public virtual void AddPoints(VertexList points, int sp, int num)
        {
            for (int i = 0; i < num; i++)
            {
                CadVertex p = points[i + sp];
                AddPoint(p);
            }
        }

        public virtual void AddPoints(VertexList points, int sp)
        {
            AddPoints(points, sp, points.Count - sp);
        }

        public virtual void AddPoints(VertexList points)
        {
            foreach (CadVertex p in points)
            {
                AddPoint(p);
            }
        }

        public virtual void AddPointsReverse(VertexList points)
        {
            int cnt = points.Count;
            int i = cnt - 1;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public virtual void AddPointsReverse(VertexList points, int sp)
        {
            int cnt = points.Count;
            int i = cnt - 1 - sp;

            for (; i >= 0; i--)
            {
                AddPoint(points[i]);
            }
        }

        public virtual void InsertPointAt(int index, CadVertex pt)
        {
            if (index > mPointList.Count - 1)
            {
                mPointList.Add(pt);
                return;
            }

            mPointList.Insert(index, pt);
        }

        public virtual void RemovePointAt(int index)
        {
            mPointList.RemoveAt(index);
        }

        public virtual void RemovePointsRange(int index, int count)
        {
            mPointList.RemoveRange(index, count);
        }

        public virtual void InsertPointsRange(int index, VertexList collection)
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

        public virtual CadVertex GetPointAt(int idx)
        {
            return mPointList[idx];
        }

        public virtual CadVertex GetStorePointAt(int idx)
        {
            return mStoreList[idx];
        }

        public virtual void SetPointAt(int index, CadVertex pt)
        {
            mPointList[index] = pt;
        }

        public virtual void SelectPointAt(int index, bool sel)
        {
            CadVertex p = mPointList[index];
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

            IsSelected = false;
        }

        public virtual void Select()
        {
            SelectAllPoints();

            mChildList.ForEach(c =>
            {
                c.Select();
            });
        }

        public virtual void SelectAllPoints()
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

            //DOut.pl($"StartEdit ID:{ID}");

            mStoreList = new VertexList();
            mStoreList.AddRange(mPointList);
        }

        public virtual void EndEdit()
        {
            if (mStoreList != null)
            {
                mStoreList.Clear();
                mStoreList = null;
            }

            //DOut.pl($"EndEdit ID:{ID}");
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

        public virtual int FindPoint(CadVertex t)
        {
            int i = 0;
            foreach (CadVertex p in mPointList)
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

        public virtual void SetPointList(VertexList list)
        {
            mPointList = list;
        }

        #region "Dump" 
        public void SimpleDump(string prefix = nameof(CadFigure))
        {
            DOut.pl(
                prefix +
                "(" + this.GetHashCode().ToString() + ")" +
                "ID=" + ID.ToString());
        }

        public void Dump(string prefix = nameof(CadFigure))
        {
            DOut.pl(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            DOut.Indent++;
            DOut.pl("ID=" + ID.ToString());
            DOut.pl("LayerID=" + LayerID.ToString());
            DOut.pl("Type=" + Type.ToString());

            Normal.dump("Normal=");

            DOut.pl("PointList [");
            DOut.Indent++;
            foreach (CadVertex point in PointList)
            {
                point.dump("");
            }
            DOut.Indent--;
            DOut.pl("]");


            DOut.pl("ParentID=" + (mParent != null ? mParent.ID : 0));

            DOut.pl("Child [");
            DOut.Indent++;
            foreach (CadFigure fig in mChildList)
            {
                DOut.pl("" + fig.ID);
            }
            DOut.Indent--;
            DOut.pl("]");

            DOut.Indent--;
            DOut.pl("}");
        }

        #endregion

        public virtual void MoveSelectedPointsFromStored(DrawContext dc, Vector3d delta)
        {
            if (Locked) return;

            //Log.d("moveSelectedPoints" + 
            //    " dx=" + delta.x.ToString() +
            //    " dy=" + delta.y.ToString() +
            //    " dz=" + delta.z.ToString()
            //    );

            FigUtil.MoveSelectedPointsFromStored(this, dc, delta);

            mChildList.ForEach(c =>
            {
               c.MoveSelectedPointsFromStored(dc, delta);
            });
        }

        public virtual void MoveAllPoints(Vector3d delta)
        {
            if (Locked) return;

            FigUtil.MoveAllPoints(this, delta);
        }

        public virtual void AddPoint(CadVertex p)
        {
            mPointList.Add(p);
        }

        public virtual void AddPointInCreating(DrawContext dc, CadVertex p)
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

        public virtual void Draw(DrawContext dc, DrawPen pen)
        {
        }

        public virtual void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
        }

        public virtual void DrawSelected(DrawContext dc, DrawPen pen)
        {
        }

        public virtual void DrawTemp(DrawContext dc, CadVertex tp, DrawPen pen)
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
            return FigUtil.GetContainsRect(this);
        }

        public virtual CadRect GetContainsRectScrn(DrawContext dc)
        {
            return FigUtil.GetContainsRectScrn(this, dc);
        }

        public virtual VertexList GetPoints(int curveSplitNum)
        {
            return FigUtil.GetPoints(this, curveSplitNum);
        }

        public virtual Centroid GetCentroid()
        {
            return default(Centroid);
        }

        public virtual void RecalcNormal()
        {
            Normal = CadUtil.TypicalNormal(PointList);
        }

        public virtual CadSegment GetSegmentAt(int n)
        {
            return FigUtil.GetSegmentAt(this, n);
        }

        public virtual FigureSegment GetFigSegmentAt(int n)
        {
            return FigUtil.GetFigSegmentAt(this, n);
        }


        public virtual int SegmentCount
        {
            get
            {
                return FigUtil.SegmentCount(this);
            }
        }

        /*
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
        */

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

        public virtual bool IsPointSelected(int idx)
        {
            if (idx >= PointCount) return false;
            return PointList[idx].Selected;
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

        public virtual void ForEachPoint(Action<CadVertex> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                dg(GetPointAt(i));
            }
        }

        public virtual void ForEachPointB(Func<CadVertex, bool> dg)
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

        public virtual void ForEachPoint(Action<CadVertex, int> dg)
        {
            int cnt = PointCount;
            for (int i = 0; i < cnt; i++)
            {
                dg(GetPointAt(i), i);
            }
        }

        public virtual void ForEachPointB(Func<CadVertex, int, bool> dg)
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

        public virtual void Rotate(Vector3d org, CadQuaternion q, CadQuaternion r)
        {
            CadQuaternion qp;

            int n = PointList.Count;

            for (int i = 0; i < n; i++)
            {
                CadVertex p = PointList[i];

                p.vector -= org;

                qp = CadQuaternion.FromPoint(p.vector);

                qp = r * qp;
                qp = qp * q;

                p.vector = qp.ToPoint();

                p += org;

                PointList[i] = p;
            }
        }



        public virtual MpGeometricData_v1001 GeometricDataToMp_v1001()
        {
            MpSimpleGeometricData_v1001 geo = new MpSimpleGeometricData_v1001();
            geo.PointList = MpUtil_v1001.VertexListToMp(PointList);
            return geo;
        }

        public virtual void GeometricDataFromMp_v1001(MpGeometricData_v1001 geo)
        {
            if (!(geo is MpSimpleGeometricData_v1001))
            {
                return;
            }

            MpSimpleGeometricData_v1001 g = (MpSimpleGeometricData_v1001)geo;

            mPointList = MpUtil_v1001.VertexListFromMp(g.PointList);
        }

        public virtual MpGeometricData_v1002 GeometricDataToMp_v1002()
        {
            MpSimpleGeometricData_v1002 geo = new MpSimpleGeometricData_v1002();
            geo.PointList = MpUtil_v1002.VertexListToMp(PointList);
            return geo;
        }

        public virtual void GeometricDataFromMp_v1002(MpGeometricData_v1002 geo)
        {
            if (!(geo is MpSimpleGeometricData_v1002))
            {
                return;
            }

            MpSimpleGeometricData_v1002 g = (MpSimpleGeometricData_v1002)geo;

            mPointList = MpUtil_v1002.VertexListFromMp(g.PointList);
        }
    } // End of class CadFigure
}