using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public class CadFigurePoint : CadFigure
    {
        public override CreateStates CreateState
        {
            get
            {
                return GetCreateState();
            }
        }


        private CreateStates GetCreateState()
        {
            if (PointList.Count < 1)
            {
                return CreateStates.NOT_ENOUGH;
            }

            return CreateStates.FULL;
        }

        public CadFigurePoint()
        {
            Type = Types.POINT;
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
            mPointList.Add(p);
        }

        public override void AddPoint(CadVector p)
        {
            if (mPointList.Count > 0)
            {
                return;
            }

            mPointList.Add(p);
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            if (index > 0)
            {
                return;
            }

            mPointList[index] = pt;
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 1)
            {
                mPointList.Clear();
            }
        }

        public override void Draw(DrawContext dc, int pen)
        {
            drawPoint(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, int pen, int idxA, int idxB)
        {
            // NOP
        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            drawSelected_Point(dc, pen);
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
            // NOP
        }

        private void drawPoint(DrawContext dc, int pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            dc.Drawing.DrawCross(pen, PointList[0], 4);
        }

        private void drawSelected_Point(DrawContext dc, int pen)
        {
            if (PointList.Count > 0)
            {
                if (PointList[0].Selected)
                {
                    dc.Drawing.DrawSelectedPoint(PointList[0]);
                }
            }
        }

        public override void StartCreate(DrawContext dc)
        {
            // NOP
        }

        public override void EndCreate(DrawContext dc)
        {
        }

        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            CadVector p = StoreList[0];

            if (p.Selected)
            {
                mPointList[0] = p + delta;
                return;
            }
        }

        public override Centroid GetCentroid()
        {
            Centroid ret = default(Centroid);

            ret.Point = mPointList[0];

            ret.SplitList = new List<CadFigure>();

            ret.SplitList.Add(this);

            return ret;
        }
    }
}