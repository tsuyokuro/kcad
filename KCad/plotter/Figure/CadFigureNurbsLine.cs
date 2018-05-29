using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Plotter.CadFigure;
using CadDataTypes;
using BSpline;

namespace Plotter
{
    public class CadFigureNurbsLine : CadFigure
    {
        public NURBSCurve Nurbs = new NURBSCurve();

        public CadFigureNurbsLine()
        {
            Type = Types.NURBS_LINE;
        }

        public override void StartCreate(DrawContext dc)
        {
        }

        public override void EndCreate(DrawContext dc)
        {
        }

        public override void DrawTemp(DrawContext dc, CadVector tp, int pen)
        {
        }

        public override void AddPointInCreating(DrawContext dc, CadVector p)
        {
        }


        #region Point Move
        public override void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            base.MoveSelectedPoints(dc, delta);
        }

        public override void MoveAllPoints(DrawContext dc, CadVector delta)
        {
            if (Locked) return;

            Util.MoveAllPoints(this, dc, delta);
        }
        #endregion


        public override int PointCount
        {
            get
            {
                return mPointList.Count;
            }
        }

        public override void RemoveSelected()
        {
            mPointList.RemoveAll(a => a.Selected);

            if (PointCount < 2)
            {
                mPointList.Clear();
            }
        }

        public override void AddPoint(CadVector p)
        {
            mPointList.Add(p);
        }

        public override void Draw(DrawContext dc, int pen)
        {
            if (PointList.Count < 2)
            {
                return;
            }

            CadVector c;
            CadVector n;

            c = PointList[0];

            for (int i=1; i<PointList.Count; i++)
            {
                n = PointList[i];
                dc.Drawing.DrawLine(DrawTools.PEN_NURBS_CTRL_LINE, c, n);

                c = n;
            }

            //Nurbs.Closed = true;
            Nurbs.PassOnEdge = true;
            Nurbs.SetCotrolPoints(mPointList);

            /*
            VectorList vl = Nurbs.Evaluate();

            if (vl==null || vl.Count<2)
            {
                return;
            }

            c = vl[0];

            for (int i=1; i< vl.Count; i++)
            {
                n = vl[i];
                dc.Drawing.DrawLine(pen, c, n);

                c = n;
            }
            */

            int cnt = Nurbs.PointCount;

            c = Nurbs.GetPoint(0);

            for (int i=1;i<cnt; i++)
            {
                n = Nurbs.GetPoint(i);
                dc.Drawing.DrawLine(pen, c, n);
                c = n;
            }
        }

        public override void InvertDir()
        {
            mPointList.Reverse();
            Normal = -Normal;
        }

        public override void SetPointAt(int index, CadVector pt)
        {
            mPointList[index] = pt;
        }

        public override DiffData EndEditWithDiff()
        {
            DiffData diff = base.EndEditWithDiff();
            RecalcNormal();
            return diff;
        }

        public override void EndEdit()
        {
            base.EndEdit();
            RecalcNormal();
        }
    }
}