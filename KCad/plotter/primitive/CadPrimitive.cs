﻿using System;
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

    public struct PointPair
    {
        public CadVertex P0;
        public CadVertex P1;

        public PointPair(CadVertex p0, CadVertex p1)
        {
            P0 = p0;
            P1 = p1;
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

        public void CheckMin(Vector3d p)
        {
            MinX = Math.Min(MinX, p.X);
            MinY = Math.Min(MinY, p.Y);
        }

        public void CheckMax(Vector3d p)
        {
            MaxX = Math.Max(MaxX, p.X);
            MaxY = Math.Max(MaxY, p.Y);
        }

        public void Check(Vector3d p)
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

        public Vector3d GetMinAsVector()
        {
            return new Vector3d(MinX, MinY, 0);
        }

        public Vector3d GetMaxAsVector()
        {
            return new Vector3d(MaxX, MaxY, 0);
        }

        public Vector3d Inner(Vector3d v)
        {
            if (v.X > MaxX) v.X = MaxX;
            if (v.Y > MaxY) v.Y = MaxY;
            if (v.X < MinX) v.X = MinX;
            if (v.Y < MinY) v.Y = MinY;

            return v;
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

        public void CheckMin(Vector3d p)
        {
            MinX = Math.Min(MinX, p.X);
            MinY = Math.Min(MinY, p.Y);
            MinZ = Math.Min(MinZ, p.Z);
        }

        public void CheckMax(Vector3d p)
        {
            MaxX = Math.Max(MaxX, p.X);
            MaxY = Math.Max(MaxY, p.Y);
            MaxZ = Math.Max(MaxZ, p.Z);
        }

        public void Check(Vector3d p)
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

        public Vector3d GetMinAsVector()
        {
            return new Vector3d(MinX, MinY, MinZ);
        }

        public Vector3d GetMaxAsVector()
        {
            return new Vector3d(MaxX, MaxY, MaxZ);
        }
    }
}