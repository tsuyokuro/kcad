using CadDataTypes;
using Plotter;
using System;

namespace SplineCurve
{
    public class NURBSSerface
    {
        public bool UClosed = false;
        public bool VClosed = false;

        public bool UPassEdge = false;
        public bool VPassEdge = false;

        // Control point の見かけ上の数
        // Closeの場合は、実際の数 + Degreeに設定する
        public int UCtrlCnt = 5;
        public int VCtrlCnt = 5;

        // Control pointのリスト上での数
        public int UCtrlDataCnt = 5;
        public int VCtrlDataCnt = 5;

        // 制御点リスト
        public VectorList CtrlPoints = null;

        // Weight情報
        public double[,] Weights;

        BSplineParam UBSpline = new BSplineParam();
        BSplineParam VBSpline = new BSplineParam();

        // U方向の出力されるPointの数
        public int UOutCnt
        {
            get { return UBSpline.OutputCnt; }
        }

        // V方向の出力されるPointの数
        public int VOutCnt
        {
            get { return VBSpline.OutputCnt; }
        }

        public NURBSSerface(
            int deg,
            int uCtrlCnt, int vCtrlCnt,
            int uDivCnt, int vDivCnt,
            bool uedge, bool vedge,
            bool uclose, bool vclose
            )
        {
            UClosed = uclose;
            VClosed = vclose;

            UCtrlDataCnt = uCtrlCnt;
            VCtrlDataCnt = vCtrlCnt;

            UCtrlCnt = uCtrlCnt;
            if (UClosed)
            {
                UCtrlCnt += deg;
            }

            VCtrlCnt = vCtrlCnt;
            if (VClosed)
            {
                VCtrlCnt += deg;
            }

            UBSpline.Setup(deg, UCtrlCnt, uDivCnt, uedge);
            VBSpline.Setup(deg, VCtrlCnt, uDivCnt, vedge);

            ResetWeights();
        }

        public void ResetWeights()
        {
            Weights = new double[UCtrlDataCnt, VCtrlDataCnt];

            for (int j = 0; j < VCtrlDataCnt; j++)
            {
                for (int i = 0; i < UCtrlDataCnt; i++)
                {
                    Weights[i, j] = 1.0;
                }
            }
        }

        private CadVector CalcPoint(double u, double v)
        {
            CadVector pt = CadVector.Zero;

            double weight = 0f;

            int sp;

            int vcnt = VCtrlCnt;
            int ucnt = UCtrlCnt;

            int di;
            int dj;

            for (int j = 0; j < vcnt; ++j)
            {
                dj = j % VCtrlDataCnt;

                sp = UCtrlDataCnt * dj;

                for (int i = 0; i < ucnt; ++i)
                {
                    double ubs = UBSpline.BasisFunc(i, u);
                    double vbs = VBSpline.BasisFunc(j, v);

                    di = i % UCtrlDataCnt;

                    int cp = sp + di;

                    pt += (ubs * vbs * Weights[di, dj]) * CtrlPoints[cp];

                    weight += ubs * vbs * Weights[di, dj];
                }
            }

            return pt / weight;
        }

        public void Eval(VectorList vl)
        {
            double u;
            double v;

            for (int j = 0; j <= VBSpline.DivCnt; ++j)
            {
                v = j * VBSpline.Step + VBSpline.LowKnot;
                if (v >= VBSpline.HighKnot)
                {
                    v = VBSpline.HighKnot - BSpline.Epsilon;
                }

                for (int i = 0; i <= UBSpline.DivCnt; ++i)
                {
                    u = i * UBSpline.Step + UBSpline.LowKnot;
                    if (u >= UBSpline.HighKnot)
                    {
                        u = UBSpline.HighKnot - BSpline.Epsilon;
                    }

                    vl.Add(CalcPoint(u, v));
                }
            }
        }
    }
}
