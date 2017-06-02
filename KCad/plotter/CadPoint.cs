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

        public bool Valid
        {
            set
            {
                if (value)
                {
                    Type = Types.STD;
                }
                else
                {
                    Type = Types.INVALID;
                }
            }

            get
            {
                return Type != Types.INVALID;
            }
        }

        public static CadPoint Zero = default(CadPoint);

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

        public static CadPoint Create(Vector3d v)
        {
            CadPoint p = default(CadPoint);
            p.set(v.X, v.Y, v.Z);

            p.Flag = 0;
            p.Type = Types.STD;

            return p;
        }

        public static CadPoint Create(Vector4d v)
        {
            CadPoint p = default(CadPoint);
            p.set(v.X, v.Y, v.Z);

            p.Flag = 0;
            p.Type = Types.STD;

            return p;
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
            if (jo == null)
            {
                this = default(CadPoint);
                return;
            }


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
            return new Vector3d(p.vector);
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
        public double Norm()
        {
            return System.Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        // 単位ベクトルを求める
        public CadPoint UnitVector()
        {
            CadPoint ret = default(CadPoint);

            double norm = this.Norm();

            double f = 1.0 / norm;

            ret.x = x * f;
            ret.y = y * f;
            ret.z = z * f;

            return ret;
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

            CadPoint a = fig.GetPointAt(idx1);
            CadPoint b = fig.GetPointAt(idx2);

            CadPoint hp1 = b - a;
            hp1 = hp1 / 3;
            hp1 = hp1 + a;

            CadPoint hp2 = a - b;
            hp2 = hp2 / 3;
            hp2 = hp2 + b;

            hp1.Type = CadPoint.Types.HANDLE;
            hp2.Type = CadPoint.Types.HANDLE;

            fig.InsertPointAt(idx1 + 1, hp1);
            fig.InsertPointAt(idx1 + 2, hp2);

            return 2;
        }
    }
}