using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        public abstract class CreateBehavior
        {
            public abstract void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p);
            public abstract void StartCreate(CadFigure fig, DrawContext dc);
            public abstract void EndCreate(CadFigure fig, DrawContext dc);
            public abstract void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen);
            public abstract CreateStates GetCreateState(CadFigure fig);
        }

        public class PolyLinesCreateBehavior : CreateBehavior
        {
            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                fig.mPointList.Add(p);
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointCount == 0)
                {
                    return;
                }

                CadVector lastPt = fig.PointList[fig.PointCount - 1];

                dc.Drawing.DrawLine(pen, lastPt, tp);
            }

            public override void EndCreate(CadFigure fig, DrawContext dc)
            {
                if (fig.PointList.Count > 2)
                {
                    //Vector3d normal = CadUtil.RepresentativeNormal(fig.PointList);
                    //double t = Vector3d.Dot(normal, dc.ViewDir);

                    fig.Normal = CadVector.Create(dc.ViewDir);
                    fig.Normal *= -1;
                }
            }

            public override CreateStates GetCreateState(CadFigure fig)
            {
                if (fig.PointList.Count < 2)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (fig.PointList.Count == 2)
                {
                    return CreateStates.ENOUGH;
                }
                else if (fig.PointList.Count > 2)
                {
                    return CreateStates.WAIT_NEXT_POINT;
                }

                return CreateStates.NONE;
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }
        }

        public class RectCreateBehavior : CreateBehavior
        {
            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
                if (fig.mPointList.Count == 0)
                {
                    fig.mPointList.Add(p);
                }
                else
                {
                    // 左回りになるように設定

                    CadVector pp0 = dc.CadPointToUnitPoint(fig.PointList[0]);
                    CadVector pp2 = dc.CadPointToUnitPoint(p);

                    CadVector pp1 = pp0;
                    pp1.y = pp2.y;

                    CadVector pp3 = pp0;
                    pp3.x = pp2.x;

                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp1));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp2));
                    fig.mPointList.Add(dc.UnitPointToCadPoint(pp3));

                    fig.IsLoop = true;
                }
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
                if (fig.PointList.Count <= 0)
                {
                    return;
                }

                dc.Drawing.DrawRect(pen, fig.PointList[0], tp);
            }

            public override void EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Normal = CadVector.Create(dc.ViewDir);
                fig.Normal *= -1;
                fig.Type = Types.POLY_LINES;
            }

            public override CreateStates GetCreateState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 4)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
                // NOP
            }
        }

        public class LineCreateBehavior : PolyLinesCreateBehavior
        {
            public override CreateStates GetCreateState(CadFigure fig)
            {
                if (fig.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (fig.PointList.Count < 2)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }

            public override void EndCreate(CadFigure fig, DrawContext dc)
            {
                fig.Type = Types.POLY_LINES;
            }
        }
    }
}