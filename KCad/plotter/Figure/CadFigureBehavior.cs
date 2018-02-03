﻿using System;
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
            public abstract Types EndCreate(CadFigure fig, DrawContext dc);
            public abstract void DrawTemp(CadFigure fig, DrawContext dc, CadVector tp, int pen);
            public abstract CreateStates GetCreateState(CadFigure fig);
        }

        public class Util
        {
            public static void MoveSelectedPoint(CadFigure fig, DrawContext dc, CadVector delta)
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

                    if (i < fig.mPointList.Count)
                    {
                        fig.mPointList[i] = op + delta;
                    }
                }
            }

            public static void MoveAllPoints(CadFigure fig, CadVector delta)
            {
                CadUtil.MovePoints(fig.mPointList, delta);
            }

            public static CadRect GetContainsRect(CadFigure fig)
            {
                return CadUtil.GetContainsRect(fig.mPointList);
            }

            public static CadRect GetContainsRectScrn(CadFigure fig, DrawContext dc)
            {
                return CadUtil.GetContainsRectScrn(dc, fig.mPointList);
            }

            public static VectorList GetPoints(CadFigure fig, int curveSplitNum)
            {
                return fig.mPointList;
            }

            public static CadVector GetPointAt(CadFigure fig, int idx)
            {
                return fig.mPointList[idx];
            }

            public static void SetPointAt(CadFigure fig, int index, CadVector pt)
            {
                fig.mPointList[index] = pt;
            }

            public static void SelectPointAt(CadFigure fig, int index, bool sel)
            {
                CadVector p = fig.mPointList[index];
                p.Selected = sel;
                fig.mPointList[index] = p;
            }

            public static CadSegment GetSegmentAt(CadFigure fig, int n)
            {
                if (n < fig.mPointList.Count - 1)
                {
                    return new CadSegment(fig.mPointList[n], fig.mPointList[n + 1]);
                }

                if (n == fig.mPointList.Count - 1 && fig.IsLoop)
                {
                    return new CadSegment(fig.mPointList[n], fig.mPointList[0]);
                }

                throw new System.ArgumentException("GetSegmentAt", "bad index");
            }

            public static FigureSegment GetFigSegmentAt(CadFigure fig, int n)
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

            public static int SegmentCount(CadFigure fig)
            {
                int cnt = fig.mPointList.Count - 1;

                if (fig.IsLoop)
                {
                    cnt++;
                }

                return cnt;
            }
        }
    }
}