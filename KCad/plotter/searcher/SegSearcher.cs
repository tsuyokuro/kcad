using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
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

        public CadPoint CrossViewPoint;

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
            //double d = dc.pixelsToMilli(pixel);
            mRange = pixel;
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

        public void searchAllLayer(DrawContext dc, CadPoint p, CadObjectDB db)
        {
            TargetPoint = p;
            searchAllLayer(dc, db);
        }

        public void searchAllLayer(DrawContext dc, CadObjectDB db)
        {
            search(dc, db, db.CurrentLayer);

            foreach (CadLayer layer in db.LayerList)
            {
                if (layer.ID == db.CurrentLayerID)
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

            if (!layer.Visible)
            {
                return;
            }

            minDist = CadConst.MaxValue;

            IEnumerable<CadFigure> list = layer.FigureList;
            foreach (CadFigure fig in list.Reverse())
            {
                checkFig(dc, layer, fig);
            }
        }

        private void checkSeg(DrawContext dc, uint layerID, CadFigure fig, int idxA, int idxB, CadPoint a, CadPoint b)
        {
            if (fig != null && isIgnore(fig.ID, idxA))
            {
                return;
            }

            if (fig != null && isIgnore(fig.ID, idxB))
            {
                return;
            }

            CadPoint pa = dc.CadPointToUnitPoint(a);
            CadPoint pb = dc.CadPointToUnitPoint(b);

            CrossInfo ret = CadUtil.getPerpCrossSeg2D(pa, pb, TargetPoint);

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
                CadPoint tp = dc.UnitPointToCadPoint(TargetPoint);
                CrossInfo ret3d = CadUtil.getPerpCrossLine(a, b, tp);

                seg.LayerID = layerID;
                seg.Figure = fig;
                seg.PtIndexA = idxA;
                seg.PtIndexB = idxB;
                seg.CrossPoint = ret3d.CrossPoint;
                seg.CrossViewPoint = ret.CrossPoint;
                seg.Distance = dist;

                seg.pA = a;
                seg.pB = b;

                minDist = dist;
            }
        }

        private void checkCircle(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            if (isIgnore(fig.ID, 0))
            {
                return;
            }

            if (isIgnore(fig.ID, 1))
            {
                return;
            }

            if (isIgnore(fig.ID, 2))
            {
                return;
            }

            CadPoint c = fig.getPointAt(0);
            CadPoint a = fig.getPointAt(1);
            CadPoint b = fig.getPointAt(2);

            CadPoint pc = dc.CadPointToUnitPoint(c);
            CadPoint pa = dc.CadPointToUnitPoint(a);
            CadPoint pb = dc.CadPointToUnitPoint(b);

            double r = CadUtil.segNorm2D(pa, pc);
            double tr = CadUtil.segNorm2D(TargetPoint, pc);

            double pad = CadUtil.segNorm2D(TargetPoint, pa);
            double pbd = CadUtil.segNorm2D(TargetPoint, pb);

            int idxB = 1;

            if (pbd < pad)
            {
                idxB = 2;
            }


            double dist = Math.Abs(tr - r);

            if (dist > mRange * 2.0)
            {
                return;
            }

            if (dist < minDist)
            {
                CadPoint tp = dc.UnitPointToCadPoint(TargetPoint);
                r = CadUtil.segNorm(a, c);
                tr = CadUtil.segNorm(tp, c);

                CadPoint td = tp - c;

                td *= (r / tr);
                td += c;

                seg.LayerID = layer.ID;
                seg.Figure = fig;
                seg.PtIndexA = 0;
                seg.PtIndexB = idxB;
                seg.CrossPoint = td;
                seg.CrossViewPoint = dc.CadPointToUnitPoint(td);
                seg.Distance = dist;

                seg.pA = c;
                seg.pB = fig.getPointAt(idxB);

                minDist = dist;
            }
        }

        private void checkSegs(DrawContext dc, CadLayer layer, CadFigure fig)
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

                checkSeg(dc, layer.ID, fig, ia, ib, a, b);

                a = b;

                ia = ib;

                idx++;
            }

            if (fig.Closed)
            {
                b = pl[0];
                checkSeg(dc, layer.ID, fig, pl.Count - 1, 0, a, b);
            }
        }

        private void checkFig(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            switch (fig.Type)
            {
                case CadFigure.Types.LINE:
                case CadFigure.Types.POLY_LINES:
                case CadFigure.Types.RECT:
                    checkSegs(dc, layer, fig);
                    break;
                case CadFigure.Types.CIRCLE:
                    checkCircle(dc, layer, fig);
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
