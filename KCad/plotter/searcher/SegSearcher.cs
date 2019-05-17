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
            CadVertex a = fseg.Point0;
            CadVertex b = fseg.Point1;

            if (fig.StoreList != null)
            {
                a = fseg.StoredPoint0;
                b = fseg.StoredPoint1;
            }

            CadVertex cwp = dc.DevPointToWorldPoint(Target.Pos);

            CadVertex xfaceNormal = dc.DevVectorToWorldVector(Target.DirX);
            CadVertex yfaceNormal = dc.DevVectorToWorldVector(Target.DirY);

            CadVertex cx = CadUtil.CrossSegPlane(a, b, cwp, xfaceNormal);
            CadVertex cy = CadUtil.CrossSegPlane(a, b, cwp, yfaceNormal);

            CadVertex pa = dc.WorldPointToDevPoint(a);
            CadVertex pb = dc.WorldPointToDevPoint(b);

            if (!cx.Valid && !cy.Valid)
            {
                return;
            }

            CadVertex p = CadVertex.InvalidValue;
            double mind = Double.MaxValue;

            CadVectorArray4 vtbl = default;

            vtbl[0] = cx;
            vtbl[1] = cy;
            vtbl.Length = 2;

            for (int i = 0; i < vtbl.Length; i++)
            {
                CadVertex v = vtbl[i];

                if (!v.Valid)
                {
                    continue;
                }

                CadVertex devv = dc.WorldPointToDevPoint(v);

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
            if (fig.PointCount < 3)
            {
                return;
            }

            VertexList vl = fig.PointList;

            if (fig.StoreList != null)
            {
                vl = fig.StoreList;
            }


            CadVertex c = vl[0];
            CadVertex a = vl[1];
            CadVertex b = vl[2];


            CadVertex pc = dc.WorldPointToDevPoint(c);
            CadVertex pa = dc.WorldPointToDevPoint(a);
            CadVertex pb = dc.WorldPointToDevPoint(b);

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
                CadVertex tp = dc.DevPointToWorldPoint(Target.Pos);
                r = CadUtil.SegNorm(a, c);
                tr = CadUtil.SegNorm(tp, c);

                CadVertex td = tp - c;

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
    }
}
