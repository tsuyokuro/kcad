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
            public CadFigureBehavior()
            {
            }

            public abstract States GetState(CadFigure fig);
            public abstract void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p);
            public abstract void AddPoint(CadFigure fig, CadVector p);
            public abstract void SetPointAt(CadFigure fig, int index, CadVector pt);
            public abstract void RemoveSelected(CadFigure fig);
            public abstract void Draw(CadFigure fig, DrawContext dc, int pen);
            public abstract void DrawSeg(CadFigure fig, DrawContext dc, int pen, int idxA, int idxB);
            public abstract void DrawSelected(CadFigure fig, DrawContext dc, int pen);
            public abstract void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen);
            public abstract void StartCreate(CadFigure fig, DrawContext dc);
            public abstract Types EndCreate(CadFigure fig, DrawContext dc);
            public abstract Centroid GetCentroid(CadFigure fig);

            public virtual void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadVector delta)
            {
                if (fig.StoreList == null)
                {
                    return;
                }

                for (int i = 0; i < fig.StoreList.Count; i++)
                {
                    CadVector op = fig.StoreList[i];

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

            public virtual void MoveAllPoints(CadFigure fig, CadVector delta)
            {
                CadUtil.MovePoints(fig.mPointList, delta);
            }

            public virtual CadRect GetContainsRect(CadFigure fig)
            {
                return CadUtil.GetContainsRect(fig.PointList);
            }

            public virtual CadRect GetContainsRectScrn(CadFigure fig, DrawContext dc)
            {
                return CadUtil.GetContainsRectScrn(dc, fig.PointList);
            }

            public virtual List<CadVector> GetPoints(CadFigure fig, int curveSplitNum)
            {
                return fig.PointList;
            }

            public virtual void StartEdit(CadFigure fig)
            {

            }

            public virtual void EndEdit(CadFigure fig)
            {

            }

            public virtual void CancelEdit(CadFigure fig)
            {

            }

            public virtual void RecalcNormal(CadFigure fig)
            {

            }

            public virtual CadSegment GetSegmentAt(CadFigure fig, int n )
            {
                if ( n < fig.mPointList.Count - 1)
                {
                    return new CadSegment(fig.mPointList[n], fig.mPointList[n + 1]);
                }

                if (n == fig.mPointList.Count - 1 && fig.IsLoop)
                {
                    return new CadSegment(fig.mPointList[n], fig.mPointList[0]);
                }

                throw new System.ArgumentException("GetSegmentAt", "bad index");
            }

            public virtual FigureSegment GetFigSegmentAt(CadFigure fig, int n)
            {
                if (n < fig.mPointList.Count - 1)
                {
                    return new FigureSegment(fig, n, n, n + 1);
                }

                if (n == fig.mPointList.Count - 1 && fig.IsLoop)
                {
                    return new FigureSegment(fig, n, n, 0);
                }

                throw new System.ArgumentException("GetFigSegmentAt", "bad index");
            }

            public virtual int SegmentCount(CadFigure fig)
            {
                int cnt = fig.mPointList.Count - 1;

                if (fig.IsLoop)
                {
                    cnt++;
                }

                return cnt;
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

            public override void AddPointInCreating(CadFigure fig, DrawContext dc, CadVector p)
            {
            }

            public override void AddPoint(CadFigure fig, CadVector p)
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

            public override void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen)
            {
            }

            public override Types EndCreate(CadFigure fig, DrawContext dc)
            {
                return fig.Type;
            }

            public override void RemoveSelected(CadFigure fig)
            {
            }

            public override void SetPointAt(CadFigure fig, int index, CadVector pt)
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