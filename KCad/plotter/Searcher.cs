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

        public UInt32 Flag;

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

    public struct MarkSeg
    {
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

        public int PtIndexA;
        public int PtIndexB;

        public CadPoint pA;
        public CadPoint pB;

        public CadPoint CrossPoint;

        public double Distance;

        public void clean()
        {
            Figure = null;
        }

        public bool Valid { get { return FigureID != 0; } }

        public void dump(DebugOut dout)
        {
            dout.println("MarkSeg {");
            dout.Indent++;
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PtIndexA:" + PtIndexA.ToString());
            dout.println("PtIndexB:" + PtIndexB.ToString());
            dout.Indent--;
            dout.println("}");
        }

        public bool update()
        {
            if (Figure == null)
            {
                return true;
            }

            if (PtIndexA >= Figure.PointList.Count)
            {
                return false;
            }

            if (PtIndexB >= Figure.PointList.Count)
            {
                return false;
            }


            pA = Figure.PointList[PtIndexA];
            pB = Figure.PointList[PtIndexB];

            return true;
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
            double d = dc.pixelsToMilli(pixel);
            mRange = d;
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

        public void searchAllLayer(CadPoint p, CadObjectDB db)
        {
            TargetPoint = p;
            searchAllLayer(db);
        }

        public void searchAllLayer(CadObjectDB db)
        {
            if (db.CurrentLayer.Visible)
            {
                search(db, db.CurrentLayer);
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

                search(db, layer);
            }
        }

        public void search(CadPoint p, CadObjectDB db, CadLayer layer)
        {
            TargetPoint = p;
            search(db, layer);
        }

        public void search(CadObjectDB db, CadLayer layer)
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
                checkFig(layer, fig);
            }
        }

        public void check(CadPoint pt)
        {
            checkFigPoint(pt, 0, null, 0, MarkPoint.Types.IDEPEND_POINT);
        }

        private void checkFig(CadLayer layer, CadFigure fig)
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
                checkFigPoint(pt, layer.ID, fig, idx, MarkPoint.Types.POINT);
            }
        }

        public void checkRelativePoints(CadObjectDB db)
        {
            foreach (CadLayer layer in db.LayerList)
            {
                checkRelativePoints(layer);
            }
        }

        public void checkRelativePoints(CadLayer layer)
        {
            List<CadRelativePoint> list = layer.RelPointList;

            int idx = 0;
            foreach (CadRelativePoint rp in list)
            {
                checkFigPoint(rp.point, layer.ID, null, idx, MarkPoint.Types.RELATIVE_POINT);
                idx++;
            }
        }

        private void checkFigPoint(CadPoint pt, uint layerID, CadFigure fig, int ptIdx, MarkPoint.Types type)
        {
            if (fig != null && isIgnore(fig.ID, ptIdx))
            {
                return;
            }

            double dx = Math.Abs(pt.x - TargetPoint.x);
            double dy = Math.Abs(pt.y - TargetPoint.y);

            if (dx <= mRange)
            {
                if (dx < xmatch.DistX || (dx == xmatch.DistX && dy < xmatch.DistY))
                {
                    xmatch.Type = type;
                    xmatch.LayerID = layerID;
                    xmatch.Figure = fig;
                    xmatch.PointIndex = ptIdx;
                    xmatch.Point = pt;
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

    public class SegSearcher
    {
        private MarkSeg seg;

        private CadPoint TargetPoint;
        private double mRange;
        private double minDist = 0;

        private List<MarkSeg> IgnoreSegList; 

        private List<SelectItem> IgnoreList = null;

        public void setRangePixel(DrawContext dc, int pixel)
        {
            double d = dc.pixelsToMilli(pixel);
            mRange = d;
        }

        public void clean()
        {
            seg = default(MarkSeg);
        }

        public void setTargetPoint(CadPoint p)
        {
            TargetPoint = p;
        }

        public void setIgnoreList(List<SelectItem> list)
        {
            IgnoreList = list;
        }

        public void setIgnoreSeg(List<MarkSeg> segList)
        {
            IgnoreSegList = segList;
        }

        public MarkSeg getMatch()
        {
            return seg;
        }

        public void searchAllLayer(CadPoint p, CadObjectDB db)
        {
            TargetPoint = p;
            searchAllLayer(db);
        }

        public void searchAllLayer(CadObjectDB db)
        {
            search(db, db.CurrentLayer);

            foreach (CadLayer layer in db.LayerList)
            {
                if (layer.ID == db.CurrentLayerID)
                {
                    continue;
                }

                search(db, layer);
            }
        }

        public void search(CadPoint p, CadObjectDB db, CadLayer layer)
        {
            TargetPoint = p;
            search(db, layer);
        }

        public void search(CadObjectDB db, CadLayer layer)
        {
            if (layer == null)
            {
                return;
            }

            if (!layer.Visible)
            {
                return;
            }

            minDist = Double.MaxValue;

            IEnumerable<CadFigure> list = layer.FigureList;
            foreach (CadFigure fig in list.Reverse())
            {
                checkFig(layer, fig);
            }
        }

        private void checkSeg(uint layerID, CadFigure fig, int idxA, int idxB, CadPoint a, CadPoint b)
        {
            if (fig!=null && isIgnore(fig.ID, idxA))
            {
                return;
            }

            if (fig != null && isIgnore(fig.ID, idxB))
            {
                return;
            }

            CrossInfo ret = CadUtil.getPerpCrossSeg(a, b, TargetPoint);

            if (!ret.isCross)
            {
                return;
            }

            CadPoint d = ret.CrossPoint - TargetPoint;

            double dist = d.norm();


            if (dist > mRange)
            {
                return;
            }

            if (dist < minDist)
            {
                seg.LayerID = layerID;
                seg.Figure = fig;
                seg.PtIndexA = idxA;
                seg.PtIndexB = idxB;
                seg.CrossPoint = ret.CrossPoint;
                seg.Distance = dist;

                seg.pA = a;
                seg.pB = b;

                minDist = dist;
            }
        }

        private void checkCircle(CadLayer layer, CadFigure fig)
        {
            if (isIgnore(fig.ID, 0))
            {
                return;
            }

            if (isIgnore(fig.ID, 1))
            {
                return;
            }

            CadPoint c = fig.getPointAt(0);
            CadPoint a = fig.getPointAt(1);

            double r = CadUtil.segNorm2D(a, c);
            double tr = CadUtil.segNorm2D(TargetPoint, c);

            double dist = Math.Abs(tr - r);

            if (dist > mRange * 2.0)
            {
                return;
            }

            CadPoint td = TargetPoint - c;

            td *= (r / tr);
            td += c;


            if (dist < minDist)
            {
                seg.LayerID = layer.ID;
                seg.Figure = fig;
                seg.PtIndexA = 0;
                seg.PtIndexB = 1;
                seg.CrossPoint = td;
                seg.Distance = dist;

                seg.pA = c;
                seg.pB = a;

                minDist = dist;
            }
        }

        private void checkSegs(CadLayer layer, CadFigure fig)
        {
            IReadOnlyList<CadPoint> pl = fig.PointList;

            int num = pl.Count;

            if (num < 2)
            {
                return;
            }

            CadPoint a;
            CadPoint b;

            int idx = 0;
            a = pl[idx];

            int ia = 0;
            int ib = 0;

            while (idx < num - 1)
            {
                ib = idx + 1;

                b = pl[ib];

                if (b.Type == CadPoint.Types.HANDLE)
                {
                    idx++;
                    continue;
                }

                if (a.Type == CadPoint.Types.BREAK)
                {
                    idx++;
                    continue;
                }

                if (isIgnoreSeg(fig.ID, idx))
                {
                    idx++;
                    continue;
                }

                checkSeg(layer.ID, fig, ia, ib, a, b);

                a = b;

                ia = ib;

                idx++;
            }

            if (fig.Closed)
            {
                b = pl[0];
                checkSeg(layer.ID, fig, pl.Count - 1, 0, a, b);
            }
        }

        private void checkFig(CadLayer layer, CadFigure fig)
        {
            switch (fig.Type)
            {
                case CadFigure.Types.LINE:
                case CadFigure.Types.POLY_LINES:
                case CadFigure.Types.RECT:
                    checkSegs(layer, fig);
                    break;
                case CadFigure.Types.CIRCLE:
                    checkCircle(layer, fig);
                    break;
                default:
                    break;
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

        private bool isIgnoreSeg(uint figId, int index)
        {
            if (IgnoreSegList == null)
            {
                return false;
            }

            foreach (MarkSeg item in IgnoreSegList)
            {
                if (item.FigureID == figId && (item.PtIndexA == index || item.PtIndexB == index))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
