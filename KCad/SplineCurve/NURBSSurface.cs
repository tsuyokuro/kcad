﻿using CadDataTypes;
using Plotter;
using System;

namespace SplineCurve
{
    public class NurbsSurface
    {
        // Control pointの数
        public int UCtrlCnt = 5;
        public int VCtrlCnt = 5;

        // Control pointのデータ数
        public int UCtrlDataCnt = 5;
        public int VCtrlDataCnt = 5;

        // 制御点リスト
        public VectorList CtrlPoints = null;

        // Weight情報
        public double[] Weights;

        public int[] Order;


        public BSplineParam UBSpline = new BSplineParam();
        public BSplineParam VBSpline = new BSplineParam();

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

        public NurbsSurface()
        {
        }

        public NurbsSurface(
            int deg,
            int uCtrlCnt, int vCtrlCnt,
            int uDivCnt, int vDivCnt,
            bool uedge, bool vedge,
            bool uclose, bool vclose
            )
        {
            Setup(
                deg,
                uCtrlCnt, vCtrlCnt,
                uDivCnt, vDivCnt,
                uedge, vedge,
                uclose, vclose
                );
        }

        public void Setup(
            int deg,
            int uCtrlCnt, int vCtrlCnt,
            int uDivCnt, int vDivCnt,
            bool uedge, bool vedge,
            bool uclose, bool vclose
            )
        {
            UCtrlDataCnt = uCtrlCnt;
            VCtrlDataCnt = vCtrlCnt;

            UCtrlCnt = uCtrlCnt;
            if (uclose)
            {
                UCtrlCnt += deg;
            }

            VCtrlCnt = vCtrlCnt;
            if (vclose)
            {
                VCtrlCnt += deg;
            }

            CreateOrder(UCtrlCnt, VCtrlCnt, uCtrlCnt, vCtrlCnt);

            UBSpline.Setup(deg, UCtrlCnt, uDivCnt, uedge);
            VBSpline.Setup(deg, VCtrlCnt, uDivCnt, vedge);

            SetDefaultWeights();
        }

        public void SetDefaultWeights()
        {
            Weights = new double[UCtrlDataCnt*VCtrlDataCnt];

            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = 1.0;
            }
        }

        private void CreateOrder(int ucnt, int vcnt, int udcnt, int vdcnt)
        {
            Order = new int[ucnt * vcnt];

            for (int j = 0; j < vcnt; j++)
            {
                int ds = (j % vdcnt) * udcnt;
                int s = j * ucnt;

                for (int i = 0; i < UCtrlCnt; i++)
                {
                    Order[s + i] = ds + (i % udcnt);
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

            for (int j = 0; j < vcnt; ++j)
            {
                sp = ucnt * j;

                for (int i = 0; i < ucnt; ++i)
                {
                    double ubs = UBSpline.BasisFunc(i, u);
                    double vbs = VBSpline.BasisFunc(j, v);

                    int cp = Order[sp + i];

                    pt += (ubs * vbs * Weights[cp]) * CtrlPoints[cp];

                    weight += ubs * vbs * Weights[cp];
                }
            }

            return pt / weight;
        }

        public double GetWeight(int u, int v)
        {
            return Weights[v * UCtrlDataCnt + u];
        }

        public void SetWeight(int u, int v, double val)
        {
            Weights[v * UCtrlDataCnt + u] = val;
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
