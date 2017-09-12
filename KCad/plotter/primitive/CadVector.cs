
using Newtonsoft.Json.Linq;
using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;

namespace Plotter
{
    [Serializable]
    public struct CadVector : IEquatable<CadVector>
    {
        public enum Types : byte
        {
            STD = 0,
            BREAK = 1,
            HANDLE = 2,
        }

        private static class Flags
        {
            public static byte INVALID = 0x80;
            public static byte SELECTED = 0x01;
        }

        public Types Type { get; set; }
        private byte Flag;

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
                Flag = value ? (byte)(Flag | Flags.SELECTED) : (byte)(Flag & ~Flags.SELECTED);
            }
        }

        public bool Valid
        {
            set
            {
                if (value)
                {
                    Flag = (byte)(Flag & ~Flags.INVALID);
                }
                else
                {
                    Flag = (byte)(Flag | Flags.INVALID);
                }
            }

            get
            {
                return (Flag & Flags.INVALID) == 0;
            }
        }

        public static CadVector Zero = default(CadVector);

        public static CadVector UnitX = CadVector.Create(1, 0, 0);
        public static CadVector UnitY = CadVector.Create(0, 1, 0);
        public static CadVector UnitZ = CadVector.Create(0, 0, 1);

        public static CadVector Invalid = CadVector.CreateInvalid();

        public CadVector(double x, double y, double z, Types type = Types.STD)
        {
            vector.X = x;
            vector.Y = y;
            vector.Z = z;

            this.Flag = 0;
            this.Type = type;
        }

        public static CadVector Create(double v)
        {
            return Create(v, v, v);
        }

        public static CadVector Create(double x, double y, double z, Types type = Types.STD)
        {
            CadVector v = default(CadVector);
            v.Set(x, y, z);

            v.Flag = 0;
            v.Type = type;

            return v;
        }

        public static CadVector Create()
        {
            CadVector v = default(CadVector);
            v.Set(0, 0, 0);

            v.Flag = 0;
            v.Type = Types.STD;

            return v;
        }

        public static CadVector Create(Vector3d v)
        {
            CadVector p = default(CadVector);
            p.Set(v.X, v.Y, v.Z);

            p.Flag = 0;
            p.Type = Types.STD;

            return p;
        }

        public static CadVector Create(Vector4d v)
        {
            CadVector p = default(CadVector);
            p.Set(v.X, v.Y, v.Z);

            p.Flag = 0;
            p.Type = Types.STD;

            return p;
        }

        public static CadVector CreateInvalid()
        {
            CadVector p = default(CadVector);
            p.Valid = false;
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
                this = default(CadVector);
                return;
            }


            Type = (Types)(byte)jo["type"];
            Flag = (byte)jo["flags"];
            x = (double)jo["x"];
            y = (double)jo["y"];
            z = (double)jo["z"];
        }

        public bool IsZero()
        {
            return x == 0 && y == 0 && z == 0;
        }

        public void Set(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public CadVector SetVector(Vector3d v)
        {
            vector = v;
            return this;
        }

        public CadVector SetVector(CadVector p)
        {
            vector = p.vector;
            return this;
        }

        public void Set(ref CadVector p)
        {
            Flag = p.Flag;
            x = p.x;
            y = p.y;
            z = p.z;
        }

        public bool VectorEquals(CadVector p)
        {
            return (x == p.x && y == p.y && z == p.z);
        }

        public bool CoordEqualsThreshold(CadVector p, double m = 0.000001)
        {
            return (
                x > p.x - m && x < p.x + m &&
                y > p.y - m && y < p.y + m &&
                z > p.z - m && z < p.z + m
                );
        }

        public bool DataEquals(CadVector p)
        {
            return VectorEquals(p) && (Type == p.Type);
        }

        public static CadVector operator +(CadVector p1, CadVector p2)
        {
            p1.x += p2.x;
            p1.y += p2.y;
            p1.z += p2.z;

            return p1;
        }

        public static CadVector operator -(CadVector p1, CadVector p2)
        {
            p1.x -= p2.x;
            p1.y -= p2.y;
            p1.z -= p2.z;

            return p1;
        }

        public static CadVector operator *(CadVector p1, double f)
        {
            p1.x *= f;
            p1.y *= f;
            p1.z *= f;

            return p1;
        }

        public static CadVector operator /(CadVector p1, double f)
        {
            p1.x /= f;
            p1.y /= f;
            p1.z /= f;

            return p1;
        }

        public static CadVector operator -(CadVector p1, double d)
        {
            p1.x -= d;
            p1.y -= d;
            p1.z -= d;

            return p1;
        }

        public static CadVector operator +(CadVector p1, double d)
        {
            p1.x += d;
            p1.y += d;
            p1.z += d;

            return p1;
        }

        public static explicit operator Vector3d (CadVector p)
        {
            return new Vector3d(p.vector);
        }

        public static explicit operator CadVector (Vector3d v)
        {
            return Create(
                v.X,
                v.Y,
                v.Z
                );
        }

        public static explicit operator CadVector(Vector4d v)
        {
            return Create(
                v.X,
                v.Y,
                v.Z
                );
        }

        public static explicit operator Vector4d (CadVector p)
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
        public CadVector UnitVector()
        {
            CadVector ret = default(CadVector);

            double norm = this.Norm();

            double f = 1.0 / norm;

            ret.x = x * f;
            ret.y = y * f;
            ret.z = z * f;

            return ret;
        }

        public void dump(DebugOut dout, string prefix = nameof(CadVector))
        {
            dout.println(prefix + "{");
            dout.Indent++;
            dout.println("Type:" + Type.ToString());
            dout.println("x:" + x.ToString());
            dout.println("y:" + y.ToString());
            dout.println("z:" + z.ToString());
            dout.Indent--;
            dout.println("}");
        }

        public bool Equals(CadVector v)
        {
            return x == v.x & y == v.y & z == v.z & Type == v.Type & Flag == v.Flag;
        }

        public override int GetHashCode()
        {
            return (int)Type ^ (int)x ^ (int)y ^ (int)z ^ (int)Flag;
        }
    }
}