using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public class CadFigurePoint : CadFigureBehavior
        {
            // Do not have data member.

            public override States GetState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return States.NOT_ENOUGH;
                }

                return States.FULL;
            }

            public CadFigurePoint()
            {
            }

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                p.Type = CadVector.Types.BREAK;
                fig.mPointList.Add(p);
            }

            public override void AddPoint(CadFigure fig, CadVector p)
            {
                if (fig.mPointList.Count > 0)
                {
                    return;
                }

                p.Type = CadVector.Types.BREAK;
                fig.mPointList.Add(p);
            }

            public override void SetPointAt(CadFigure fig, int index, CadVector pt)
            {
                if (index > 0)
                {
                    return;
                }

                pt.Type = CadVector.Types.BREAK;
                fig.mPointList[index] = pt;
            }

            public override void RemoveSelected(CadFigure fig)
            {
                fig.mPointList.RemoveAll(a => a.Selected);

                if (fig.PointCount < 1)
                {
                    fig.mPointList.Clear();
                }
            }

            public override void Draw(CadFigure fig, DrawContext dc, int pen)
            {
                drawPoint(fig, dc, pen);
            }

            public override void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
                // NOP
            }

            public override void DrawSelected(CadFigure fig, DrawContext dc, int pen)
            {
                drawSelected_Point(fig, dc, pen);
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                // NOP
            }

            private void drawPoint(CadFigure fig, DrawContext dc, int pen)
            {
                if (fig.PointList.Count == 0)
                {
                    return;
                }

                dc.Drawing.DrawCross(pen, fig.PointList[0], 4);
            }

            private void drawSelected_Point(CadFigure fig, DrawContext dc, int pen)
            {
                if (fig.PointList[0].Selected) dc.Drawing.DrawSelectedPoint(fig.PointList[0]);
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }

            public override Types EndCreate(CadFigure fig, DrawContext dc)
            {
                return fig.Type;
            }

            public override void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadVector delta)
            {
                CadVector p = fig.StoreList[0];

                if (p.Selected)
                {
                    fig.mPointList[0] = p + delta;
                    return;
                }
            }

            public override Centroid GetCentroid(CadFigure fig)
            {
                Centroid ret = default(Centroid);

                ret.Point = fig.mPointList[0];

                ret.SplitList = new List<CadFigure>();

                ret.SplitList.Add(fig);

                return ret;
            }
        }
    }
}