using System;


namespace Plotter
{
    public class Gridding
    {
        private CadVector mGridSize;

        public CadVector GridSize
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

        public CadVector XMatchU = default(CadVector);
        public CadVector YMatchU = default(CadVector);

        public CadVector XMatchW = default(CadVector);
        public CadVector YMatchW = default(CadVector);

        public bool Enable = false;

        public Gridding()
        {
            GridSize = CadVector.Create(10, 10, 10);
        }

        public void Clear()
        {
            XMatchU.Type = CadVector.Types.INVALID;
            YMatchU.Type = CadVector.Types.INVALID;

            XMatchW.Type = CadVector.Types.INVALID;
            YMatchW.Type = CadVector.Types.INVALID;
        }


        private CadVector CalcGridSizeU(DrawContext dc, CadVector gridSizeW)
        {
            CadVector gridSize = dc.CadPointToUnitPoint(gridSizeW) - dc.CadPointToUnitPoint(CadVector.Zero);

            gridSize.x = Math.Abs(gridSize.x);
            gridSize.y = Math.Abs(gridSize.y);
            gridSize.z = Math.Abs(gridSize.z);

            return gridSize;
        }

        public void Check(DrawContext dc, CadVector up)
        {
            if (!Enable)
            {
                Clear();
                return;
            }

            CadVector scr = CalcGridSizeU(dc, GridSize);

            CadVector t = default(CadVector);

            CadVector sp = up;

            CadVector u0 = dc.mViewOrg;

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

            CadVector usz;
            double t = 1;
            double d;

            CadVector uzero = dc.CadPointToUnitPoint(CadVector.Zero);

            usz = dc.CadPointToUnitPoint(CadVector.Create(szx, 0, 0)) - uzero;

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

            usz = dc.CadPointToUnitPoint(CadVector.Create(0, szy, 0)) - uzero;

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

            usz = dc.CadPointToUnitPoint(CadVector.Create(0, 0, szy)) - uzero;

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
