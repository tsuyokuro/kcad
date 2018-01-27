#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;


namespace Plotter
{
    public class PointSearcher
    {
        private MarkPoint xmatch = default(MarkPoint);
        private MarkPoint ymatch = default(MarkPoint);
        private MarkPoint xymatch = default(MarkPoint);

        private List<MarkPoint> XYMatchList = new List<MarkPoint>();


        private IReadOnlyList<SelectItem> IgnoreList = null;

        private CadCursor TargetPoint;
        private double mRange;

        public uint CurrentLayerID
        {
            set; get;
        } = 0;

        public PointSearcher()
        {
            CleanMatches();
        }

        public void SetRangePixel(DrawContext dc, double pixel)
        {
            mRange = pixel;
        }

        public void CleanMatches()
        {
            xmatch.reset();
            ymatch.reset();
            xymatch.reset();

            XYMatchList.Clear();
        }

        public void SetTargetPoint(CadCursor p)
        {
            TargetPoint = p;
        }

        public void SetIgnoreList(List<SelectItem> list)
        {
            IgnoreList = list;
        }

        public MarkPoint GetXMatch()
        {
            return xmatch;
        }

        public MarkPoint GetYMatch()
        {
            return ymatch;
        }

        public MarkPoint GetXYMatch(int n = -1)
        {
            if (n==-1)
            {
                return xymatch;
            }

            if (XYMatchList.Count == 0)
            {
                return xymatch;
            }

            return XYMatchList[n];
        }

        public List<MarkPoint> GetXYMatches()
        {
            return XYMatchList;
        }

        public void SearchAllLayer(DrawContext dc, CadObjectDB db)
        {
            if (db.CurrentLayer.Visible)
            {
                Search(dc, db, db.CurrentLayer);
            }

            foreach (CadLayer layer in db.LayerList)
            {
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

            layer.ForEachFig(fig =>
            {
                CheckFigure(dc, layer, fig);
            });
        }

        public void Check(DrawContext dc, CadVector pt)
        {
            CheckFigPoint(dc, pt, null, null, 0);
        }

        public void Check(DrawContext dc, List<CadVector> list)
        {
            foreach(CadVector pt in list)
            {
                Check(dc, pt);
            }
        }

        public void CheckFigure(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            fig.ForEachPoint((pt, idx) =>
            {
                CheckFigPoint(dc, pt, layer, fig, idx);
            });

            /*
            IReadOnlyList<CadVector> pointList = fig.PointList;

            if (pointList == null)
            {
                return;
            }

            int idx = -1;

            foreach (CadVector pt in pointList)
            {
                idx++;
                CheckFigPoint(dc, pt, layer, fig, idx);
            }
            */
        }

        private void CheckFigPoint(DrawContext dc, CadVector pt, CadLayer layer, CadFigure fig, int ptIdx)
        {
            if (fig != null && IsIgnore(fig.ID, ptIdx))
            {
                return;
            }

            CadVector ppt = dc.CadPointToUnitPoint(pt);

            double dx = Math.Abs(ppt.x - TargetPoint.Pos.x);
            double dy = Math.Abs(ppt.y - TargetPoint.Pos.y);

            CrossInfo cix = CadUtil.PerpendicularCrossLine(TargetPoint.Pos, TargetPoint.Pos + TargetPoint.DirX, ppt);
            CrossInfo ciy = CadUtil.PerpendicularCrossLine(TargetPoint.Pos, TargetPoint.Pos + TargetPoint.DirY, ppt);

            double nx = (ppt - ciy.CrossPoint).Norm(); // Cursor Y軸からの距離
            double ny = (ppt - cix.CrossPoint).Norm(); // Cursor X軸からの距離

            if (nx <= mRange)
            {
                if (nx < xmatch.DistanceX || (nx == xmatch.DistanceX && ny < xmatch.DistanceY))
                {
                    xmatch = getMarkPoint();
                }
            }

            if (ny <= mRange)
            {
                if (ny < ymatch.DistanceY || (ny == ymatch.DistanceY && nx < ymatch.DistanceX))
                {
                    ymatch = getMarkPoint();
                }
            }

            if (dx <= mRange && dy <= mRange)
            {
                if (dx <= xymatch.DistanceX || dy <= xymatch.DistanceY)
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

                    xymatch = t;

                    if (!ContainsXYMatch(t))
                    {
                        XYMatchList.Add(xymatch);
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
            foreach (MarkPoint lmp in XYMatchList)
            {
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

            foreach (SelectItem item in IgnoreList)
            {
                if (item.FigureID == figId && item.PointIndex == index)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
