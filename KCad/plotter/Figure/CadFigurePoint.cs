﻿using System;
using System.Collections.Generic;
using System.Drawing;
using CadDataTypes;

namespace Plotter
{
    public class CadFigurePoint : CadFigure
    {
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

        public override void Draw(DrawContext dc, DrawPen pen)
        {
            drawPoint(dc, pen);
        }

        public override void DrawSeg(DrawContext dc, DrawPen pen, int idxA, int idxB)
        {
            // NOP
        }

        public override void DrawSelected(DrawContext dc, DrawPen pen)
        {
            drawSelected_Point(dc, pen);
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, DrawPen pen)
        {
            // NOP
        }

        public override void InvertDir()
        {
            // NOP
        }

        private void drawPoint(DrawContext dc, DrawPen pen)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            double size = dc.DevSizeToWoldSize(4);

            dc.Drawing.DrawCross(pen, PointList[0], size);
        }

        private void drawSelected_Point(DrawContext dc, DrawPen pen)
        {
            if (PointList.Count > 0)
            {
                if (PointList[0].Selected)
                {
                    dc.Drawing.DrawSelectedPoint(PointList[0], dc.GetPen(DrawTools.PEN_SELECT_POINT));
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

        public override void MoveSelectedPointsFromStored(DrawContext dc, CadVector delta)
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

            return ret;
        }
    }
}