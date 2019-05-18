using System;
using CadDataTypes;


namespace Plotter
{
    public class Gridding
    {
        private CadVertex mGridSize;

        public CadVertex GridSize
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

        public CadVertex XMatchU = default(CadVertex);
        public CadVertex YMatchU = default(CadVertex);

        public CadVertex XMatchW = default(CadVertex);
        public CadVertex YMatchW = default(CadVertex);

        public Gridding()
        {
            GridSize = CadVertex.Create(10, 10, 10);
        }

        public void Clear()
        {
            XMatchU.Valid = false;
            YMatchU.Valid = false;

            XMatchW.Valid = false;
            YMatchW.Valid = false;
        }

        public void CopyFrom(Gridding g)
        {
            mGridSize = g.mGridSize;
            Range = g.Range;
        }

        private CadVertex CalcGridSizeU(DrawContext dc, CadVertex gridSizeW)
        {
            CadVertex gridSize = dc.WorldPointToDevPoint(gridSizeW) - dc.WorldPointToDevPoint(CadVertex.Zero);

            gridSize.X = Math.Abs(gridSize.X);
            gridSize.Y = Math.Abs(gridSize.Y);
            gridSize.Z = Math.Abs(gridSize.Z);

            return gridSize;
        }

        public void Check(DrawContext dc, CadVertex up)
        {
            CadVertex scr = CalcGridSizeU(dc, GridSize);

            CadVertex t = default(CadVertex);

            CadVertex sp = up;

            CadVertex u0 = dc.ViewOrg;

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
                XMatchU.Valid = true;
            }

            range = Math.Min(scr.Y / 3, Range);

            if (Math.Abs(t.Y - up.Y) < range)
            {
                YMatchU = t + dc.ViewOrg;
                YMatchU.X = sp.X;
                YMatchU.Z = 0;
                YMatchU.Valid = true;
            }

            if (XMatchU.Valid)
            {
                XMatchW = dc.DevPointToWorldPoint(XMatchU);
            }

            if (YMatchU.Valid)
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

            CadVertex usz;
            double t = 1;
            double d;

            CadVertex uzero = dc.WorldPointToDevPoint(CadVertex.Zero);

            double uszx;
            double uszy;

            usz = dc.WorldPointToDevPoint(CadVertex.Create(szx, 0, 0)) - uzero;

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

            usz = dc.WorldPointToDevPoint(CadVertex.Create(0, szy, 0)) - uzero;

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

            usz = dc.WorldPointToDevPoint(CadVertex.Create(0, 0, szy)) - uzero;

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
