using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct CadCursor
    {
        public CadVector Pos;
        public CadVector DirX;
        public CadVector DirY;

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

    public struct CadSegment
    {
        public CadVector P0;
        public CadVector P1;

        public CadSegment(CadVector a, CadVector b)
        {
            P0 = a;
            P1 = b;
        }
    }

    public struct FigureSegment
    {
        CadFigure Fig;
        public int SegIndex;
        public int Index0;
        public int Index1;

        public static FigureSegment InvalidValue = new FigureSegment(null, -1, -1, -1);

        public FigureSegment(CadFigure fig, int segIndex, int a, int b)
        {
            Fig = fig;
            SegIndex = segIndex;
            Index0 = a;
            Index1 = b;
        }
    }

    public struct CadRect
    {
        public CadVector p0;
        public CadVector p1;
    }

    public struct Plane
    {   //ax+by+cz+d=0

        public CadVector Normal; // 法線 a, b, c
        public double d;        // d = a x + b y + c z = 平面の法線ベクトルと平面が通るある点との内積


        public Plane(CadVector normal,  double d)
        {
            Normal = normal;
            this.d = d;
        }

        // ひとつの頂点と法線ベクトルから平面を作成する
        // normalは単位ベクトルであること
        public static Plane CreateFromPointNormal(CadVector p, CadVector normal)
        {
            //pとnormalを内積
            double d = p.x * normal.x + p.y * normal.y + p.z * normal.z;

            return new Plane(normal, d);
        }

        public static Plane CreateFrom3Point(CadVector a, CadVector b, CadVector c)
        {
            CadVector normal = CadMath.Normal(a, b, c);
            return CreateFromPointNormal(a, normal);
        }
    }
}
