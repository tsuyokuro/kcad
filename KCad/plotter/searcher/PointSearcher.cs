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

        private List<SelectItem> IgnoreList = null;

        private CadPoint TargetPoint;
        private double mRange;

        public uint CurrentLayerID
        {
            set; get;
        } = 0;

        public PointSearcher()
        {
            Clean();
        }

        public void SetRangePixel(DrawContext dc, int pixel)
        {
            //double d = dc.pixelsToMilli(pixel);
            mRange = pixel;
        }

        public void Clean()
        {
            xmatch.reset();
            ymatch.reset();
            xymatch.reset();
        }

        public void SetTargetPoint(CadPoint p)
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

        public MarkPoint GetXYMatch()
        {
            return xymatch;
        }

        public void SearchAllLayer(DrawContext dc, CadPoint p, CadObjectDB db)
        {
            TargetPoint = p;
            SearchAllLayer(dc, db);
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

        public void Search(DrawContext dc, CadPoint p, CadObjectDB db, CadLayer layer)
        {
            TargetPoint = p;
            Search(dc, db, layer);
        }

        public void Search(DrawContext dc, CadObjectDB db, CadLayer layer)
        {
            if (layer == null)
            {
                return;
            }

            // In order to give priority to the new Obj, to scan in reverse order
            IEnumerable<CadFigure> list = layer.FigureList;

            var rev = list.Reverse();

            foreach (CadFigure fig in rev)
            {
                CheckFigure(dc, layer, fig);
            }
        }

        public void Check(DrawContext dc, CadPoint pt)
        {
            CheckFigPoint(dc, pt, 0, null, 0, MarkPoint.Types.IDEPEND_POINT);
        }

        private void CheckFigure(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            IReadOnlyList<CadPoint> pointList = fig.PointList;

            if (pointList == null)
            {
                return;
            }

            int idx = -1;

            foreach (CadPoint pt in pointList)
            {
                idx++;
                CheckFigPoint(dc, pt, layer.ID, fig, idx, MarkPoint.Types.POINT);
            }
        }

        public void CheckRelativePoints(DrawContext dc, CadObjectDB db)
        {
            foreach (CadLayer layer in db.LayerList)
            {
                CheckRelativePoints(dc, layer);
            }
        }

        public void CheckRelativePoints(DrawContext dc, CadLayer layer)
        {
            List<CadRelativePoint> list = layer.RelPointList;

            int idx = 0;
            foreach (CadRelativePoint rp in list)
            {
                CheckFigPoint(dc, rp.point, layer.ID, null, idx, MarkPoint.Types.RELATIVE_POINT);
                idx++;
            }
        }

        private void CheckFigPoint(DrawContext dc, CadPoint pt, uint layerID, CadFigure fig, int ptIdx, MarkPoint.Types type)
        {
            if (fig != null && IsIgnore(fig.ID, ptIdx))
            {
                return;
            }

            CadPoint ppt = dc.CadPointToUnitPoint(pt);

            double dx = Math.Abs(ppt.x - TargetPoint.x);
            double dy = Math.Abs(ppt.y - TargetPoint.y);

            if (dx <= mRange)
            {
                if (dx < xmatch.DistX || (dx == xmatch.DistX && dy < xmatch.DistY))
                {
                    xmatch.Type = type;
                    xmatch.LayerID = layerID;
                    xmatch.Figure = fig;
                    xmatch.PointIndex = ptIdx;
                    xmatch.Point = pt;
                    xmatch.ViewPoint = ppt;
                    xmatch.Flag |= MarkPoint.X_MATCH;
                    xmatch.DistX = dx;
                    xmatch.DistY = dy;
                }
            }

            if (dy <= mRange)
            {
                if (dy < ymatch.DistY || (dy == ymatch.DistY && dx < ymatch.DistX))
                {
                    ymatch.Type = type;
                    ymatch.LayerID = layerID;
                    ymatch.Figure = fig;
                    ymatch.PointIndex = ptIdx;
                    ymatch.Point = pt;
                    ymatch.ViewPoint = ppt;
                    ymatch.Flag |= MarkPoint.Y_MATCH;
                    ymatch.DistX = dx;
                    ymatch.DistY = dy;
                }
            }

            if (dx <= mRange && dy <= mRange)
            {
                if (dx < xymatch.DistX || dy < xymatch.DistY)
                {
                    xymatch.Type = type;
                    xymatch.LayerID = layerID;
                    xymatch.Figure = fig;
                    xymatch.PointIndex = ptIdx;
                    xymatch.Point = pt;
                    xymatch.ViewPoint = ppt;
                    xymatch.Flag |= MarkPoint.X_MATCH;
                    xymatch.Flag |= MarkPoint.Y_MATCH;
                    xymatch.DistX = dx;
                    xymatch.DistY = dy;
                }
            }
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
