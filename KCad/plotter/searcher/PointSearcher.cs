#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Plotter
{
    public struct MarkPoint
    {
        public enum Types : byte
        {
            POINT = 0,
            RELATIVE_POINT = 1,
            IDEPEND_POINT = 2,
        }

        public Types Type { get; set; }

        public static UInt32 X_MATCH = 1;
        public static UInt32 Y_MATCH = 2;
        public static UInt32 Z_MATCH = 4;

        public uint LayerID;
        public uint FigureID
        {
            get
            {
                if (Figure == null)
                {
                    return 0;
                }

                return Figure.ID;
            }
        }

        public CadFigure Figure;
        public int PointIndex;

        public CadPoint Point;

        public CadPoint ViewPoint;

        public uint Flag;

        public double DistX;
        public double DistY;
        public double DistZ;

        public void init()
        {
            this = default(MarkPoint);

            DistX = Double.MaxValue;
            DistY = Double.MaxValue;
            DistZ = Double.MaxValue;
        }

        public void dump(DebugOut dout)
        {
            dout.println("MarkPoint {");
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PointIndex:" + PointIndex.ToString());
            dout.println("}");
        }
    }


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
            clean();
        }

        public void setRangePixel(DrawContext dc, int pixel)
        {
            //double d = dc.pixelsToMilli(pixel);
            mRange = pixel;
        }

        public void clean()
        {
            xmatch.init();
            ymatch.init();
            xymatch.init();
        }

        public void setTargetPoint(CadPoint p)
        {
            TargetPoint = p;
        }

        public void setIgnoreList(List<SelectItem> list)
        {
            IgnoreList = list;
        }

        public MarkPoint getXMatch()
        {
            return xmatch;
        }

        public MarkPoint getYMatch()
        {
            return ymatch;
        }

        public MarkPoint getXYMatch()
        {
            return xymatch;
        }

        public void searchAllLayer(DrawContext dc, CadPoint p, CadObjectDB db)
        {
            TargetPoint = p;
            searchAllLayer(dc, db);
        }

        public void searchAllLayer(DrawContext dc, CadObjectDB db)
        {
            if (db.CurrentLayer.Visible)
            {
                search(dc, db, db.CurrentLayer);
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

                search(dc, db, layer);
            }
        }

        public void search(DrawContext dc, CadPoint p, CadObjectDB db, CadLayer layer)
        {
            TargetPoint = p;
            search(dc, db, layer);
        }

        public void search(DrawContext dc, CadObjectDB db, CadLayer layer)
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
                checkFig(dc, layer, fig);
            }
        }

        public void check(DrawContext dc, CadPoint pt)
        {
            checkFigPoint(dc, pt, 0, null, 0, MarkPoint.Types.IDEPEND_POINT);
        }

        private void checkFig(DrawContext dc, CadLayer layer, CadFigure fig)
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
                checkFigPoint(dc, pt, layer.ID, fig, idx, MarkPoint.Types.POINT);
            }
        }

        public void checkRelativePoints(DrawContext dc, CadObjectDB db)
        {
            foreach (CadLayer layer in db.LayerList)
            {
                checkRelativePoints(dc, layer);
            }
        }

        public void checkRelativePoints(DrawContext dc, CadLayer layer)
        {
            List<CadRelativePoint> list = layer.RelPointList;

            int idx = 0;
            foreach (CadRelativePoint rp in list)
            {
                checkFigPoint(dc, rp.point, layer.ID, null, idx, MarkPoint.Types.RELATIVE_POINT);
                idx++;
            }
        }

        private void checkFigPoint(DrawContext dc, CadPoint pt, uint layerID, CadFigure fig, int ptIdx, MarkPoint.Types type)
        {
            if (fig != null && isIgnore(fig.ID, ptIdx))
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

        private bool isIgnore(uint figId, int index)
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
