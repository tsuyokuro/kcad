using OpenTK;
using System;

namespace CadDataTypes
{
    public struct CadVertex : IEquatable<CadVertex>
    {
        public static byte INVALID = 0x80;
        public static byte SELECTED = 0x01;
        public static byte HANDLE = 0x02;

        private static byte TYPE_MASK = (byte)(INVALID | HANDLE);

        public byte Flag;

        public ICadVertexAttr Attr;

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

        public static CadVertex Zero = default(CadVertex);

        public static CadVertex UnitX = CadVertex.Create(1, 0, 0);
        public static CadVertex UnitY = CadVertex.Create(0, 1, 0);
        public static CadVertex UnitZ = CadVertex.Create(0, 0, 1);

        public static CadVertex InvalidValue = CadVertex.CreateInvalid();

        public static CadVertex MaxValue = CadVertex.Create(double.MaxValue);
        public static CadVertex MinValue = CadVertex.Create(double.MinValue);

        public CadVertex(double x, double y, double z)
        {
            vector.X = x;
            vector.Y = y;
            vector.Z = z;

            this.Flag = 0;
            Attr = null;
        }

        public static CadVertex Create(double v)
        {
            return Create(v, v, v);
        }

        public static CadVertex Create(double x, double y)
        {
            return Create(x, y, 0);
        }

        public static CadVertex Create(double x, double y, double z)
        {
            CadVertex v = default(CadVertex);
            v.Set(x, y, z);

            v.Flag = 0;

            return v;
        }

        public static CadVertex Create()
        {
            CadVertex v = default(CadVertex);
            v.Set(0, 0, 0);

            v.Flag = 0;

            return v;
        }

        public static CadVertex Create(Vector3d v)
        {
            CadVertex p = default(CadVertex);
            p.Set(v.X, v.Y, v.Z);

            p.Flag = 0;

            return p;
        }

        public static CadVertex Create(Vector4d v)
        {
            CadVertex p = default(CadVertex);
            p.Set(v.X, v.Y, v.Z);

            p.Flag = 0;

            return p;
        }

        public static CadVertex Create(CadVertex v)
        {
            return v;
        }

        public static CadVertex CreateInvalid()
        {
            CadVertex p = default(CadVertex);
            p.Valid = false;
            return p;
        }

        public CadVertex(double x, double y, double z, byte flag, ICadVertexAttr attr)
        {
            vector = new Vector3d(x, y, z);
            Flag = flag;
            Attr = attr;
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

        public CadVertex SetVector(Vector3d v)
        {
            vector = v;
            return this;
        }

        public CadVertex SetVector(CadVertex p)
        {
            vector = p.vector;
            return this;
        }

        public void Set(ref CadVertex p)
        {
            Flag = p.Flag;
            x = p.x;
            y = p.y;
            z = p.z;
        }

        #region 同値判定
        public bool DataEquals(CadVertex p)
        {
            return Equals(p) && ((Flag & TYPE_MASK) == (p.Flag & TYPE_MASK));
        }

        public bool EqualsThreshold(CadVertex p, double m = 0.000001)
        {
            return (
                x > p.x - m && x < p.x + m &&
                y > p.y - m && y < p.y + m &&
                z > p.z - m && z < p.z + m
                );
        }


        public bool Equals(CadVertex v)
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
            CadVertex t = (CadVertex)obj;

            return Equals(t);
        }


        public static bool operator ==(CadVertex p1, CadVertex p2)
        {
            return p1.x == p2.x & p1.y == p2.y & p1.z == p2.z;
        }

        public static bool operator !=(CadVertex p1, CadVertex p2)
        {
            return p1.x != p2.x | p1.y != p2.y | p1.z != p2.z;
        }
        #endregion


        #region 二項演算子
        public static CadVertex operator +(CadVertex p1, CadVertex p2)
        {
            p1.x += p2.x;
            p1.y += p2.y;
            p1.z += p2.z;

            return p1;
        }

        public static CadVertex operator -(CadVertex p1, CadVertex p2)
        {
            p1.x -= p2.x;
            p1.y -= p2.y;
            p1.z -= p2.z;

            return p1;
        }

        public static CadVertex operator *(CadVertex p1, double f)
        {
            p1.x *= f;
            p1.y *= f;
            p1.z *= f;

            return p1;
        }

        public static CadVertex operator *(double f, CadVertex p1)
        {
            p1.x *= f;
            p1.y *= f;
            p1.z *= f;

            return p1;
        }

        public static CadVertex operator *(CadVertex p1, CadVertex p2)
        {
            p1.x *= p2.x;
            p1.y *= p2.y;
            p1.z *= p2.z;

            return p1;
        }

        public static CadVertex operator /(CadVertex p1, double f)
        {
            p1.x /= f;
            p1.y /= f;
            p1.z /= f;

            return p1;
        }

        public static CadVertex operator -(CadVertex p1, double d)
        {
            p1.x -= d;
            p1.y -= d;
            p1.z -= d;

            return p1;
        }
        #endregion

        #region 単項演算子
        public static CadVertex operator -(CadVertex p1)
        {
            p1.x *= -1;
            p1.y *= -1;
            p1.z *= -1;

            return p1;
        }

        public static CadVertex operator +(CadVertex p1, double d)
        {
            p1.x += d;
            p1.y += d;
            p1.z += d;

            return p1;
        }
        #endregion

        #region Cast operator
        public static explicit operator Vector3d(CadVertex p)
        {
            return new Vector3d(p.vector);
        }

        public static explicit operator CadVertex(Vector3d v)
        {
            return Create(
                v.X,
                v.Y,
                v.Z
                );
        }

        public static explicit operator CadVertex(Vector4d v)
        {
            return Create(
                v.X,
                v.Y,
                v.Z
                );
        }

        public static explicit operator Vector4d(CadVertex p)
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
        public static CadVertex Min(CadVertex v1, CadVertex v2)
        {
            CadVertex v = default(CadVertex);

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
        public static CadVertex Max(CadVertex v1, CadVertex v2)
        {
            CadVertex v = default(CadVertex);

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
        public CadVertex UnitVector()
        {
            CadVertex ret = default(CadVertex);

            double norm = this.Norm();

            double f = 1.0 / norm;

            ret.x = x * f;
            ret.y = y * f;
            ret.z = z * f;

            return ret;
        }

        public string CoordString()
        {
            return x.ToString() + ", " + y.ToString() + ", " + z.ToString();
        }

        public override string ToString()
        {
            return CoordString();
        }
    }
}