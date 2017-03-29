using System;


namespace Plotter
{
    class Gridding
    {
        CadPoint GridSizeW;

        double Range = 8;

        public CadPoint XMatchU = default(CadPoint);
        public CadPoint YMatchU = default(CadPoint);

        public CadPoint XMatchW = default(CadPoint);
        public CadPoint YMatchW = default(CadPoint);

        public bool Enable = true;

        public Gridding()
        {
            GridSizeW = CadPoint.Create(10, 10, 10);
        }

        public void Clear()
        {
            XMatchU.Type = CadPoint.Types.INVALID;
            YMatchU.Type = CadPoint.Types.INVALID;

            XMatchW.Type = CadPoint.Types.INVALID;
            YMatchW.Type = CadPoint.Types.INVALID;
        }


        private CadPoint CalcGridSizeU(DrawContext dc)
        {
            CadPoint gridSize = dc.CadPointToUnitPoint(GridSizeW) - dc.CadPointToUnitPoint(CadPoint.Zero);

            //Range = gridSize.x / 4;

            return gridSize;
        } 

        public void Check(DrawContext dc, CadPoint up)
        {
            if (!Enable)
            {
                Clear();
                return;
            }

            CadPoint scr = CalcGridSizeU(dc);

            CadPoint t = default(CadPoint);

            CadPoint sp = up;

            CadPoint u0 = dc.mViewOrg;

            up -= u0;

            t.x = (Math.Round(up.x / scr.x)) * scr.x;
            t.y = (Math.Round(up.y / scr.y)) * scr.y;
            t.z = 0;

            //scr.dump(DebugOut.Std);

            if (Math.Abs(t.x - up.x) < Range)
            {
                XMatchU = t + dc.ViewOrg;
                XMatchU.y = sp.y;
                XMatchU.z = 0;
                XMatchU.Valid = true;
            }

            if (Math.Abs(t.y - up.y) < Range)
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
    }
}
