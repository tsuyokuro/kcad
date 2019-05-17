using CadDataTypes;

namespace Plotter
{
    public struct CadCursor
    {
        public CadVertex Pos;
        public CadVertex DirX;
        public CadVertex DirY;

        public CadVertex StorePos;

        public static CadCursor Create()
        {
            CadCursor cc = default(CadCursor);

            cc.DirX = CadVertex.UnitX;
            cc.DirY = CadVertex.UnitY;

            return cc;
        }

        public static CadCursor Create(CadVertex pixp)
        {
            CadCursor cc = default(CadCursor);

            cc.Pos = pixp;
            cc.DirX = CadVertex.UnitX;
            cc.DirY = CadVertex.UnitY;

            return cc;
        }

        public void Store()
        {
            StorePos = Pos;
        }

        public CadVertex DistanceX(CadVertex pixp)
        {
            CadVertex a1 = Pos;
            CadVertex a2 = Pos + DirY;

            CadVertex b1 = pixp;
            CadVertex b2 = pixp + DirX;

            CadVertex c = CadUtil.CrossLine2D(a1, a2, b1, b2);

            return pixp - c;
        }

        public CadVertex DistanceY(CadVertex pixp)
        {
            CadVertex a1 = Pos;
            CadVertex a2 = Pos + DirX;

            CadVertex b1 = pixp;
            CadVertex b2 = pixp + DirY;

            CadVertex c = CadUtil.CrossLine2D(a1, a2, b1, b2);

            return pixp - c;
        }
    }
}