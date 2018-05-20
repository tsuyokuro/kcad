using OpenTK;
using System;

namespace CadDataTypes
{
    public struct CadVector : IEquatable<CadVector>
    {
        public static byte INVALID = 0x80;
        public static byte SELECTED = 0x01;
        public static byte HANDLE = 0x02;

        private static byte TYPE_MASK = (byte)(INVALID | HANDLE);

        public byte Flag;

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
                return (Flag & SELECTED) != 0;
            }

            set
            {
                Flag = value ? (byte)(Flag | SELECTED) : (byte)(Flag & ~SELECTED);
            }
        }

        public bool IsHandle
        {
            get
            {
                return (Flag & HANDLE) != 0;
            }

            set
            {
                Flag = value ? (byte)(Flag | HANDLE) : (byte)(Flag & ~HANDLE);
            }
        }


        public bool Valid
        {
            set
            {
                Invalid = !value;
            }

            get
            {
                return !Invalid;
            }
        }

        public bool Invalid
        {
            set
            {
                Flag = value ? (byte)(Flag | INVALID) : (byte)(Flag & ~INVALID);
            }

            get
            {
                return (Flag & INVALID) != 0;
            }
        }

        public static CadVector Zero = default(CadVector);

        public static CadVector UnitX = CadVector.Create(1, 0, 0);
        public static CadVector UnitY = CadVector.Create(0, 1, 0);
        public static CadVector UnitZ = CadVector.Create(0, 0, 1);

        public static CadVector InvalidValue = CadVector.CreateInvalid();

        public static CadVector MaxValue = CadVector.Create(double.MaxValue);
        public static CadVector MinValue = CadVector.Create(double.MinValue);

        public CadVector(double x, double y, double z)
        {
            vector.X = x;
            vector.Y = y;
            vector.Z = z;

            this.Flag = 0;
        }

        public static CadVector Create(double v)
        {
            return Create(v, v, v);
        }

        public static CadVector Create(double x, double y)
        {
            return Create(x, y, 0);
        }

        public static CadVector Create(double x, double y, double z)
        {
            CadVector v = default(CadVector);
            v.Set(x, y, z);

            v.Flag = 0;

            return v;
        }

        public static CadVector Create()
        {
            CadVector v = default(CadVector);
            v.Set(0, 0, 0);

            v.Flag = 0;

            return v;
        }

        public static CadVector Create(Vector3d v)
        {
            CadVector p = default(CadVector);
            p.Set(v.X, v.Y, v.Z);

            p.Flag = 0;

            return p;
        }

        public static CadVector Create(Vector4d v)
        {
            CadVector p = default(CadVector);
            p.Set(v.X, v.Y, v.Z);

            p.Flag = 0;

            return p;
        }

        public static CadVector Create(CadVector v)
        {
            return v;
        }

        public static CadVector CreateInvalid()
        {
            CadVector p = default(CadVector);
            p.Valid = false;
            return p;
        }

        public CadVector(double x, double y, double z, byte flag)
        {
            vector = new Vector3d(x, y, z);
            Flag = flag;
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

        #region 同値判定
        public bool DataEquals(CadVector p)
        {
            return Equals(p) && ((Flag & TYPE_MASK) == (p.Flag & TYPE_MASK));
        }

        public bool EqualsThreshold(CadVector p, double m = 0.000001)
        {
            return (
                x > p.x - m && x < p.x + m &&
                y > p.y - m && y < p.y + m &&
                z > p.z - m && z < p.z + m
                );
        }


        public bool Equals(CadVector v)
        {
            return x == v.x & y == v.y & z == v.z;
        }

        private const double HASH_COEFFICIENT = 10000.0;

        public override int GetHashCode()
        {
            return
                ((int)(x * HASH_COEFFICIENT)) ^
                ((int)(y * HASH_COEFFICIENT) << 2) ^
                ((int)(z * HASH_COEFFICIENT) >> 2);
        }

        public override bool Equals(object obj)
        {
            CadVector t = (CadVector)obj;

            return Equals(t);
        }


        public static bool operator ==(CadVector p1, CadVector p2)
        {
            return p1.x == p2.x & p1.y == p2.y & p1.z == p2.z;
        }

        public static bool operator !=(CadVector p1, CadVector p2)
        {
            return p1.x != p2.x | p1.y != p2.y | p1.z != p2.z;
        }
        #endregion


        #region 二項演算子
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

        public static CadVector operator *(double f, CadVector p1)
        {
            p1.x *= f;
            p1.y *= f;
            p1.z *= f;

            return p1;
        }

        public static CadVector operator *(CadVector p1, CadVector p2)
        {
            p1.x *= p2.x;
            p1.y *= p2.y;
            p1.z *= p2.z;

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
        #endregion

        #region 単項演算子
        public static CadVector operator -(CadVector p1)
        {
            p1.x *= -1;
            p1.y *= -1;
            p1.z *= -1;

            return p1;
        }

        public static CadVector operator +(CadVector p1, double d)
        {
            p1.x += d;
            p1.y += d;
            p1.z += d;

            return p1;
        }
        #endregion

        #region Cast operator
        public static explicit operator Vector3d(CadVector p)
        {
            return new Vector3d(p.vector);
        }

        public static explicit operator CadVector(Vector3d v)
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

        public static explicit operator Vector4d(CadVector p)
        {
            return new Vector4d(
                p.vector.X,
                p.vector.Y,
                p.vector.Z,
                1.0f
                );
        }
        #endregion

        /// <summary>
        /// 二点の成分から最小の成分でVectorを作成
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static CadVector Min(CadVector v1, CadVector v2)
        {
            CadVector v = default(CadVector);

            v.x = Math.Min(v1.x, v2.x);
            v.y = Math.Min(v1.y, v2.y);
            v.z = Math.Min(v1.z, v2.z);

            return v;
        }

        /// <summary>
        /// 二点の成分から最大の成分でVectorを作成
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static CadVector Max(CadVector v1, CadVector v2)
        {
            CadVector v = default(CadVector);

            v.x = Math.Max(v1.x, v2.x);
            v.y = Math.Max(v1.y, v2.y);
            v.z = Math.Max(v1.z, v2.z);

            return v;
        }



        // ベクトルのノルム(長さ)を求める
        public double Norm()
        {
            return Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        public double Norm2D()
        {
            return Math.Sqrt((x * x) + (y * y));
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

        public string SimpleString()
        {
            return x.ToString() + "," + y.ToString() + "," + z.ToString();
        }
    }
}