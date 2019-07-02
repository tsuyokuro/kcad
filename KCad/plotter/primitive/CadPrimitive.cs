using System;
using System.Collections.Generic;
using CadDataTypes;
using OpenTK;

namespace Plotter
{
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

        public CadVertex P0;
        public CadVertex P1;

        public CadSegment(CadVertex a, CadVertex b)
        {
            P0 = a;
            P1 = b;
        }

        public void dump(string name = "FigureSegment")
        {
            DOut.pl(name + "{");
            DOut.Indent++;
            DOut.pl("Valid:" + Valid.ToString());
            P0.dump("P0");
            P1.dump("P1");
            DOut.Indent--;
            DOut.pl("}");
        }
    }

    //public struct PointPair
    //{
    //    public CadVertex P0;
    //    public CadVertex P1;

    //    public PointPair(CadVertex p0, CadVertex p1)
    //    {
    //        P0 = p0;
    //        P1 = p1;
    //    }
    //}

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

        public CadVertex Point0
        {
            get
            {
                return Figure.GetPointAt(Index0);
            }

        }

        public CadVertex Point1
        {
            get
            {
                return Figure.GetPointAt(Index1);
            }
        }

        public CadVertex StoredPoint0
        {
            get
            {
                return Figure.GetStorePointAt(Index0);
            }

        }

        public CadVertex StoredPoint1
        {
            get
            {
                return Figure.GetStorePointAt(Index1);
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
            DOut.pl(name + "{");
            DOut.Indent++;
            DOut.pl("FigureID:" + Figure.ID.ToString());
            DOut.pl("SegIndex:" + SegIndex.ToString());
            DOut.pl("Index0:" + Index0.ToString());
            DOut.pl("Index1:" + Index1.ToString());
            DOut.Indent--;
            DOut.pl("}");

        }
    }

    public struct IndexPair
    {
        public int Idx0;
        public int Idx1;

        public IndexPair(int i0, int i1)
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

    // 直方体の対角線を保持
    public struct CadRect
    {
        public CadVertex p0;
        public CadVertex p1;

        public void Normalize()
        {
            CadVertex minv = p0;
            CadVertex maxv = p0;

            if (p0.X < p1.X)
            {
                maxv.X = p1.X;
            }
            else
            {
                minv.X = p1.X;
            }

            if (p0.Y < p1.Y)
            {
                maxv.Y = p1.Y;
            }
            else
            {
                minv.Y = p1.Y;
            }

            if (p0.Z < p1.Z)
            {
                maxv.Z = p1.Z;
            }
            else
            {
                minv.Z = p1.Z;
            }

            p0 = minv;
            p1 = maxv;
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
        public Vector2d Min;
        public Vector2d Max;

        public static MinMax2D Create()
        {
            MinMax2D mm = default(MinMax2D);

            mm.Min.X = Double.MaxValue;
            mm.Min.Y = Double.MaxValue;

            mm.Max.X = Double.MinValue;
            mm.Max.Y = Double.MinValue;
            return mm;
        }

        public void CheckMin(Vector3d p)
        {
            Min.X = Math.Min(Min.X, p.X);
            Min.Y = Math.Min(Min.Y, p.Y);
        }

        public void CheckMax(Vector3d p)
        {
            Max.X = Math.Max(Max.X, p.X);
            Max.Y = Math.Max(Max.Y, p.Y);
        }

        public void Check(Vector3d p)
        {
            CheckMin(p);
            CheckMax(p);
        }

        public void CheckMin(MinMax3D mm)
        {
            Min.X = Math.Min(Min.X, mm.Min.X);
            Min.Y = Math.Min(Min.Y, mm.Min.Y);
        }

        public void CheckMax(MinMax3D mm)
        {
            Max.X = Math.Max(Max.X, mm.Max.X);
            Max.Y = Math.Max(Max.Y, mm.Max.Y);
        }

        public void Check(MinMax3D mm)
        {
            CheckMin(mm);
            CheckMax(mm);
        }

        public Vector3d GetMinAsVector()
        {
            return new Vector3d(Min.X, Min.Y, 0);
        }

        public Vector3d GetMaxAsVector()
        {
            return new Vector3d(Max.X, Max.Y, 0);
        }

        public Vector3d Inner(Vector3d v)
        {
            if (v.X > Max.X) v.X = Max.X;
            if (v.Y > Max.Y) v.Y = Max.Y;
            if (v.X < Min.X) v.X = Min.X;
            if (v.Y < Min.Y) v.Y = Min.Y;

            return v;
        }
    }

    public struct MinMax3D
    {
        public Vector3d Min;
        public Vector3d Max;

        public static MinMax3D Create(
            )
        {
            MinMax3D mm = default;

            mm.Min.X = Double.MaxValue;
            mm.Min.Y = Double.MaxValue;
            mm.Min.Z = Double.MaxValue;

            mm.Max.X = Double.MinValue;
            mm.Max.Y = Double.MinValue;
            mm.Max.Z = Double.MinValue;

            return mm;
        }

        public void CheckMin(Vector3d p)
        {
            Min.X = Math.Min(Min.X, p.X);
            Min.Y = Math.Min(Min.Y, p.Y);
            Min.Z = Math.Min(Min.Z, p.Z);
        }

        public void CheckMax(Vector3d p)
        {
            Max.X = Math.Max(Max.X, p.X);
            Max.Y = Math.Max(Max.Y, p.Y);
            Max.Z = Math.Max(Max.Z, p.Z);
        }

        public void Check(Vector3d p)
        {
            CheckMin(p);
            CheckMax(p);
        }

        public void CheckMin(MinMax3D mm)
        {
            Min.X = Math.Min(Min.X, mm.Min.X);
            Min.Y = Math.Min(Min.Y, mm.Min.Y);
            Min.Z = Math.Min(Min.Z, mm.Min.Z);
        }

        public void CheckMax(MinMax3D mm)
        {
            Max.X = Math.Max(Max.X, mm.Max.X);
            Max.Y = Math.Max(Max.Y, mm.Max.Y);
            Max.Z = Math.Max(Max.Z, mm.Max.Z);
        }

        public void Check(MinMax3D mm)
        {
            CheckMin(mm);
            CheckMax(mm);
        }

        public Vector3d GetMinAsVector()
        {
            return Min;
        }

        public Vector3d GetMaxAsVector()
        {
            return Max;
        }
    }
}