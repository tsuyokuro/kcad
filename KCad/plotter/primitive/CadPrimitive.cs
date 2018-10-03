using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

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
        public bool Valid
        {
            set
            {
                P0.Valid = value;
            }

            get
            {
                return P0.Valid;
            }
        }

        public CadVector P0;
        public CadVector P1;

        public CadSegment(CadVector a, CadVector b)
        {
            P0 = a;
            P1 = b;
        }

        public void dump(string name = "FigureSegment")
        {
            DbgOut.pln(name + "{");
            DbgOut.Indent++;
            DbgOut.pln("Valid:" + Valid.ToString());
            P0.dump("P0");
            P1.dump("P1");
            DbgOut.Indent--;
            DbgOut.pln("}");
        }
    }

    public struct FigureSegment
    {
        public CadFigure Figure;
        public int SegIndex;
        public int Index0;
        public int Index1;

        public static FigureSegment InvalidValue = new FigureSegment(null, -1, -1, -1);

        public uint FigureID
        {
            get
            {
                if (Figure == null)
                {
                    return 0;
                }

                return Figure.ID;
            }
        }

        public CadVector Point0
        {
            get
            {
                return Figure.GetPointAt(Index0);
            }

        }

        public CadVector Point1
        {
            get
            {
                return Figure.GetPointAt(Index1);
            }
        }

        public CadSegment Segment
        {
            get
            {
                return Figure.GetSegmentAt(SegIndex);
            }

        }

        public FigureSegment(CadFigure fig, int segIndex, int a, int b)
        {
            Figure = fig;
            SegIndex = segIndex;
            Index0 = a;
            Index1 = b;
        }

        public void dump(string name = "FigureSegment")
        {
            DbgOut.pln(name + "{");
            DbgOut.Indent++;
            DbgOut.pln("FigureID:" + Figure.ID.ToString());
            DbgOut.pln("SegIndex:" + SegIndex.ToString());
            DbgOut.pln("Index0:" + Index0.ToString());
            DbgOut.pln("Index1:" + Index1.ToString());
            DbgOut.Indent--;
            DbgOut.pln("}");

        }
    }

    public struct Index2
    {
        public int Idx0;
        public int Idx1;

        public Index2(int i0, int i1)
        {
            Idx0 = i0;
            Idx1 = i1;
        }
    }

    public struct CadSize2D
    {
        public double Width;
        public double Height;

        public CadSize2D(double w, double h)
        {
            Width = w;
            Height = h;
        }
    }

    public struct CadRect
    {
        public CadVector p0;
        public CadVector p1;

        public void Normalize()
        {
            CadVector minv = p0;
            CadVector maxv = p0;

            if (p0.x < p1.x)
            {
                maxv.x = p1.x;
            }
            else
            {
                minv.x = p1.x;
            }

            if (p0.y < p1.y)
            {
                maxv.y = p1.y;
            }
            else
            {
                minv.y = p1.y;
            }

            if (p0.z < p1.z)
            {
                maxv.z = p1.z;
            }
            else
            {
                minv.z = p1.z;
            }

            p0 = minv;
            p1 = maxv;
        }
    }

    public struct Plane
    {   //ax+by+cz+d=0

        public CadVector Normal; // 法線 a, b, c
        public double d;        // d = a x + b y + c z = 平面の法線ベクトルと平面が通るある点との内積


        public Plane(CadVector normal, double d)
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

    public class CadRegion2D
    {
        public double X;
        public double Y;
        public List<List<double>> Data = new List<List<double>>();
    }

    public struct MinMax2D
    {
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;

        public static MinMax2D Create(
            double minx = Double.MaxValue,
            double miny = Double.MaxValue,
            double maxx = Double.MinValue,
            double maxy = Double.MinValue
            )
        {
            MinMax2D mm = default(MinMax2D);

            mm.MinX = minx;
            mm.MinY = miny;
            mm.MaxX = maxx;
            mm.MaxY = maxy;

            return mm;
        }

        public void CheckMin(CadVector p)
        {
            MinX = Math.Min(MinX, p.x);
            MinY = Math.Min(MinY, p.y);
        }

        public void CheckMax(CadVector p)
        {
            MaxX = Math.Max(MaxX, p.x);
            MaxY = Math.Max(MaxY, p.y);
        }

        public void Check(CadVector p)
        {
            CheckMin(p);
            CheckMax(p);
        }

        public void CheckMin(MinMax3D mm)
        {
            MinX = Math.Min(MinX, mm.MinX);
            MinY = Math.Min(MinY, mm.MinY);
        }

        public void CheckMax(MinMax3D mm)
        {
            MaxX = Math.Max(MaxX, mm.MaxX);
            MaxY = Math.Max(MaxY, mm.MaxY);
        }

        public void Check(MinMax3D mm)
        {
            CheckMin(mm);
            CheckMax(mm);
        }

        public CadVector GetMinAsVector()
        {
            return CadVector.Create(MinX, MinY, 0);
        }

        public CadVector GetMaxAsVector()
        {
            return CadVector.Create(MaxX, MaxY, 0);
        }
    }

    public struct MinMax3D
    {
        public double MinX;
        public double MinY;
        public double MinZ;
        public double MaxX;
        public double MaxY;
        public double MaxZ;

        public static MinMax3D Create(
            double minx = Double.MaxValue,
            double miny = Double.MaxValue,
            double minz = Double.MaxValue,
            double maxx = Double.MinValue,
            double maxy = Double.MinValue,
            double maxz = Double.MinValue
            )
        {
            MinMax3D mm = default(MinMax3D);

            mm.MinX = minx;
            mm.MinY = miny;
            mm.MinZ = minz;
            mm.MaxX = maxx;
            mm.MaxY = maxy;
            mm.MaxZ = maxz;

            return mm;
        }

        public void CheckMin(CadVector p)
        {
            MinX = Math.Min(MinX, p.x);
            MinY = Math.Min(MinY, p.y);
            MinZ = Math.Min(MinZ, p.z);
        }

        public void CheckMax(CadVector p)
        {
            MaxX = Math.Max(MaxX, p.x);
            MaxY = Math.Max(MaxY, p.y);
            MaxZ = Math.Max(MaxZ, p.z);
        }

        public void Check(CadVector p)
        {
            CheckMin(p);
            CheckMax(p);
        }

        public void CheckMin(MinMax3D mm)
        {
            MinX = Math.Min(MinX, mm.MinX);
            MinY = Math.Min(MinY, mm.MinY);
            MinZ = Math.Min(MinZ, mm.MinZ);
        }

        public void CheckMax(MinMax3D mm)
        {
            MaxX = Math.Max(MaxX, mm.MaxX);
            MaxY = Math.Max(MaxY, mm.MaxY);
            MaxZ = Math.Max(MaxZ, mm.MaxZ);
        }

        public void Check(MinMax3D mm)
        {
            CheckMin(mm);
            CheckMax(mm);
        }

        public CadVector GetMinAsVector()
        {
            return CadVector.Create(MinX, MinY, MinZ);
        }

        public CadVector GetMaxAsVector()
        {
            return CadVector.Create(MaxX, MaxY, MaxZ);
        }
    }
}