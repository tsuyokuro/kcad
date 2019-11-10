using CadDataTypes;
using System;

namespace Plotter
{
    // PolyLineの直線部分に含めてスプライン曲線部分を点リストに展開する
    public static class PolyLineExpander
    {
        public static VertexList GetExpandList(
            VertexList src,
            int curveSplitNum)
        {
            int start = 0;
            int cnt = src.Count;

            VertexList ret = new VertexList(curveSplitNum * ((cnt + 1) / 2));

            ForEachPoints<Object>(src, start, cnt, curveSplitNum, (v, d) => { ret.Add(v); return null; }, null);

            return ret;
        }

        //public static VertexList GetExpandList(
        //    VertexList src,
        //    int start, int cnt,
        //    int curveSplitNum)
        //{
        //    VertexList ret = new VertexList(curveSplitNum * ((cnt + 1) / 2));

        //    ForEachPoints<Object>(src, start, cnt, curveSplitNum, (v, d) => { ret.Add(v); return null; }, null);

        //    return ret;
        //}

        public static CadVertex ForEachPoints<T>(
            VertexList src,
            int start, int cnt,
            int curveSplitNum,
            Func<CadVertex, T, T> func, T param)
        {
            VertexList pl = src;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            int i = start;
            int end = start + cnt - 1;

            CadVertex last = default;

            for (; i <= end;)
            {
                if (i + 3 <= end)
                {
                    if (pl[i + 1].IsHandle &&
                        pl[i + 2].IsHandle)
                    {
                        last = CadUtil.ForEachBezierPoints4<T>(pl[i], pl[i + 1], pl[i + 2], pl[i + 3], curveSplitNum, func, param);

                        i += 4;
                        continue;
                    }
                    else if (pl[i + 1].IsHandle && !pl[i + 2].IsHandle)
                    {
                        last = CadUtil.ForEachBezierPoints3<T>(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, func, param);

                        i += 3;
                        continue;
                    }
                }

                if (i + 2 <= end)
                {
                    if (pl[i + 1].IsHandle && !pl[i + 2].IsHandle)
                    {
                        last = CadUtil.ForEachBezierPoints3<T>(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, func, param);

                        i += 3;
                        continue;
                    }
                }

                param = func(pl[i], param);
                last = pl[i];
                i++;
            }

            return last;
        }

        public static CadVertex ForEachSegs<T>(
            VertexList src, bool isloop, 
            int curveSplitNum,
            Action<CadVertex, CadVertex, T> action, T param)
        {
            VertexList pl = src;

            int start = 0;
            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            CadVertex p0 = src[start];

            int i = start + 1;
            int end = start + cnt - 1;

            for (; i <= end;)
            {
                if (i + 3 <= end)
                {
                    if (pl[i + 1].IsHandle &&
                        pl[i + 2].IsHandle)
                    {
                        action(p0, pl[i], param);

                        p0 = CadUtil.ForEachBezierSegs4<T>(pl[i], pl[i + 1], pl[i + 2], pl[i + 3], curveSplitNum, action, param);
                        
                        i += 4;
                        continue;
                    }
                    else if (pl[i + 1].IsHandle && !pl[i + 2].IsHandle)
                    {
                        action(p0, pl[i], param);

                        p0 = CadUtil.ForEachBezierSegs3<T>(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, action, param);

                        i += 3;
                        continue;
                    }
                }

                if (i + 2 <= end)
                {
                    if (pl[i + 1].IsHandle && !pl[i + 2].IsHandle)
                    {
                        action(p0, pl[i], param);

                        p0 = CadUtil.ForEachBezierSegs3<T>(pl[i], pl[i + 1], pl[i + 2], curveSplitNum, action, param);

                        i += 3;
                        continue;
                    }
                }

                action(p0, pl[i], param);
                p0 = pl[i];
                i++;
            }

            if (isloop)
            {
                action(p0, pl[0], param);
            }

            return p0;
        }
    }
}
