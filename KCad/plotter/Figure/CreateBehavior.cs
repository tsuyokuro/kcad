using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        public abstract class Creator
        {
            public virtual CadFigure Figure
            {
                get;
                set;
            } = null;


            public virtual void AddPointInCreating(DrawContext dc, CadVector p)
            {
                Figure.AddPointInCreating(dc, p);
            }

            public virtual void DrawTemp(DrawContext dc, CadVector tp, int pen)
            {
                Figure.DrawTemp(dc, tp, pen);
            }

            public virtual void EndCreate(DrawContext dc)
            {
                Figure.EndCreate(dc);
            }

            public virtual void StartCreate(DrawContext dc)
            {
                Figure.StartCreate(dc);
            }

            public abstract CreateStates GetCreateState();

            public static Creator Get(CadFigure fig)
            {
                CadFigure.Types type = fig.Type;

                Creator creator = null;

                switch (type)
                {
                    case Types.LINE:
                        creator = new LineCreator(fig);
                        break;

                    case Types.RECT:
                        creator = new RectCreator(fig);
                        break;

                    case Types.POLY_LINES:
                        creator = new PolyLinesCreator(fig);
                        break;

                    case Types.CIRCLE:
                        creator = new CircleCreator(fig);
                        break;

                    case Types.POINT:
                        creator = new PointCreator(fig);
                        break;

                    case Types.DIMENTION_LINE:
                        creator = new DimLineCreator(fig);
                        break;

                    default:
                        break;
                }

                return creator;
            }
        }

        public class PolyLinesCreator : Creator
        {
            public PolyLinesCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override void AddPointInCreating(DrawContext dc, CadVector p)
            {
                Figure.mPointList.Add(p);
            }

            public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
            {
                if (Figure.PointCount == 0)
                {
                    return;
                }

                CadVector lastPt = Figure.PointList[Figure.PointCount - 1];

                dc.Drawing.DrawLine(pen, lastPt, tp);
            }

            public override void EndCreate(DrawContext dc)
            {
                if (Figure.PointList.Count > 2)
                {
                    //Vector3d normal = CadUtil.RepresentativeNormal(fig.PointList);
                    //double t = Vector3d.Dot(normal, dc.ViewDir);

                    Figure.Normal = CadVector.Create(dc.ViewDir);
                    Figure.Normal *= -1;
                }
            }

            public override CreateStates GetCreateState()
            {
                if (Figure.PointList.Count < 2)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count == 2)
                {
                    return CreateStates.ENOUGH;
                }
                else if (Figure.PointList.Count > 2)
                {
                    return CreateStates.WAIT_NEXT_POINT;
                }

                return CreateStates.NONE;
            }

            public override void StartCreate(DrawContext dc)
            {
                // NOP
            }
        }

        public class RectCreator : Creator
        {
            public RectCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override void AddPointInCreating(DrawContext dc, CadVector p)
            {
                if (Figure.mPointList.Count == 0)
                {
                    Figure.mPointList.Add(p);
                }
                else
                {
                    // 左回りになるように設定

                    CadVector pp0 = dc.CadPointToUnitPoint(Figure.PointList[0]);
                    CadVector pp2 = dc.CadPointToUnitPoint(p);

                    CadVector pp1 = pp0;
                    pp1.y = pp2.y;

                    CadVector pp3 = pp0;
                    pp3.x = pp2.x;

                    Figure.mPointList.Add(dc.UnitPointToCadPoint(pp1));
                    Figure.mPointList.Add(dc.UnitPointToCadPoint(pp2));
                    Figure.mPointList.Add(dc.UnitPointToCadPoint(pp3));

                    Figure.IsLoop = true;
                }
            }

            public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
            {
                if (Figure.PointList.Count <= 0)
                {
                    return;
                }

                dc.Drawing.DrawRect(pen, Figure.PointList[0], tp);
            }

            public override void EndCreate(DrawContext dc)
            {
                Figure.Normal = CadVector.Create(dc.ViewDir);
                Figure.Normal *= -1;
                Figure.Type = Types.POLY_LINES;
            }

            public override void StartCreate(DrawContext dc)
            {
                // NOP
            }

            public override CreateStates GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 4)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }
        }

        public class LineCreator : PolyLinesCreator
        {
            public LineCreator(CadFigure fig) : base(fig)
            {
            }

            public override void EndCreate(DrawContext dc)
            {
                Figure.Type = Types.POLY_LINES;
            }

            public override CreateStates GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 2)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }
        }

        public class CircleCreator : Creator
        {
            public CircleCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override CreateStates GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 2)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }
        }

        public class DimLineCreator : Creator
        {
            public DimLineCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override CreateStates GetCreateState()
            {
                if (Figure.PointList.Count < 2)
                {
                    return CreateStates.NOT_ENOUGH;
                }
                else if (Figure.PointList.Count < 3)
                {
                    return CreateStates.WAIT_LAST_POINT;
                }

                return CreateStates.FULL;
            }
        }

        public class PointCreator : Creator
        {
            public PointCreator(CadFigure fig)
            {
                Figure = fig;
            }

            public override CreateStates GetCreateState()
            {
                if (Figure.PointList.Count < 1)
                {
                    return CreateStates.NOT_ENOUGH;
                }

                return CreateStates.FULL;
            }
        }
    }
}