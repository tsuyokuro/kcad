using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
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
