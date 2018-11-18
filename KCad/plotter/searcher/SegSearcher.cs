using System;
using System.Collections.Generic;
using System.Linq;
using CadDataTypes;

namespace Plotter
{
    public class SegSearcher
    {
        private MarkSegment MarkSeg;

        private CadCursor Target;

        public double Range;

        public double MinDist = 0;

        private List<MarkSegment> IgnoreSegList;

        private IReadOnlyList<MarkPoint> IgnoreList = null;

        public bool IsMatch
        {
            get
            {
                return MarkSeg.FigureID != 0;
            }
        }


        public void SetRangePixel(DrawContext dc, double pixel)
        {
            Range = pixel;
        }

        public void Clean()
        {
            MarkSeg = default(MarkSegment);
            MarkSeg.Clean();
        }

        public void SetTargetPoint(CadCursor cursor)
        {
            Target = cursor;
        }

        public void SetIgnoreList(IReadOnlyList<MarkPoint> list)
        {
            IgnoreList = list; 
        }

        public void SetIgnoreSeg(List<MarkSegment> segList)
        {
            IgnoreSegList = segList;
        }

        public MarkSegment GetMatch()
        {
            return MarkSeg;
        }

        public void SearchAllLayer(DrawContext dc, CadObjectDB db)
        {
            Search(dc, db, db.CurrentLayer);

            for (int i=0; i<db.LayerList.Count; i++)
            {
                CadLayer layer = db.LayerList[i];

                if (layer.ID == db.CurrentLayerID)
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

            if (!layer.Visible)
            {
                return;
            }

            MinDist = CadConst.MaxValue;

            for (int i=layer.FigureList.Count-1; i>=0; i--)
            {
                CadFigure fig = layer.FigureList[i];
                CheckFig(dc, layer, fig);
            }
        }

        private void CheckSeg(DrawContext dc, CadLayer layer, FigureSegment fseg)
        {
            CadFigure fig = fseg.Figure;
            int idxA = fseg.Index0;
            int idxB = fseg.Index1;
            CadVector a = fseg.Point0;
            CadVector b = fseg.Point1;

            if (fig.StoreList != null)
            {
                a = fseg.StoredPoint0;
                b = fseg.StoredPoint1;
            }

            if (fig != null && IsIgnore(fig.ID, idxA))
            {
                return;
            }

            if (fig != null && IsIgnore(fig.ID, idxB))
            {
                return;
            }

            CadVector cwp = dc.DevPointToWorldPoint(Target.Pos);

            CadVector xfaceNormal = dc.DevVectorToWorldVector(Target.DirX);
            CadVector yfaceNormal = dc.DevVectorToWorldVector(Target.DirY);

            CadVector cx = CadUtil.CrossSegPlane(a, b, cwp, xfaceNormal);
            CadVector cy = CadUtil.CrossSegPlane(a, b, cwp, yfaceNormal);

            CadVector pa = dc.WorldPointToDevPoint(a);
            CadVector pb = dc.WorldPointToDevPoint(b);

            if (!cx.Valid && !cy.Valid)
            {
                return;
            }

            CadVector p = CadVector.InvalidValue;
            double mind = Double.MaxValue;

            Span<CadVector> vtbl = stackalloc CadVector[] { cx, cy };

            for (int i = 0; i < vtbl.Length; i++)
            {
                CadVector v = vtbl[i];

                if (!v.Valid)
                {
                    continue;
                }

                CadVector devv = dc.WorldPointToDevPoint(v);

                double td = (devv - Target.Pos).Norm();

                if (td < mind)
                {
                    mind = td;
                    p = v;
                }
            }

            if (!p.Valid)
            {
                return;
            }

            if (mind > Range)
            {
                return;
            }

            if (mind < MinDist)
            {
                MarkSeg.Layer = layer;
                MarkSeg.FigSeg = fseg;
                MarkSeg.CrossPoint = p;
                MarkSeg.CrossPointScrn = dc.WorldPointToDevPoint(p);
                MarkSeg.Distance = mind;

                MinDist = mind;
            }
        }


        private void CheckCircle(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            if (IsIgnore(fig.ID, 0))
            {
                return;
            }

            if (IsIgnore(fig.ID, 1))
            {
                return;
            }

            if (IsIgnore(fig.ID, 2))
            {
                return;
            }

            if (fig.PointCount < 3)
            {
                return;
            }

            CadVector c = fig.GetPointAt(0);
            CadVector a = fig.GetPointAt(1);
            CadVector b = fig.GetPointAt(2);

            CadVector pc = dc.WorldPointToDevPoint(c);
            CadVector pa = dc.WorldPointToDevPoint(a);
            CadVector pb = dc.WorldPointToDevPoint(b);

            double r = CadUtil.SegNorm2D(pa, pc);
            double tr = CadUtil.SegNorm2D(Target.Pos, pc);

            double pad = CadUtil.SegNorm2D(Target.Pos, pa);
            double pbd = CadUtil.SegNorm2D(Target.Pos, pb);

            int idxB = 1;

            if (pbd < pad)
            {
                idxB = 2;
            }


            double dist = Math.Abs(tr - r);

            if (dist > Range * 2.0)
            {
                return;
            }

            if (dist < MinDist)
            {
                CadVector tp = dc.DevPointToWorldPoint(Target.Pos);
                r = CadUtil.SegNorm(a, c);
                tr = CadUtil.SegNorm(tp, c);

                CadVector td = tp - c;

                td *= (r / tr);
                td += c;

                FigureSegment fseg = new FigureSegment(fig, 0, 0, idxB);

                MarkSeg.Layer = layer;
                MarkSeg.FigSeg = fseg;
                MarkSeg.CrossPoint = td;
                MarkSeg.CrossPointScrn = dc.WorldPointToDevPoint(td);
                MarkSeg.Distance = dist;


                MinDist = dist;
            }
        }

        private void CheckSegs(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            for (int i=0;i < fig.SegmentCount; i++)
            {
                FigureSegment seg = fig.GetFigSegmentAt(i);
                CheckSeg(dc, layer, seg);
            }
        }

        private void CheckFig(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            switch (fig.Type)
            {
                case CadFigure.Types.LINE:
                case CadFigure.Types.POLY_LINES:
                case CadFigure.Types.RECT:
                case CadFigure.Types.DIMENTION_LINE:
                case CadFigure.Types.MESH:
                    CheckSegs(dc, layer, fig);
                    break;
                case CadFigure.Types.CIRCLE:
                    CheckCircle(dc, layer, fig);
                    break;
                default:
                    break;
            }
        }

        private bool IsIgnore(uint figId, int index)
        {
            if (IgnoreList == null)
            {
                return false;
            }

            for (int i=0; i<IgnoreList.Count; i++)
            {
                MarkPoint item = IgnoreList[i];

                if (item.FigureID == figId && item.PointIndex == index)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIgnoreSeg(uint figId, int index)
        {
            if (IgnoreSegList == null)
            {
                return false;
            }

            for (int i=0; i<IgnoreSegList.Count; i++)
            {
                MarkSegment item = IgnoreSegList[i];

                if (item.FigureID == figId && (item.PtIndexA == index || item.PtIndexB == index))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
