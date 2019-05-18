using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public static class VectorExtentions
    {
        public static Vector4d ToVector4d(this Vector3d v, double w)
        {
            return new Vector4d(v.X, v.Y, v.Z, w);
        }

        public static Vector3d ToVector3d(this Vector4d v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static void dump(this Vector3d v, string prefix = nameof(Vector3d))
        {
            DOut.pl(prefix + "{");
            DOut.Indent++;
            DOut.pl("x:" + v.X.ToString());
            DOut.pl("y:" + v.Y.ToString());
            DOut.pl("z:" + v.Z.ToString());
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
