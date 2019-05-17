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

            gridSize.x = Math.Abs(gridSize.x);
            gridSize.y = Math.Abs(gridSize.y);
            gridSize.z = Math.Abs(gridSize.z);

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

            t.x = (Math.Round(up.x / scr.x)) * scr.x;
            t.y = (Math.Round(up.y / scr.y)) * scr.y;
            t.z = 0;

            range = Math.Min(scr.x / 3, Range);

            if (Math.Abs(t.x - up.x) < range)
            {
                XMatchU = t + dc.ViewOrg;
                XMatchU.y = sp.y;
                XMatchU.z = 0;
                XMatchU.Valid = true;
            }

            range = Math.Min(scr.y / 3, Range);

            if (Math.Abs(t.y - up.y) < range)
            {
                YMatchU = t + dc.ViewOrg;
                YMatchU.x = sp.x;
                YMatchU.z = 0;
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

            double szx = grid.GridSize.x;
            double szy = grid.GridSize.y;
            double szz = grid.GridSize.z;

            CadVertex usz;
            double t = 1;
            double d;

            CadVertex uzero = dc.WorldPointToDevPoint(CadVertex.Zero);

            double uszx;
            double uszy;

            usz = dc.WorldPointToDevPoint(CadVertex.Create(szx, 0, 0)) - uzero;

            uszx = Math.Abs(usz.x);
            uszy = Math.Abs(usz.y);

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

            uszx = Math.Abs(usz.x);
            uszy = Math.Abs(usz.y);

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

            uszx = Math.Abs(usz.x);
            uszy = Math.Abs(usz.y);


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
