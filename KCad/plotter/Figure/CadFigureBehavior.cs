using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public partial class CadFigure
    {
        [Serializable]
        public abstract class CadFigureBehavior
        {
            // Do not have data member.

            public CadFigureBehavior()
            {
            }

            public abstract States getState(CadFigure fig);
            public abstract void addPoint(CadFigure fig, CadPoint p);
            public abstract void setPointAt(CadFigure fig, int index, CadPoint pt);
            public abstract void removeSelected(CadFigure fig);
            public abstract void draw(CadFigure fig, DrawContext dc, Pen pen);
            public abstract void drawSeg(CadFigure fig, DrawContext dc, Pen pen, int idxA, int idxB);
            public abstract void drawSelected(CadFigure fig, DrawContext dc, Pen pen);
            public abstract void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, Pen pen);
            public abstract void startCreate(CadFigure fig);
            public abstract Types endCreate(CadFigure fig);

            public virtual void moveSelectedPoint(CadFigure fig, CadPoint delta)
            {
                for (int i = 0; i < fig.StoreList.Count; i++)
                {
                    CadPoint op = fig.StoreList[i];

                    if (!op.Selected)
                    {
                        continue;
                    }

                    if (i < fig.PointList.Count)
                    {
                        fig.mPointList[i] = op + delta;
                    }
                }
            }

            public virtual void moveAllPoints(CadFigure fig, CadPoint delta)
            {
                CadUtil.movePoints(fig.mPointList, delta);
            }

            public virtual CadRect getContainsRect(CadFigure fig)
            {
                return CadUtil.getContainsRect(fig.PointList);
            }

            public virtual IReadOnlyList<CadPoint> getPoints(CadFigure fig, int curveSplitNum)
            {
                return fig.PointList;
            }
        }

        #region Nop Behavior
        public class CadNopBehavior : CadFigureBehavior
        {
            public CadNopBehavior()
            {
            }

            public override States getState(CadFigure fig)
            {
                return States.NONE;
            }

            public override void addPoint(CadFigure fig, CadPoint p)
            {
            }

            public override void draw(CadFigure fig, DrawContext dc, Pen pen)
            {
            }

            public override void drawSeg(CadFigure fig, DrawContext dc, Pen pen, int idxA, int idxB)
            {
            }

            public override void drawSelected(CadFigure fig, DrawContext dc, Pen pen)
            {
            }

            public override void drawTemp(CadFigure fig, DrawContext dc, CadPoint tp, Pen pen)
            {
            }

            public override Types endCreate(CadFigure fig)
            {
                return fig.Type;
            }

            public override void removeSelected(CadFigure fig)
            {
            }

            public override void setPointAt(CadFigure fig, int index, CadPoint pt)
            {
            }

            public override void startCreate(CadFigure fig)
            {
            }
        }
        #endregion
    }
}