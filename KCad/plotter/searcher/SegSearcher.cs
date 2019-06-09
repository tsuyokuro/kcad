using System;
using System.Collections.Generic;
using System.Linq;
using CadDataTypes;
using OpenTK;

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

            if (fig.StoreList != null && fig.StoreList.Count > 1)
            {
                a = fseg.StoredPoint0;
                b = fseg.StoredPoint1;
            }

            Vector3d cwp = dc.DevPointToWorldPoint(Target.Pos);

            Vector3d xfaceNormal = dc.DevVectorToWorldVector(Target.DirX);
            Vector3d yfaceNormal = dc.DevVectorToWorldVector(Target.DirY);

            Vector3d cx = CadUtil.CrossSegPlane(a.vector, b.vector, cwp, xfaceNormal);
            Vector3d cy = CadUtil.CrossSegPlane(a.vector, b.vector, cwp, yfaceNormal);

            CadVertex pa = dc.WorldPointToDevPoint(a);
            CadVertex pb = dc.WorldPointToDevPoint(b);

            if (!cx.IsValid() && !cy.IsValid())
            {
                return;
            }

            Vector3d p = VectorUtil.InvalidVector3d;
            double mind = Double.MaxValue;

            StackArray<Vector3d> vtbl = default;

            vtbl[0] = cx;
            vtbl[1] = cy;
            vtbl.Length = 2;

            for (int i = 0; i < vtbl.Length; i++)
            {
                Vector3d v = vtbl[i];

                if (!v.IsValid())
                {
                    continue;
                }

                Vector3d devv = dc.WorldPointToDevPoint(v);

                double td = (devv - Target.Pos).Norm();

                if (td < mind)
                {
                    mind = td;
                    p = v;
                }
            }

            if (!p.IsValid())
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


            Vector3d c = vl[0].vector;
            Vector3d a = vl[1].vector;
            Vector3d b = vl[2].vector;


            Vector3d pc = dc.WorldPointToDevPoint(c);
            Vector3d pa = dc.WorldPointToDevPoint(a);
            Vector3d pb = dc.WorldPointToDevPoint(b);

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
                Vector3d tp = dc.DevPointToWorldPoint(Target.Pos);
                r = CadUtil.SegNorm(a, c);
                tr = CadUtil.SegNorm(tp, c);

                Vector3d td = tp - c;

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
