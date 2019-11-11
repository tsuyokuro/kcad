using CadDataTypes;
using System;

namespace Plotter
{
    public static class PolyLineExpander
    {
        public static VertexList GetExpandList(
            VertexList src,
            int curveSplitNum)
        {
            int cnt = src.Count;

            VertexList ret = new VertexList(curveSplitNum * ((cnt + 1) / 2));

            ForEachPoints<Object>(src, curveSplitNum, (v, d) => { ret.Add(v); }, null);

            return ret;
        }

        private enum ScanState
        {
            START,
            MAIN,
            HANDLE_1,
            HANDLE_2,

        }

        public static CadVertex ForEachPoints<T>(
            VertexList src,
            int curveSplitNum,
            Action<CadVertex, T> action, T param)
        {
            VertexList pl = src;

            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            CadVertex p0 = src[0];

            int i = 0;

            ScanState state = ScanState.START;

            for (; i < cnt; i++)
            {
                if (state == ScanState.START)
                {
                    p0 = src[i];
                    action(p0, param);

                    state = ScanState.MAIN;
                }
                else if (state == ScanState.MAIN)
                {
                    if (pl[i].IsHandle)
                    {
                        state = ScanState.HANDLE_1;
                    }
                    else
                    {
                        p0 = pl[i];
                        action(p0, param);
                    }
                }
                else if (state == ScanState.HANDLE_1)
                {
                    if (pl[i].IsHandle)
                    {
                        state = ScanState.HANDLE_2;
                    }
                    else
                    {
                        p0 = CadUtil.ForEachBezierPoints3<T>(pl[i - 2], pl[i - 1], pl[i], curveSplitNum, action, param);
                        state = ScanState.MAIN;
                    }
                }
                else if (state == ScanState.HANDLE_2)
                {
                    p0 = CadUtil.ForEachBezierPoints4<T>(pl[i - 3], pl[i - 2], pl[i - 1], pl[i], curveSplitNum, action, param);
                    state = ScanState.MAIN;
                }
            }

            if (state == ScanState.MAIN)
            {
            }
            else if (state == ScanState.HANDLE_1)
            {
                p0 = CadUtil.ForEachBezierPoints3<T>(pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, action, param);
            }
            else if (state == ScanState.HANDLE_2)
            {
                p0 = CadUtil.ForEachBezierPoints4<T>(pl[cnt - 3], pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, action, param);
            }

            return p0;
        }

        public static CadVertex ForEachSegs<T>(
            VertexList src, bool isloop,
            int curveSplitNum,
            Action<CadVertex, CadVertex, T> action, T param)
        {
            VertexList pl = src;

            int cnt = pl.Count;

            if (cnt <= 0)
            {
                return CadVertex.InvalidValue;
            }

            CadVertex p0 = src[0];

            int i = 0;

            ScanState state = ScanState.START;

            for (; i < cnt; i++)
            {
                if (state == ScanState.START)
                {
                    p0 = src[i];
                    state = ScanState.MAIN;
                }
                else if (state == ScanState.MAIN)
                {
                    if (pl[i].IsHandle)
                    {
                        state = ScanState.HANDLE_1;
                    }
                    else
                    {
                        action(p0, pl[i], param);
                        p0 = pl[i];
                    }
                }
                else if (state == ScanState.HANDLE_1)
                {
                    if (pl[i].IsHandle)
                    {
                        state = ScanState.HANDLE_2;
                    }
                    else
                    {
                        p0 = CadUtil.ForEachBezierSegs3<T>(pl[i-2], pl[i-1], pl[i], curveSplitNum, action, param);
                        state = ScanState.MAIN;
                    }
                }
                else if (state == ScanState.HANDLE_2)
                {
                    p0 = CadUtil.ForEachBezierSegs4<T>(pl[i-3], pl[i-2], pl[i-1], pl[i], curveSplitNum, action, param);
                    state = ScanState.MAIN;
                }
            }

            if (state == ScanState.MAIN) {
                if (isloop)
                {
                    action(p0, pl[0], param);
                }
            }
            else if (state == ScanState.HANDLE_1)
            {
                p0 = CadUtil.ForEachBezierSegs3<T>(pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, action, param);
            }
            else if (state == ScanState.HANDLE_2)
            {
                p0 = CadUtil.ForEachBezierSegs4<T>(pl[cnt - 3], pl[cnt - 2], pl[cnt - 1], pl[0], curveSplitNum, action, param);
            }

            return p0;
        }

    }
}
