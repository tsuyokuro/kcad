﻿
using Newtonsoft.Json.Linq;
using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;

namespace Plotter
{
    [Serializable]
    public struct CadPoint
    {
        public enum Types : byte
        {
            INVALID = 0xff,
            STD = 0,
            BREAK = 1,
            HANDLE = 2,
        }

        public const byte IGNORE_X = 1;
        public const byte IGNORE_Y = 2;
        public const byte IGNORE_Z = 4;

        public static class Flags
        {
            public static uint SELECTED = 0x0001;
            public static uint REMOVE_MARK = 0x0002;
        }

        public Types Type;
        public uint Flag;

        public double x
        {
            set
            {
                vector.X = value;
            }

            get
            {
                return vector.X;
            }
        }

        public double y
        {
            set
            {
                vector.Y = value;
            }

            get
            {
                return vector.Y;
            }
        }

        public double z
        {
            set
            {
                vector.Z = value;
            }

            get
            {
                return vector.Z;
            }
        }

        public Vector3d vector;

        public bool Selected
        {
            get
            {
                return (Flag & Flags.SELECTED) != 0;
            }

            set
            {
                Flag = value ? (Flag | Flags.SELECTED) : (Flag & ~Flags.SELECTED);
            }
        }

        public bool RemoveMark
        {
            get
            {
                return (Flag & Flags.REMOVE_MARK) != 0;
            }

            set
            {
                Flag = value ? (Flag | Flags.REMOVE_MARK) : (Flag & ~Flags.REMOVE_MARK);
            }
        }

        public CadPoint(double x, double y, double z, Types type = Types.STD)
        {
            vector.X = x;
            vector.Y = y;
            vector.Z = z;

            this.Flag = 0;
            this.Type = type;
        }

        public static CadPoint Create(double v)
        {
            return Create(v, v, v);
        }

        public static CadPoint Create(double x, double y, double z, Types type = Types.STD)
        {
            CadPoint v = default(CadPoint);
            v.set(x, y, z);

            v.Flag = 0;
            v.Type = type;

            return v;
        }

        public static CadPoint Create()
        {
            CadPoint v = default(CadPoint);
            v.set(0, 0, 0);

            v.Flag = 0;
            v.Type = Types.STD;

            return v;
        }

        public JObject ToJson()
        {
            var jo = new JObject();

            jo.Add("type", (byte)Type);
            jo.Add("flags", Flag);
            jo.Add("x", x);
            jo.Add("y", y);
            jo.Add("z", z);

            return jo;
        }

        public void FromJson(JObject jo)
        {
            Type = (Types)(byte)jo["type"];
            Flag = (uint)jo["flags"];
            x = (double)jo["x"];
            y = (double)jo["y"];
            z = (double)jo["z"];
        }

        public bool IsZero()
        {
            return x == 0 && y == 0 && z == 0;
        }

