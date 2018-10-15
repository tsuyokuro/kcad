using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    // PolyLineの直線部分に含めてスプライン曲線部分を点リストに展開する
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

            ForEachPoints(src, start, cnt, curveSplitNum, (v)=> { ret.Add(v); });

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
}
