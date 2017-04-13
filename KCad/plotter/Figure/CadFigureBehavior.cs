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

            public abstract States GetState(CadFigure fig);
            public abstract void AddPointInCreating(CadFigure fig, DrawContext dc, CadPoint p);
            public abstract void AddPoint(CadFigure fig, CadPoint p);
            public abstract void SetPointAt(CadFigure fig, int index, CadPoint pt);
            public abstract void RemoveSelected(CadFigure fig);
            public abstract void Draw(CadFigure fig, DrawContext dc, int pen);
            public abstract void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB);
            public abstract void DrawSelected(CadFigure fig, DrawContext dc, int pen);
            public abstract void DrawTemp(CadFigure fig, DrawContext dc, CadPoint tp, int pen);
            public abstract void StartCreate(CadFigure fig, DrawContext dc);
            public abstract Types EndCreate(CadFigure fig, DrawContext dc);
            public abstract Centroid GetCentroid(CadFigure fig);

            public virtual void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadPoint delta)
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

            public virtual void MoveAllPoints(CadFigure fig, CadPoint delta)
            {
                CadUtil.movePoints(fig.mPointList, delta);
            }

            public virtual CadRect GetContainsRect(CadFigure fig)
            {
                return CadUtil.getContainsRect(fig.PointList);
            }

            public virtual IReadOnlyList<CadPoint> GetPoints(CadFigure fig, int curveSplitNum)
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

            public override States GetState(CadFigure fig)
            {
                return States.NONE;
            }

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadPoint p)
            {
            }

            public override void AddPoint(CadFigure fig, CadPoint p)
            {
            }

            public override void Draw(CadFigure fig, DrawContext dc, int pen)
            {
            }

            public override void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB)
            {
            }

            public override void DrawSelected(CadFigure fig, DrawContext dc, int pen)
            {
            }

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadPoint tp, int pen)
            {
            }

            public override Types EndCreate(CadFigure fig, DrawContext dc)
            {
                return fig.Type;
            }

            public override void RemoveSelected(CadFigure fig)
            {
            }

            public override void SetPointAt(CadFigure fig, int index, CadPoint pt)
            {
            }

            public override void StartCreate(CadFigure fig, DrawContext dc)
            {
            }

            public override Centroid GetCentroid(CadFigure fig)
            {
                return default(Centroid);
            }
        }
        #endregion
    }
}