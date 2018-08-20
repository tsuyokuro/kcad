using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    public struct VLExpander
    {
        public delegate void delegateForEach(
            VectorList src,
            int curveSplitNum,
            Action<CadVector> action);

        public delegate void delegateForEachPart(
            VectorList src,
            int start,
            int cnt,
            int curveSplitNum,
            Action<CadVector> action);


        public int CurveSplitNum;

        public delegateForEach ForEachPoints;

        public delegateForEachPart ForEachPointsPart;


        public static VLExpander CreateForPolyLine(int curveSplitNum = 32)
        {
            VLExpander vle = default(VLExpander);

            vle.CurveSplitNum = curveSplitNum;
            vle.ForEachPoints = PolyLineExpander.ForEachPoints;
            vle.ForEachPointsPart = PolyLineExpander.ForEachPoints;

            return vle;
        }

        public void ForEachExpandPoints(
            VectorList src,
            Action<CadVector> action)
        {
            ForEachPoints(src, CurveSplitNum, action);
        }

        public void ForEachExpandPointsPart(
            VectorList src,
            int start, int cnt,
            Action<CadVector> action)
        {
            ForEachPointsPart(src, start, cnt, CurveSplitNum, action);
        }
    }

    public static class PolyLineExpander
    {
        public static VectorList GetExpandList(
            VectorList src,
            int curveSplitNum)
        {
            return GetExpandList(src, 0, src.Count, curveSplitNum);
        }

        public static VectorList GetExpandList(
            VectorList src,
            int start, int cnt,
            int curveSplitNum)
        {
            VectorList ret = new VectorList(curveSplitNum * ((cnt + 1) / 2));

            ForEachPoints(src, start, cnt, curveSplitNum, action);

            void action(CadVector v)
            {
                ret.Add(v);
            }

            return ret;
        }

        public static void ForEachPoints(
            VectorList src,
            int curveSplitNum,
            Action<CadVector> action)
        {
            ForEachPoints(src, 0, src.Count, curveSplitNum, action);
        }

        public static void ForEachPoints(
            VectorList src,
            int start, int cnt,
            int curveSplitNum,
            Action<CadVector> action)
        {
            VectorList pl = src;

            if (cnt <= 0)
            {
                return;
            }

            int i = start;
            int end = start + cnt - 1;

            for (; i <= end;)
            {
                if (i + 3 <= end)
                {
                    if (pl[i + 1].IsHandle &&
                        pl[i + 2].IsHandle)
                    {
                        CadUtil.ForEachBezierPoints(pl[i], pl[i + 1], pl[i + 2], pl[i + 3], curveSplitNum, action);

                        i += 4;
                        continue;
                    }
                    else if (pl[i + 1].IsHandle && !pl[i + 2].IsHandle)
                    {
                        CadUtil.ForEachBezierPoints(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, action);

                        i += 3;
                        continue;
                    }
                }

                if (i + 2 <= end)
                {
                    if (pl[i + 1].IsHandle && !pl[i + 2].IsHandle)
                    {
                        CadUtil.ForEachBezierPoints(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, action);

                        i += 3;
                        continue;
                    }
                }

                action(pl[i]);
                i++;
            }
        }
    }

    public static class CircleExpander
    {
        public static VectorList GetExpandList(
            CadVector cp, CadVector pa, CadVector pb,
            int splitCnt)
        {
            VectorList ret = new VectorList(splitCnt + 1);

            ForEachPoints(cp , pa, pb, splitCnt, action);

            void action(CadVector v)
            {
                ret.Add(v);
            }

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
