﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Plotter
{
    public class SegSearcher
    {
        private MarkSeg seg;

        private CadVector TargetPoint;
        private double mRange;
        private double minDist = 0;

        private List<MarkSeg> IgnoreSegList;

        private List<SelectItem> IgnoreList = null;

        public void SetRangePixel(DrawContext dc, double pixel)
        {
            //double d = dc.pixelsToMilli(pixel);
            mRange = pixel;
        }

        public void Clean()
        {
            seg = default(MarkSeg);
        }

        public void SetTargetPoint(CadVector p)
        {
            TargetPoint = p;
        }

        public void SetIgnoreList(List<SelectItem> list)
        {
            IgnoreList = list;
        }

        public void SetIgnoreSeg(List<MarkSeg> segList)
        {
            IgnoreSegList = segList;
        }

        public MarkSeg GetMatch()
        {
            return seg;
        }

        public void SearchAllLayer(DrawContext dc, CadVector p, CadObjectDB db)
        {
            TargetPoint = p;
            SearchAllLayer(dc, db);
        }

        public void SearchAllLayer(DrawContext dc, CadObjectDB db)
        {
            Search(dc, db, db.CurrentLayer);

            foreach (CadLayer layer in db.LayerList)
            {
                if (layer.ID == db.CurrentLayerID)
                {
                    continue;
                }

                Search(dc, db, layer);
            }
        }

        public void Search(DrawContext dc, CadVector p, CadObjectDB db, CadLayer layer)
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

            if (!layer.Visible)
            {
                return;
            }

            minDist = CadConst.MaxValue;

            IEnumerable<CadFigure> list = layer.FigureList;
            foreach (CadFigure fig in list.Reverse())
            {
                CheckFig(dc, layer, fig);
            }
        }

        private void CheckSeg(DrawContext dc, uint layerID, CadFigure fig, int idxA, int idxB, CadVector a, CadVector b)
        {
            if (fig != null && IsIgnore(fig.ID, idxA))
            {
                return;
            }

            if (fig != null && IsIgnore(fig.ID, idxB))
            {
                return;
            }

            CadVector pa = dc.CadPointToUnitPoint(a);
            CadVector pb = dc.CadPointToUnitPoint(b);

            CrossInfo ret = CadUtil.PerpendicularCrossSeg2D(pa, pb, TargetPoint);

            if (!ret.IsCross)
            {
                return;
            }

            CadVector d = ret.CrossPoint - TargetPoint;

            double dist = d.Norm();


            if (dist > mRange)
            {
                return;
            }

            if (dist < minDist)
            {
                CadVector tp = dc.UnitPointToCadPoint(TargetPoint);
                CrossInfo ret3d = CadUtil.PerpendicularCrossLine(a, b, tp);

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

            CadVector c = fig.GetPointAt(0);
            CadVector a = fig.GetPointAt(1);
            CadVector b = fig.GetPointAt(2);

            CadVector pc = dc.CadPointToUnitPoint(c);
            CadVector pa = dc.CadPointToUnitPoint(a);
            CadVector pb = dc.CadPointToUnitPoint(b);

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
                CadVector tp = dc.UnitPointToCadPoint(TargetPoint);
                r = CadUtil.segNorm(a, c);
                tr = CadUtil.segNorm(tp, c);

                CadVector td = tp - c;

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
                seg.pB = fig.GetPointAt(idxB);

                minDist = dist;
            }
        }

        private void CheckSegs(DrawContext dc, CadLayer layer, CadFigure fig)
        {
            IReadOnlyList<CadVector> pl = fig.PointList;

            int num = pl.Count;

            if (num < 2)
            {
                return;
            }

            CadVector a;
            CadVector b;

            int idx = 0;
            a = pl[idx];

            int ia = 0;
            int ib = 0;

            while (idx < num - 1)
            {
                ib = idx + 1;

                b = pl[ib];

                if (b.Type == CadVector.Types.HANDLE)
                {
                    idx++;
                    continue;
                }

                if (a.Type == CadVector.Types.BREAK)
                {
                    idx++;
                    continue;
                }

                if (IsIgnoreSeg(fig.ID, idx))
                {
                    idx++;
                    continue;
                }

                CheckSeg(dc, layer.ID, fig, ia, ib, a, b);

                a = b;

                ia = ib;

                idx++;
            }

            if (fig.Closed)
            {
                b = pl[0];
                CheckSeg(dc, layer.ID, fig, pl.Count - 1, 0, a, b);
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

            foreach (SelectItem item in IgnoreList)
            {
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
