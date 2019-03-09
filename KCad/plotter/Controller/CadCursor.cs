using CadDataTypes;

namespace Plotter
{
    public struct CadCursor
    {
        public CadVector Pos;
        public CadVector DirX;
        public CadVector DirY;

        public CadVector StorePos;

        public static CadCursor Create()
        {
            CadCursor cc = default(CadCursor);

            cc.DirX = CadVector.UnitX;
            cc.DirY = CadVector.UnitY;

            return cc;
        }

        public static CadCursor Create(CadVector pixp)
        {
            CadCursor cc = default(CadCursor);

            cc.Pos = pixp;
            cc.DirX = CadVector.UnitX;
            cc.DirY = CadVector.UnitY;

            return cc;
        }

        public void Store()
        {
            StorePos = Pos;
        }

        public CadVector DistanceX(CadVector pixp)
        {
            CadVector a1 = Pos;
            CadVector a2 = Pos + DirY;

            CadVector b1 = pixp;
            CadVector b2 = pixp + DirX;

            CadVector c = CadUtil.CrossLine2D(a1, a2, b1, b2);

            return pixp - c;
        }

        public CadVector DistanceY(CadVector pixp)
        {
            CadVector a1 = Pos;
            CadVector a2 = Pos + DirX;

            CadVector b1 = pixp;
            CadVector b2 = pixp + DirY;

            CadVector c = CadUtil.CrossLine2D(a1, a2, b1, b2);

            return pixp - c;
        }
    }
}