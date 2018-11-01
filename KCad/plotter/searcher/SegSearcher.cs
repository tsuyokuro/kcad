using System;
using System.Collections.Generic;
using System.Linq;
using CadDataTypes;

namespace Plotter
{
    public class SegSearcher
    {
        private MarkSegment markSeg;

        private CadCursor TargetPoint;
        private double mRange;
        private double mMinDist = 0;

        private List<MarkSegment> IgnoreSegList;

        private IReadOnlyList<SelectItem> IgnoreList = null;

        public bool IsMatch
        {
            get
            {
                return markSeg.FigureID != 0;
            }
        }


        public void SetRangePixel(DrawContext dc, double pixel)
        {
            mRange = pixel;
        }

        public void Clean()
        {
            markSeg = default(MarkSegment);
            markSeg.Clean();
        }

        public void SetTargetPoint(CadCursor p)
        {
            TargetPoint = p;
        }

        public void SetIgnoreList(IReadOnlyList<SelectItem> list)
        {
            IgnoreList = list; 
        }

        public void SetIgnoreSeg(List<MarkSegment> segList)
        {
            IgnoreSegList = segList;
        }

        public MarkSegment GetMatch()
        {
            return markSeg;
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

            mMinDist = CadConst.MaxValue;

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

            CadVector cwp = dc.UnitPointToCadPoint(TargetPoint.Pos);

            CadVector xfaceNormal = dc.UnitVectorToCadVector(TargetPoint.DirX);
            CadVector yfaceNormal = dc.UnitVectorToCadVector(TargetPoint.DirY);

            CadVector cx = CadUtil.CrossSegPlane(a, b, cwp, xfaceNormal);
            CadVector cy = CadUtil.CrossSegPlane(a, b, cwp, yfaceNormal);

            CadVector pa = dc.CadPointToUnitPoint(a);
            CadVector pb = dc.CadPointToUnitPoint(b);

            if (!cx.Valid && !cy.Valid)
            {
                return;
            }

            CadVector p = CadVector.InvalidValue;
            double mind = Double.MaxValue;
            CadVector[] vtbl = new CadVector[] { cx, cy };

            for (int i = 0; i < vtbl.Length; i++)
            {
                CadVector v = vtbl[i];

                if (!v.Valid)
                {
                    continue;
                }

                CadVector devv = dc.CadPointToUnitPoint(v);

                double td = (devv - TargetPoint.Pos).Norm();

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

            if (mind > mRange)
            {
                return;
            }

            if (mind < mMinDist)
            {
                markSeg.Layer = layer;
                markSeg.FigSeg = fseg;
                markSeg.CrossPoint = p;
                markSeg.CrossPointScrn = dc.CadPointToUnitPoint(p);
                markSeg.Distance = mind;

                mMinDist = mind;
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

            CadVector pc = dc.CadPointToUnitPoint(c);
            CadVector pa = dc.CadPointToUnitPoint(a);
            CadVector pb = dc.CadPointToUnitPoint(b);

            double r = CadUtil.SegNorm2D(pa, pc);
            double tr = CadUtil.SegNorm2D(TargetPoint.Pos, pc);

            double pad = CadUtil.SegNorm2D(TargetPoint.Pos, pa);
            double pbd = CadUtil.SegNorm2D(TargetPoint.Pos, pb);

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

            if (dist < mMinDist)
            {
                CadVector tp = dc.UnitPointToCadPoint(TargetPoint.Pos);
                r = CadUtil.SegNorm(a, c);
                tr = CadUtil.SegNorm(tp, c);

                CadVector td = tp - c;

                td *= (r / tr);
                td += c;

                FigureSegment fseg = new FigureSegment(fig, 0, 0, idxB);

                markSeg.Layer = layer;
                markSeg.FigSeg = fseg;
                markSeg.CrossPoint = td;
                markSeg.CrossPointScrn = dc.CadPointToUnitPoint(td);
                markSeg.Distance = dist;


                mMinDist = dist;
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
                SelectItem item = IgnoreList[i];

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
