using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    // 円を点リストに展開
    public static class CircleExpander
    {
        public static VectorList GetExpandList(
            CadVector cp, CadVector pa, CadVector pb,
            int splitCnt)
        {
            VectorList ret = new VectorList(splitCnt + 1);

            ForEachPoints(cp , pa, pb, splitCnt, (v)=> { ret.Add(v); });

            return ret;
        }

        public static void ForEachSegs(
            CadVector cp, CadVector pa, CadVector pb,
            int splitCnt,
            Action<CadVector, CadVector> action)
        {
            CadVector va = pa - cp;
            CadVector vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }


            double dt = (2.0 * Math.PI) / (double)splitCnt;

            int div = splitCnt;

            CadVector normal = CadMath.Normal(va, vb);

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, dt);
            CadQuaternion r = q.Conjugate();

            CadVector p = va;
            CadVector tp1 = pa;
            CadVector tp2 = pa;


            int i = 0;
            for (; i < div - 1; i++)
            {
                CadQuaternion qp = CadQuaternion.FromPoint(p);
                qp = r * qp;
                qp = qp * q;

                p = qp.ToPoint();

                tp2 = p + cp;

                action(tp1, tp2);
                tp1 = tp2;
            }

            action(tp1, pa);
        }


        public static void ForEachPoints(
            CadVector cp, CadVector pa, CadVector pb,
            int splitCnt,
            Action<CadVector> action)
        {
            CadVector va = pa - cp;
            CadVector vb = pb - cp;

            if (va.Norm() < 0.01)
            {
                return;
            }

            double dt = (2.0 * Math.PI) / (double)splitCnt;

            int div = splitCnt;

            CadVector normal = CadMath.Normal(va, vb);

            CadQuaternion q = CadQuaternion.RotateQuaternion(normal, dt);
            CadQuaternion r = q.Conjugate();

            CadVector p = va;
            CadVector tp = pa;


            int i = 0;
            for (; i < div - 1; i++)
            {
                CadQuaternion qp = CadQuaternion.FromPoint(p);
                qp = r * qp;
                qp = qp * q;

                p = qp.ToPoint();

                tp = p + cp;

                action(tp);
            }
        }
    }
}
