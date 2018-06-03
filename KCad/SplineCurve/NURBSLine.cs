using CadDataTypes;
using Plotter;
using System;

namespace SplineCurve
{
    public class NURBSLine
    {
        public bool Closed;

        // �[�_��ʂ�
        public bool PassEdge;

        // �������B
        public int DividedCount = 24;

        // ����_���X�g
        public VectorList CtrlPoints = null;

        // Control point �̌�������̐�
        // Close�̏ꍇ�́A���ۂ̐� + Degree�ɐݒ肷��
        private int CtrlCnt = 0;

        // Control point�̃��X�g��ł̐�
        private int CtrlDataCnt;

        // �o�͂����Point�̌�
        public int OutCnt
        {
            get
            {
                return DividedCount + 1;
            }
        }

        BSplineParam BSplineP = new BSplineParam();

        public double[] Weights;

        public NURBSLine(int deg,
            int ctrlCnt,
            int divCnt,
            bool edge,
            bool close)
        {
            Closed = close;

            PassEdge = edge;

            CtrlDataCnt = ctrlCnt;

            CtrlCnt = ctrlCnt;
            if (Closed)
            {
                CtrlCnt += deg;
            }

            BSplineP.Setup(deg, CtrlCnt, divCnt, edge);

            ResetWeights();
        }

        private CadVector CalcPoint(double t)
        {
            CadVector linePoint = CadVector.Zero;
			double weight = 0f;

            double bs;

            int i;

            int di;

            for (i = 0; i < CtrlCnt; ++i)
            {
				bs = BSplineP.BasisFunc(i, t);

                di = i % CtrlDataCnt;

                linePoint += bs * Weights[di] * CtrlPoints[di];

                weight += bs * Weights[di];
			}

            return linePoint / weight;
		}

        // ���������_��]�����Ԃ��B
        public void Eval(VectorList vl)
        {
            for (int p = 0; p <= DividedCount; ++p)
            {
                double t = p * BSplineP.Step + BSplineP.LowKnot;
                if (t >= BSplineP.HighKnot)
                {
                    t = BSplineP.HighKnot - BSpline.Epsilon;
                }

                vl.Add( CalcPoint(t) );
            }
        }

        // �d�݂̃��Z�b�g�B
        public void ResetWeights()
        {
            Weights = new double[CtrlDataCnt];

            for (int i = 0; i < Weights.Length; ++i)
            {
                Weights[i] = 1f;
            }
        }
    }
}