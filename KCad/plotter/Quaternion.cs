using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public struct Quaternion
    {
        public double t;
        public double x;
        public double y;
        public double z;

        public Quaternion(double t, double x, double y, double z)
        {
            this.t = t;
            this.x = x;
            this.y = y;
            this.z = z;
        }


        //-----------------------------------------------------------------------------------------
        // ノルム(長さ)
        //
        public double norm()
        {
            return Math.Sqrt((t * t) + (x * x) + (y * y) + (z * z));
        }

        //-----------------------------------------------------------------------------------------
        // 共役四元数を返す
        //
        public Quaternion Conjugate()
        {
            Quaternion q = this;

            q.t = t;
            q.x = -x;
            q.y = -y;
            q.z = -z;

            return q;
        }

        //-----------------------------------------------------------------------------------------
        // 掛け算
        //
        public static Quaternion operator *(Quaternion q, Quaternion r)
        {
            return Product(q, r);
        }

        //-----------------------------------------------------------------------------------------
        // 和を求める
        //
        public static Quaternion operator +(Quaternion q, Quaternion r)
        {
            Quaternion res;

            res.t = q.t + r.t;
            res.x = q.x + r.x;
            res.y = q.y + r.y;
            res.z = q.z + r.z;

            return res;
        }

        //-----------------------------------------------------------------------------------------
        // 四元数の積を求める
        // q * r
        //
        public static Quaternion Product(Quaternion q, Quaternion r)
        {
            // A = (a; U)
            // B = (b; V)
            // AB = (ab - U・V; aV + bU + U×V)
            Quaternion ans;
            double d1, d2, d3, d4;

            d1 = q.t * r.t;
            d2 = q.x * r.x;
            d3 = q.y * r.y;
            d4 = q.z * r.z;
            ans.t = d1 - d2 - d3 - d4;

            d1 = q.t * r.x;
            d2 = r.t * q.x;
            d3 = q.y * r.z;
            d4 = -q.z * r.y;
            ans.x = d1 + d2 + d3 + d4;

            d1 = q.t * r.y;
            d2 = r.t * q.y;
            d3 = q.z * r.x;
            d4 = -q.x * r.z;
            ans.y = d1 + d2 + d3 + d4;

            d1 = q.t * r.z;
            d2 = r.t * q.z;
            d3 = q.x * r.y;
            d4 = -q.y * r.x;
            ans.z = d1 + d2 + d3 + d4;

            return ans;
        }

        //-----------------------------------------------------------------------------------------
        // 単位元を作成
        //
        public static Quaternion Unit()
        {
            Quaternion res;

            res.t = 1.0;
            res.x = 0;
            res.y = 0;
            res.z = 0;

            return res;
        }

        //-----------------------------------------------------------------------------------------
        // Vector (vx, vy, vz)を回転軸としてradianだけ回転する四元数を作成
        //
        public static Quaternion RotateQuaternion(double radian, double vx, double vy, double vz)
        {
            Quaternion ans = default(Quaternion);
            double norm;
            double c, s;

            norm = vx * vx + vy * vy + vz * vz;
            if (norm <= 0.0) return ans;

            norm = 1.0 / Math.Sqrt(norm);
            vx *= norm;
            vy *= norm;
            vz *= norm;

            c = Math.Cos(0.5 * radian);
            s = Math.Sin(0.5 * radian);

            ans.t = c;
            ans.x = s * vx;
            ans.y = s * vy;
            ans.z = s * vz;

            return ans;
        }

        //-----------------------------------------------------------------------------------------
        // Vector (v.x, v.y, v.z)を回転軸としてradianだけ回転する四元数を作成
        //
        public static Quaternion RotateQuaternion(double radian, CadPoint v)
        {
            v = v.unitVector();

            Quaternion ans = default(Quaternion);
            double c, s;

            c = Math.Cos(0.5 * radian);
            s = Math.Sin(0.5 * radian);

            ans.t = c;
            ans.x = s * v.x;
            ans.y = s * v.y;
            ans.z = s * v.z;

            return ans;
        }

        //-----------------------------------------------------------------------------------------
        // CadPointから四元数を作成
        //
        public static Quaternion FromPoint(CadPoint point)
        {
            Quaternion q;
            q.t = 0.0;
            q.x = point.x;
            q.y = point.y;
            q.z = point.z;

            return q;
        }
    }
}