        public void set(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void set(ref CadPoint p)
        {
            Flag = p.Flag;
            x = p.x;
            y = p.y;
            z = p.z;
        }

        public bool coordEquals(CadPoint p)
        {
            return (x == p.x && y == p.y && z == p.z);
        }

        public bool coordEqualsR(CadPoint p, double m = 0.000001)
        {
            return (
                x > p.x - m && x < p.x + m &&
                y > p.y - m && y < p.y + m &&
                x > p.z - m && x < p.z + m
                );
        }

        public bool dataEquals(CadPoint p)
        {
            return coordEquals(p) && (Type == p.Type);
        }

        public static CadPoint operator +(CadPoint p1, CadPoint p2)
        {
            p1.x += p2.x;
            p1.y += p2.y;
            p1.z += p2.z;

            return p1;
        }

        public static CadPoint operator -(CadPoint p1, CadPoint p2)
        {
            p1.x -= p2.x;
            p1.y -= p2.y;
            p1.z -= p2.z;

            return p1;
        }

        public static CadPoint operator *(CadPoint p1, double f)
        {
            p1.x *= f;
            p1.y *= f;
            p1.z *= f;

            return p1;
        }

        public static CadPoint operator /(CadPoint p1, double f)
        {
            p1.x /= f;
            p1.y /= f;
            p1.z /= f;

            return p1;
        }

        public static CadPoint operator -(CadPoint p1, double d)
        {
            p1.x -= d;
            p1.y -= d;
            p1.z -= d;

            return p1;
        }

        public static CadPoint operator +(CadPoint p1, double d)
        {
            p1.x += d;
            p1.y += d;
            p1.z += d;

            return p1;
        }

        public static explicit operator Vector3d (CadPoint p)
        {
            Vector3d v3 = new Vector3d(p.vector);
            return v3;
        }

        public static explicit operator CadPoint (Vector3d v)
        {
            return Create(
                v.X,
                v.Y,
                v.Z
                );
        }

        public static explicit operator CadPoint(Vector4d v)
        {
            return Create(
                v.X,
                v.Y,
                v.Z
                );
        }

        public static explicit operator Vector4d (CadPoint p)
        {
            return new Vector4d(
                p.vector.X,
                p.vector.Y,
                p.vector.Z,
                1.0f
                );
        }

        // ベクトルのノルム(長さ)を求める
        public double norm()
        {
            return System.Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        // 単位ベクトルを求める
        public CadPoint unitVector()
        {
            CadPoint ret = default(CadPoint);

            double norm = this.norm();

            double f = 1.0 / norm;

            ret.x = x * f;
            ret.y = y * f;
            ret.z = z * f;

            return ret;
        }

        public static CadPoint GetNew(double x, double y, double z)
        {
            CadPoint p = default(CadPoint);
            p.set(x, y, z);
            return p;
        }

        public void dump(DebugOut dout)
        {
            dout.println("CadPoint {");
            dout.Indent++;
            dout.println("Type:" + Type.ToString());
            dout.println("x:" + x.ToString());
            dout.println("y:" + y.ToString());
            dout.println("z:" + z.ToString());
            dout.Indent--;
            dout.println("}");
        }
    }

    public struct CadRect
    {
        public CadPoint p0;
        public CadPoint p1;
    }

    [Serializable]
    public class CadRelativePoint
    {
        public uint ID { get; set; } = 0;

        public enum Types : byte
        {
            NONE = 0,
            CENTER = 1,
        }
        public Types Type { get; set; } = Types.NONE;

        public uint FigureIdA { get; set; } = 0;
        public int IndexA { get; set; } = 0;

        public uint FigureIdB { get; set; } = 0;
        public int IndexB { get; set; } = 0;

        public CadPoint point = default(CadPoint);

        public bool Selected
        {
            get
            {
                return point.Selected;
            }

            set
            {
                point.Selected = value;
            }
        }

        public bool RemoveMark
        {
            get
            {
                return point.RemoveMark;
            }

            set
            {
                point.RemoveMark = value;
            }
        }

        public CadRelativePoint()
        {
        }

        public JObject ToJson()
        {
            JObject jo = new JObject();

            jo.Add("id", ID);
            jo.Add("type", (byte)Type);

            jo.Add("point", point.ToJson());

            jo.Add("fig_id_A", FigureIdA);
            jo.Add("indexA", IndexA);

            jo.Add("fig_id_B", FigureIdB);
            jo.Add("indexB", IndexB);

            return jo;
        }

        public void FromJson(JObject jo)
        {
            ID = (uint)jo["id"];
            Type = (Types)(byte)jo["type"];

            point.FromJson((JObject)jo["point"]);

            FigureIdA = (uint)jo["fig_id_A"];
            IndexA = (int)jo["indexA"];

            FigureIdB = (uint)jo["fig_id_B"];
            IndexB = (int)jo["indexB"];
        }

        public void set(Types type, CadFigure figA, int idxA, CadFigure figB, int idxB)
        {
            point = default(CadPoint);

            Type = type;

            FigureIdA = figA.ID;
            IndexA = idxA;

            FigureIdB = figB.ID;
            IndexB = idxB;

            update(figA, figB);
        }

        public void update(CadFigure figA, CadFigure figB)
        {
            int ia = IndexA;
            if (ia > figA.PointList.Count - 1)
            {
                ia = figA.PointList.Count - 1;
            }

            int ib = IndexB;
            if (ib > figB.PointList.Count - 1)
            {
                ib = figB.PointList.Count - 1;
            }

            CadPoint pA = figA.PointList[ia];
            CadPoint pB = figB.PointList[ib];

            switch (Type)
            {
                case Types.CENTER:
                    setCenter(pA, pB);
                    break;
            }
        }

        private void setCenter(CadPoint a, CadPoint b)
        {
            CadPoint d = b - a;
            d /= 2.0;
            point = a + d;
            point.Flag = 0;
        }

        public void draw(DrawContext dc)
        {
            double d = dc.UnitToMilli(4);

            CadPoint p0 = point;
            CadPoint p1 = point;
            CadPoint p2 = point;
            CadPoint p3 = point;

            p0.x -= d;
            p0.y += d;
            p1.x += d;
            p1.y -= d;

            p2.x -= d;
            p2.y -= d;
            p3.x += d;
            p3.y += d;

            dc.Drawing.DrawLine(DrawTools.PEN_RELATIVE_POINT, p0, p1);
            dc.Drawing.DrawLine(DrawTools.PEN_RELATIVE_POINT, p2, p3);
        }

        public void drawSelected(DrawContext dc)
        {
            if (point.Selected)
            {
                dc.Drawing.DrawSelectedPoint(point);
            }
        }
    }

    /*
    public struct CadPixelPoint
    {
        public double x;
        public double y;
        public double z;

        public CadPixelPoint(double x, double y, double z=0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void set(double x, double y, double z=0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void set(ref CadPixelPoint p)
        {
            x = p.x;
            y = p.y;
            z = p.z;
        }

        public void sub(ref CadPixelPoint p)
        {
            x -= p.x;
            y -= p.y;
            z -= p.z;
        }

        public void add(ref CadPixelPoint p)
        {
            x += p.x;
            y += p.y;
            z += p.z;
        }

        public void add(double dx, double dy, double dz)
        {
            x += dx;
            y += dy;
            z += dz;
        }

        public static CadPixelPoint operator +(CadPixelPoint p1, CadPixelPoint p2)
        {
            p1.x += p2.x;
            p1.y += p2.y;
            p1.z += p2.z;

            return p1;
        }

        public static CadPixelPoint operator -(CadPixelPoint p1, CadPixelPoint p2)
        {
            p1.x -= p2.x;
            p1.y -= p2.y;
            p1.z -= p2.z;

            return p1;
        }

        public static CadPixelPoint operator *(CadPixelPoint p1, double a)
        {
            p1.x = p1.x * a;
            p1.y = p1.y * a;
            p1.z = p1.z * a;

            return p1;
        }

        public static CadPixelPoint operator /(CadPixelPoint p1, double a)
        {
            p1.x = p1.x / a;
            p1.y = p1.y / a;
            p1.z = p1.z / a;

            return p1;
        }

        public void dump(DebugOut dout)
        {
            dout.println("CadPixelPoint {");
            dout.Indent++;
            dout.println("x:" + x.ToString());
            dout.println("y:" + y.ToString());
            dout.println("z:" + z.ToString());
            dout.Indent--;
            dout.println("}");
        }
    }
    */

    class CadPointUtil
    {
        public static int initBezier(CadFigure fig, int idx1, int idx2)
        {
            if (idx1 > idx2)
            {
                int t = idx1;
                idx1 = idx2;
                idx2 = t;
            }

            CadPoint a = fig.getPointAt(idx1);
            CadPoint b = fig.getPointAt(idx2);

            CadPoint hp1 = b - a;
            hp1 = hp1 / 3;
            hp1 = hp1 + a;

            CadPoint hp2 = a - b;
            hp2 = hp2 / 3;
            hp2 = hp2 + b;

            hp1.Type = CadPoint.Types.HANDLE;
            hp2.Type = CadPoint.Types.HANDLE;

            fig.insertPointAt(idx1 + 1, hp1);
            fig.insertPointAt(idx1 + 2, hp2);

            return 2;
        }
    }
}