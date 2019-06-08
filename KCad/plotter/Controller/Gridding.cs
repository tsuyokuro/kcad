using System;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
    public class Gridding
    {
        private Vector3d mGridSize;

        public Vector3d GridSize
        {
            set
            {
                mGridSize = value;
            }

            get
            {
                return mGridSize;
            }
        }

        public double Range = 8;

        public Vector3d XMatchU = default;
        public Vector3d YMatchU = default;

        public Vector3d XMatchW = default;
        public Vector3d YMatchW = default;

        public Gridding()
        {
            GridSize = new Vector3d(10, 10, 10);
        }

        public void Clear()
        {
            XMatchU = VectorUtil.InvalidVector3d;
            YMatchU = VectorUtil.InvalidVector3d;

            XMatchW = VectorUtil.InvalidVector3d;
            YMatchW = VectorUtil.InvalidVector3d;
        }

        public void CopyFrom(Gridding g)
        {
            mGridSize = g.mGridSize;
            Range = g.Range;
        }

        private Vector3d CalcGridSizeU(DrawContext dc, Vector3d gridSizeW)
        {
            Vector3d gridSize = dc.WorldPointToDevPoint(gridSizeW) - dc.WorldPointToDevPoint(Vector3d.Zero);

            gridSize.X = Math.Abs(gridSize.X);
            gridSize.Y = Math.Abs(gridSize.Y);
            gridSize.Z = Math.Abs(gridSize.Z);

            return gridSize;
        }

        public void Check(DrawContext dc, Vector3d up)
        {
            Vector3d scr = CalcGridSizeU(dc, GridSize);

            Vector3d t = default;

            Vector3d sp = up;

            Vector3d u0 = dc.ViewOrg;

            double range;

            up -= u0;

            t.X = (Math.Round(up.X / scr.X)) * scr.X;
            t.Y = (Math.Round(up.Y / scr.Y)) * scr.Y;
            t.Z = 0;

            range = Math.Min(scr.X / 3, Range);

            if (Math.Abs(t.X - up.X) < range)
            {
                XMatchU = t + dc.ViewOrg;
                XMatchU.Y = sp.Y;
                XMatchU.Z = 0;
            }

            range = Math.Min(scr.Y / 3, Range);

            if (Math.Abs(t.Y - up.Y) < range)
            {
                YMatchU = t + dc.ViewOrg;
                YMatchU.X = sp.X;
                YMatchU.Z = 0;
            }

            if (XMatchU.IsValid())
            {
                XMatchW = dc.DevPointToWorldPoint(XMatchU);
            }

            if (YMatchU.IsValid())
            {
                YMatchW = dc.DevPointToWorldPoint(YMatchU);
            }
        }

        /**
         * 画面上での間隔が min より大きくなるように間引く為のサイズの
         * 倍率を求める
         */
        public double Decimate(DrawContext dc, Gridding grid, double min)
        {
            double n = 1;

            double szx = grid.GridSize.X;
            double szy = grid.GridSize.Y;
            double szz = grid.GridSize.Z;

            Vector3d usz;
            double t = 1;
            double d;

            Vector3d uzero = dc.WorldPointToDevPoint(Vector3d.Zero);

            double uszx;
            double uszy;

            usz = dc.WorldPointToDevPoint(new Vector3d(szx, 0, 0)) - uzero;

            uszx = Math.Abs(usz.X);
            uszy = Math.Abs(usz.Y);

            if (uszx != 0 && uszx < min)
            {
                d = Math.Ceiling(min / uszx) * uszx;
                t = d / uszx;
            }

            if (t > n)
            {
                n = t;
            }

            if (uszy != 0 && uszy < min)
            {
                d = Math.Ceiling(min / uszy) * uszy;
                t = d / uszy;
            }

            if (t > n)
            {
                n = t;
            }

            usz = dc.WorldPointToDevPoint(new Vector3d(0, szy, 0)) - uzero;

            uszx = Math.Abs(usz.X);
            uszy = Math.Abs(usz.Y);

            if (uszx != 0 && uszx < min)
            {
                d = Math.Ceiling(min / uszx) * uszx;
                t = d / uszx;
            }

            if (t > n)
            {
                n = t;
            }

            if (uszy != 0 && uszy < min)
            {
                d = Math.Ceiling(min / uszy) * uszy;
                t = d / uszy;
            }

            if (t > n)
            {
                n = t;
            }

            usz = dc.WorldPointToDevPoint(new Vector3d(0, 0, szy)) - uzero;

            uszx = Math.Abs(usz.X);
            uszy = Math.Abs(usz.Y);


            if (uszx != 0 && uszx < min)
            {
                d = Math.Ceiling(min / uszx) * uszx;
                t = d / uszx;
            }

            if (t > n)
            {
                n = t;
            }

            if (uszy != 0 && uszy < min)
            {
                d = Math.Ceiling(min / uszy) * uszy;
                t = d / uszy;
            }

            if (t > n)
            {
                n = t;
            }

            return n;
        }
    }
}
