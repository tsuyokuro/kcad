using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public static class PolyLineExpander
    {
        public static VectorList GetExpandList(
            VectorList src,
            int start, int cnt,
            int curveSplitNum)
        {
            VectorList ret = new VectorList(curveSplitNum * ((cnt + 1) / 2));

            ForEachExpandPoints(src, start, cnt, curveSplitNum, action);

            void action(CadVector v)
            {
                ret.Add(v);
            }

            return ret;
        }

        public static void ForEachExpandPoints(
            VectorList src,
            int curveSplitNum,
            Action<CadVector> action)
        {
            ForEachExpandPoints(src, 0, src.VList.Count, curveSplitNum, action);
        }

        public static void ForEachExpandPoints(
            VectorList src,
            int start, int cnt,
            int curveSplitNum,
            Action<CadVector> action)
        {
            List<CadVector> pl = src.VList;

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
                    if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                        pl[i + 2].Type == CadVector.Types.HANDLE)
                    {
                        CadUtil.ForEachBezierPoints(pl[i], pl[i + 1], pl[i + 2], pl[i + 3], curveSplitNum, action);

                        i += 4;
                        continue;
                    }
                    else if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                        pl[i + 2].Type == CadVector.Types.STD)
                    {
                        CadUtil.ForEachBezierPoints(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, action);

                        i += 3;
                        continue;
                    }
                }

                if (i + 2 <= end)
                {
                    if (pl[i + 1].Type == CadVector.Types.HANDLE &&
                                            pl[i + 2].Type == CadVector.Types.STD)
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
}
