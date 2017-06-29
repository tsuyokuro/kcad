using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct CadSegment
    {
        public CadPoint P0;
        public CadPoint P1;

        public CadSegment(CadPoint a, CadPoint b)
        {
            P0 = a;
            P1 = b;
        }
    }

    public struct CadRect
    {
        public CadPoint p0;
        public CadPoint p1;
    }

    public struct Plane
    {   //ax+by+cz+d=0

        public CadPoint Normal; // 法線 a, b, c
        public double d;        // d = a x + b y + c z = 平面の法線ベクトルと平面が通るある点との内積


        public Plane(CadPoint normal,  double d)
        {
            Normal = normal;
            this.d = d;
        }

        // ひとつの頂点と法線ベクトルから平面を作成する
        // normalは単位ベクトルであること
        public static Plane CreateFromPointNormal(CadPoint p, CadPoint normal)
        {
            //pとnormalを内積
            double d = p.x * normal.x + p.y * normal.y + p.z * normal.z;

            return new Plane(normal, d);
        }

        public static Plane CreateFrom3Point(CadPoint a, CadPoint b, CadPoint c)
        {
            CadPoint normal = CadMath.Normal(a, b, c);
            return CreateFromPointNormal(a, normal);
        }
    }
}
