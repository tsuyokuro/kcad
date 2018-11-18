#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using CadDataTypes;


namespace Plotter
{
    public class PointSearcher
    {
        private MarkPoint XMatch = default(MarkPoint);
        private MarkPoint YMatch = default(MarkPoint);
        private MarkPoint XYMatch = default(MarkPoint);

        private List<MarkPoint> XYMatchList = new List<MarkPoint>();


        private IReadOnlyList<MarkPoint> IgnoreList = null;

        public CadCursor Target;    // Cursor(スクリーン座標系)
        public double Range;        // matchする範囲(スクリーン座標系)

        public uint CurrentLayerID
        {
            set; get;
        } = 0;

        public bool IsXMatch
        {
            get
            {
                return XMatch.IsValid;
            }
        }

        public bool IsYMatch
        {
            get
            {
                return YMatch.IsValid;
            }
        }

        public bool IsXYMatch
        {
            get
            {
                return XYMatch.IsValid;
            }
        }

        public PointSearcher()
        {
            Clean();
        }

        public void SetRangePixel(DrawContext dc, double pixel)
        {
            Range = pixel;
        }

        public void Clean()
        {
            XMatch.reset();
            YMatch.reset();
            XYMatch.reset();

            XYMatchList.Clear();
        }

        public void SetTargetPoint(CadCursor cursor)
        {
            Target = cursor;
        }

        public void SetIgnoreList(List<MarkPoint> list)
        {
            IgnoreList = list;
        }

        public MarkPoint GetXMatch()
        {
            return XMatch;
        }

        public MarkPoint GetYMatch()
        {
            return YMatch;
        }

        public MarkPoint GetXYMatch(int n = -1)
        {
            if (n==-1)
            {
                return XYMatch;
            }

            if (XYMatchList.Count == 0)
            {
                return XYMatch;
            }

            return XYMatchList[n];
        }

        public List<MarkPoint> GetXYMatches()
        {
            return XYMatchList;
        }

        public double Distance()
        {
            double ret = Double.MaxValue;
            double t;

            if (IsXMatch)
            {
                ret = (XMatch.PointScrn - Target.Pos).Norm();
            }

            if (IsYMatch)
            {
                t = (YMatch.PointScrn - Target.Pos).Norm();
                ret = Math.Min(t, ret);
            }

            if (IsXYMatch)
            {
                t = (XYMatch.PointScrn - Target.Pos).Norm();
                ret = Math.Min(t, ret);
            }

            return ret;
        }

        public void SearchAllLayer(DrawContext dc, CadObjectDB db)
        {
            if (db.CurrentLayer.Visible)
            {
                Search(dc, db, db.CurrentLayer);
            }

            for (int i=0; i<db.LayerList.Count; i++)
            {
                CadLayer layer = db.LayerList[i];

                if (layer.ID == db.CurrentLayerID)
                {
                    continue;
                }

                if (!layer.Visible)
                {
                    continue;
                }

                Search(dc, db, layer);
            }
        }

        public void Search(DrawContext dc, CadObjectDB db, CadLayer layer)
        {
            if (layer == null)
            {
                return;
            }

            for (int i=0; i< layer.FigureList.Count; i++)
            {
                CadFigure fig = layer.FigureList[i];
                CheckFigure(dc, layer, fig);
            }
        }

        public void Check(DrawContext dc, CadVector pt)
        {
            CheckFigPoint(dc, pt, null, null, 0);
        }

        public void Check(DrawContext dc, VectorList list)
        {
            for (int i=0;i< list.Count; i++)
            {
                Check(dc, list[i]);
            }
        }

        public void CheckFigure(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            VectorList list = fig.PointList;

            if (fig.StoreList != null)
            {
                list = fig.StoreList;
            }

            for (int i=0; i < fig.PointCount; i++)
            {
                CheckFigPoint(dc, list[i], layer, fig, i);
            }

            if (fig.ChildList != null)
            {
                for (int i=0; i< fig.ChildList.Count; i++)
                {
                    CadFigure c = fig.ChildList[i];
                    CheckFigure(dc, layer, c);
                }
            }
        }

        private void CheckFigPoint(DrawContext dc, CadVector pt, CadLayer layer, CadFigure fig, int ptIdx)
        {
            if (fig != null && IsIgnore(fig.ID, ptIdx))
            {
                return;
            }

            CadVector ppt = dc.WorldPointToDevPoint(pt);

            double dx = Math.Abs(ppt.x - Target.Pos.x);
            double dy = Math.Abs(ppt.y - Target.Pos.y);

            CrossInfo cix = CadUtil.PerpendicularCrossLine(Target.Pos, Target.Pos + Target.DirX, ppt);
            CrossInfo ciy = CadUtil.PerpendicularCrossLine(Target.Pos, Target.Pos + Target.DirY, ppt);

            double nx = (ppt - ciy.CrossPoint).Norm(); // Cursor Y軸からの距離
            double ny = (ppt - cix.CrossPoint).Norm(); // Cursor X軸からの距離

            if (nx <= Range)
            {
                if (nx < XMatch.DistanceX || (nx == XMatch.DistanceX && ny < XMatch.DistanceY))
                {
                    XMatch = getMarkPoint();
                }
            }

            if (ny <= Range)
            {
                if (ny < YMatch.DistanceY || (ny == YMatch.DistanceY && nx < YMatch.DistanceX))
                {
                    YMatch = getMarkPoint();
                }
            }

            if (dx <= Range && dy <= Range)
            {
                if (dx <= XYMatch.DistanceX || dy <= XYMatch.DistanceY)
                {
                    MarkPoint t = default(MarkPoint);

                    t.IsValid = true;
                    t.Layer = layer;
                    t.Figure = fig;
                    t.PointIndex = ptIdx;
                    t.Point = pt;
                    t.PointScrn = ppt;
                    t.DistanceX = dx;
                    t.DistanceY = dy;

                    XYMatch = t;

                    if (!ContainsXYMatch(t))
                    {
                        XYMatchList.Add(XYMatch);
                    }
                }
            }

            MarkPoint getMarkPoint()
            {
                MarkPoint mp = default(MarkPoint);

                mp.IsValid = true;
                mp.Layer = layer;
                mp.Figure = fig;
                mp.PointIndex = ptIdx;
                mp.Point = pt;
                mp.PointScrn = ppt;
                mp.DistanceX = nx;
                mp.DistanceY = ny;

                return mp;
            }
        }

        private bool ContainsXYMatch(MarkPoint mp)
        {
            for (int i=0; i<XYMatchList.Count; i++)
            {
                MarkPoint lmp = XYMatchList[i];

                if (mp.FigureID == lmp.FigureID && mp.PointIndex == lmp.PointIndex)
                {
                    return true;
                }
            }

            return false;
        }


        private bool IsIgnore(uint figId, int index)
        {
            if (IgnoreList == null)
            {
                return false;
            }

            for (int i=0;i<IgnoreList.Count;i++)
            {
                MarkPoint item = IgnoreList[i];

                if (item.FigureID == figId && item.PointIndex == index)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
