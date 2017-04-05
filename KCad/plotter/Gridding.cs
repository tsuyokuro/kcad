using System;


namespace Plotter
{
    public class Gridding
    {
        private CadPoint mGridSize;

        public CadPoint GridSize
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

        public CadPoint XMatchU = default(CadPoint);
        public CadPoint YMatchU = default(CadPoint);

        public CadPoint XMatchW = default(CadPoint);
        public CadPoint YMatchW = default(CadPoint);

        public bool Enable = true;

        public Gridding()
        {
            GridSize = CadPoint.Create(10, 10, 10);
        }

        public void Clear()
        {
            XMatchU.Type = CadPoint.Types.INVALID;
            YMatchU.Type = CadPoint.Types.INVALID;

            XMatchW.Type = CadPoint.Types.INVALID;
            YMatchW.Type = CadPoint.Types.INVALID;
        }


        private CadPoint CalcGridSizeU(DrawContext dc, CadPoint gridSizeW)
        {
            CadPoint gridSize = dc.CadPointToUnitPoint(gridSizeW) - dc.CadPointToUnitPoint(CadPoint.Zero);

            gridSize.x = Math.Abs(gridSize.x);
            gridSize.y = Math.Abs(gridSize.y);
            gridSize.z = Math.Abs(gridSize.z);

            return gridSize;
        }

        public void Check(DrawContext dc, CadPoint up)
        {
            if (!Enable)
            {
                Clear();
                return;
            }

            CadPoint scr = CalcGridSizeU(dc, GridSize);

            CadPoint t = default(CadPoint);

            CadPoint sp = up;

            CadPoint u0 = dc.mViewOrg;

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
                XMatchW = dc.UnitPointToCadPoint(XMatchU);
            }

            if (YMatchU.Valid)
            {
                YMatchW = dc.UnitPointToCadPoint(YMatchU);
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

            CadPoint usz;
            double t = 1;
            double d;

            CadPoint uzero = dc.CadPointToUnitPoint(CadPoint.Zero);

            usz = dc.CadPointToUnitPoint(CadPoint.Create(szx, 0, 0)) - uzero;

            if (usz.x != 0 && usz.x < min)
            {
                d = Math.Ceiling(min / usz.x) * usz.x;
                t = d / usz.x;
            }

            if (t > n)
            {
                n = t;
            }

            if (usz.y != 0 && usz.y < min)
            {
                d = Math.Ceiling(min / usz.y) * usz.y;
                t = d / usz.y;
            }

            if (t > n)
            {
                n = t;
            }

            usz = dc.CadPointToUnitPoint(CadPoint.Create(0, szy, 0)) - uzero;

            if (usz.x != 0 && usz.x < min)
            {
                d = Math.Ceiling(min / usz.x) * usz.x;
                t = d / usz.x;
            }

            if (t > n)
            {
                n = t;
            }

            if (usz.y != 0 && usz.y < min)
            {
                d = Math.Ceiling(min / usz.y) * usz.y;
                t = d / usz.y;
            }

            if (t > n)
            {
                n = t;
            }

            usz = dc.CadPointToUnitPoint(CadPoint.Create(0, 0, szy)) - uzero;

            if (usz.x != 0 && usz.x < min)
            {
                d = Math.Ceiling(min / usz.x) * usz.x;
                t = d / usz.x;
            }

            if (t > n)
            {
                n = t;
            }

            if (usz.y != 0 && usz.y < min)
            {
                d = Math.Ceiling(min / usz.y) * usz.y;
                t = d / usz.y;
            }

            if (t > n)
            {
                n = t;
            }

            return n;
        }
    }
}
