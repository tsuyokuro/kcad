using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplineCurve
{
    public class BSplineJson
    {
        public static JObject BSplineParamToJson(BSplineParam bs)
        {
            JObject jbs = new JObject();

            jbs.Add("Degree", bs.Degree);
            jbs.Add("DivCnt", bs.DivCnt);
            jbs.Add("OutputCnt", bs.OutputCnt);
            jbs.Add("KnotCnt", bs.KnotCnt);
            jbs.Add("Knots", ToJArray<double>(bs.Knots));
            jbs.Add("LowKnot", bs.LowKnot);
            jbs.Add("HighKnot", bs.HighKnot);
            jbs.Add("Step", bs.Step);

            return jbs;
        }

        public static BSplineParam BSplineParamFromJson(JObject jo)
        {
            BSplineParam bs = new BSplineParam();

            bs.Degree = (int)jo["Degree"];
            bs.DivCnt = (int)jo["DivCnt"];
            bs.OutputCnt = (int)jo["OutputCnt"];
            bs.KnotCnt = (int)jo["KnotCnt"];
            bs.Knots = DoubleArrayFromJArray((JArray)jo["Knots"]);
            bs.LowKnot = (double)jo["LowKnot"];
            bs.HighKnot = (double)jo["HighKnot"];
            bs.Step = (double)jo["Step"];

            return bs;
        }

        public static JObject NURBSLineToJson(NURBSLine n)
        {
            JObject jn = new JObject();

            jn.Add("Closed", n.Closed);
            jn.Add("PassEdge", n.PassEdge);
            jn.Add("CtrlCnt", n.CtrlCnt);
            jn.Add("CtrlDataCnt", n.CtrlDataCnt);
            jn.Add("Weights", ToJArray<double>(n.Weights));
            jn.Add("Order", ToJArray<int>(n.Order));

            jn.Add("BSplineP", BSplineParamToJson(n.BSplineP));

            return jn;
        }

        public static NURBSLine NURBSLineFromJson(JObject jo)
        {
            NURBSLine n = new NURBSLine();
            n.Closed = (bool)jo["Closed"];
            n.PassEdge = (bool)jo["PassEdge"];
            n.CtrlCnt = (int)jo["CtrlCnt"];
            n.CtrlDataCnt = (int)jo["CtrlDataCnt"];
            n.Weights = DoubleArrayFromJArray((JArray)jo["Weights"]);
            n.Order = IntArrayFromJArray((JArray)jo["Order"]);

            n.BSplineP = BSplineParamFromJson((JObject)jo["BSplineP"]);

            return n;
        }

        public static JArray ToJArray<T>(T[] t)
        {
            JArray ja = new JArray();

            for (int i = 0; i < t.Length; i++)
            {
                ja.Add(t[i]);
            }

            return ja;
        }

        public static double[] DoubleArrayFromJArray(JArray ja)
        {
            if (ja == null)
            {
                return null;
            }

            int cnt = ja.Count;

            double[] ret = new double[cnt];

            for (int i = 0; i < cnt; i++)
            {
                ret[i] = (double)ja[i];
            }

            return ret;
        }

        public static int[] IntArrayFromJArray(JArray ja)
        {
            if (ja==null)
            {
                return null;
            }

            int cnt = ja.Count;

            int[] ret = new int[cnt];

            for (int i = 0; i < cnt; i++)
            {
                ret[i] = (int)ja[i];
            }

            return ret;
        }

    }
}
