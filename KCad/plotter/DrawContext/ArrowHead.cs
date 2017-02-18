using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public enum ArrowTypes
    {
        CROSS,  // X
        PLUS,   // +
    }

    public enum ArrowPos
    {
        START,
        END,
        START_END,
    }

    public struct ArrowHead
    {
        public CadPoint p0;
        public CadPoint p1;
        public CadPoint p2;
        public CadPoint p3;
        public CadPoint p4;

        public static ArrowHead Create(ArrowTypes type, ArrowPos pos, double len, double width)
        {
            ArrowHead a = default(ArrowHead);

            double w2 = width / 2;

            if (pos == ArrowPos.END)
            {
                if (type == ArrowTypes.CROSS)
                {
                    a.p0 = CadPoint.Create(0, 0, 0);
                    a.p1 = CadPoint.Create(-len, w2, w2);
                    a.p2 = CadPoint.Create(-len, w2, -w2);
                    a.p3 = CadPoint.Create(-len, -w2, -w2);
                    a.p4 = CadPoint.Create(-len, -w2, w2);
                }
                else if (type == ArrowTypes.PLUS)
                {
                    a.p0 = CadPoint.Create(0, 0, 0);
                    a.p1 = CadPoint.Create(-len, w2, 0);
                    a.p2 = CadPoint.Create(-len, 0, -w2);
                    a.p3 = CadPoint.Create(-len, -w2, 0);
                    a.p4 = CadPoint.Create(-len, 0, w2);
                }

            }
            else
            {
                if (type == ArrowTypes.CROSS)
                {
                    a.p0 = CadPoint.Create(0, 0, 0);
                    a.p1 = CadPoint.Create(len, w2, w2);
                    a.p2 = CadPoint.Create(len, w2, -w2);
                    a.p3 = CadPoint.Create(len, -w2, -w2);
                    a.p4 = CadPoint.Create(len, -w2, w2);
                }
                else if (type == ArrowTypes.PLUS)
                {
                    a.p0 = CadPoint.Create(0, 0, 0);
                    a.p1 = CadPoint.Create(len, w2, 0);
                    a.p2 = CadPoint.Create(len, 0, -w2);
                    a.p3 = CadPoint.Create(len, -w2, 0);
                    a.p4 = CadPoint.Create(len, 0, w2);
                }
            }

            return a;
        }

        public static ArrowHead operator +(ArrowHead a, CadPoint d)
        {
            a.p0 += d;
            a.p1 += d;
            a.p2 += d;
            a.p3 += d;
            a.p4 += d;

            return a;
        }

        public void Rotate(CadQuaternion q, CadQuaternion r)
        {
            CadQuaternion qp;

            qp = CadQuaternion.FromPoint(p0);
            qp = r * qp;
            qp = qp * q;

            p0 = CadQuaternion.ToPoint(qp);


            qp = CadQuaternion.FromPoint(p1);
            qp = r * qp;
            qp = qp * q;

            p1 = CadQuaternion.ToPoint(qp);


            qp = CadQuaternion.FromPoint(p2);
            qp = r * qp;
            qp = qp * q;

            p2 = CadQuaternion.ToPoint(qp);


            qp = CadQuaternion.FromPoint(p3);
            qp = r * qp;
            qp = qp * q;

            p3 = CadQuaternion.ToPoint(qp);


            qp = CadQuaternion.FromPoint(p4);
            qp = r * qp;
            qp = qp * q;

            p4 = CadQuaternion.ToPoint(qp);
        }
    }
}
