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

        public static Quaternion operator *(Quaternion right, Quaternion left)
        {
            return Product(right, left);
        }

        public static Quaternion Product(Quaternion left, Quaternion right)
        {
            // A = (a; U)
            // B = (b; V)
            // AB = (ab - U・V; aV + bU + U×V)
            Quaternion ans;
            double d1, d2, d3, d4;

            d1 = left.t * right.t;
            d2 = left.x * right.x;
            d3 = left.y * right.y;
            d4 = left.z * right.z;
            // ab - U・V
            ans.t = d1 - (d2 + d3 + d4);

            d1 = left.t * right.x;
            d2 = right.t * left.x;
            d3 = left.y * right.z;
            d4 = -left.z * right.y;
            ans.x = d1 + d2 + d3 + d4;

            d1 = left.t * right.y;
            d2 = right.t * left.y;
            d3 = left.z * right.x;
            d4 = -left.x * right.z;
            ans.y = d1 + d2 + d3 + d4;

            d1 = left.t * right.z;
            d2 = right.t * left.z;
            d3 = left.x * right.y;
            d4 = -left.y * right.x;
            ans.z = d1 + d2 + d3 + d4;

            return ans;
        }

        // 共役四元数を返す
        public Quaternion Conjugate()
        {
            Quaternion q = this;

            q.t = t;
            q.x = -x;
            q.y = -y;
            q.z = -z;

            return q;
        }

        public static Quaternion CreateRotateQuaternion(double radian, double axisX, double axisY, double axisZ)
        {
            Quaternion ans = default(Quaternion);
            double norm;
            double c, s;

            norm = axisX * axisX + axisY * axisY + axisZ * axisZ;
            if (norm <= 0.0) return ans;

            norm = 1.0 / Math.Sqrt(norm);
            axisX *= norm;
            axisY *= norm;
            axisZ *= norm;

            c = Math.Cos(0.5 * radian);
            s = Math.Sin(0.5 * radian);

            ans.t = c;
            ans.x = s * axisX;
            ans.y = s * axisY;
            ans.z = s * axisZ;

            return ans;
        }

        // axisは、単位vectorでなければならない
        public static Quaternion CreateRotateQuaternion(double radian, CadPoint axis)
        {
            Quaternion ans = default(Quaternion);
            double c, s;

            c = Math.Cos(0.5 * radian);
            s = Math.Sin(0.5 * radian);

            ans.t = c;
            ans.x = s * axis.x;
            ans.y = s * axis.y;
            ans.z = s * axis.z;

            return ans;
        }

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
