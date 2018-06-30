using CadDataTypes;
using Newtonsoft.Json.Linq;
using Plotter.Serializer;
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

        public static JObject NURBSLineToJson(NurbsLine n)
        {
            JObject jn = new JObject();

            JArray pointArray = CadJson.ToJson.VectorListToJson(n.CtrlPoints);
            jn.Add("CtrlPoints", pointArray);
            jn.Add("CtrlCnt", n.CtrlCnt);
            jn.Add("CtrlDataCnt", n.CtrlDataCnt);
            jn.Add("Weights", ToJArray<double>(n.Weights));
            jn.Add("CtrlOrder", ToJArray<int>(n.CtrlOrder));

            jn.Add("BSplineP", BSplineParamToJson(n.BSplineP));

            return jn;
        }

        public static NurbsLine NURBSLineFromJson(JObject jo)
        {
            NurbsLine n = new NurbsLine();

            JArray jarray = (JArray)jo["CtrlPoints"];
            VectorList vl = CadJson.FromJson.VectorListFromJson(jarray);
            n.CtrlPoints = vl;

            n.CtrlCnt = (int)jo["CtrlCnt"];
            n.CtrlDataCnt = (int)jo["CtrlDataCnt"];
            n.Weights = DoubleArrayFromJArray((JArray)jo["Weights"]);
            n.CtrlOrder = IntArrayFromJArray((JArray)jo["CtrlOrder"]);

            n.BSplineP = BSplineParamFromJson((JObject)jo["BSplineP"]);

            return n;
        }

        public static JObject NURBSSurfaceToJson(NurbsSurface n)
        {
            JObject jn = new JObject();
            JArray pointArray = CadJson.ToJson.VectorListToJson(n.CtrlPoints);
            jn.Add("CtrlPoints", pointArray);

            jn.Add("UCtrlCnt", n.UCtrlCnt);
            jn.Add("VCtrlCnt", n.VCtrlCnt);

            jn.Add("UCtrlDataCnt", n.UCtrlDataCnt);
            jn.Add("VCtrlDataCnt", n.VCtrlDataCnt);

            jn.Add("Weights", ToJArray<double>(n.Weights));
            jn.Add("CtrlOrder", ToJArray<int>(n.CtrlOrder));

            jn.Add("UBSpline", BSplineParamToJson(n.UBSpline));
            jn.Add("VBSpline", BSplineParamToJson(n.VBSpline));

            return jn;
        }

        public static NurbsSurface NURBSSurfaceFromJson(JObject jo)
        {
            NurbsSurface n = new NurbsSurface();

            JArray jarray = (JArray)jo["CtrlPoints"];
            VectorList vl = CadJson.FromJson.VectorListFromJson(jarray);
            n.CtrlPoints = vl;

            n.UCtrlCnt = (int)jo["UCtrlCnt"];
            n.VCtrlCnt = (int)jo["VCtrlCnt"];

            n.UCtrlDataCnt = (int)jo["UCtrlDataCnt"];
            n.VCtrlDataCnt = (int)jo["VCtrlDataCnt"];

            n.Weights = DoubleArrayFromJArray((JArray)jo["Weights"]);
            n.CtrlOrder = IntArrayFromJArray((JArray)jo["CtrlOrder"]);

            n.UBSpline = BSplineParamFromJson((JObject)jo["UBSpline"]);
            n.VBSpline = BSplineParamFromJson((JObject)jo["VBSpline"]);

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
