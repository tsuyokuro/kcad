using CadDataTypes;

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
        public CadVector p0;
        public CadVector p1;
        public CadVector p2;
        public CadVector p3;
        public CadVector p4;

        public static ArrowHead Create(ArrowTypes type, ArrowPos pos, double len, double width)
        {
            ArrowHead a = default(ArrowHead);

            double w2 = width / 2;

            if (pos == ArrowPos.END)
            {
                if (type == ArrowTypes.CROSS)
                {
                    a.p0 = CadVector.Create(0, 0, 0);
                    a.p1 = CadVector.Create(-len, w2, w2);
                    a.p2 = CadVector.Create(-len, w2, -w2);
                    a.p3 = CadVector.Create(-len, -w2, -w2);
                    a.p4 = CadVector.Create(-len, -w2, w2);
                }
                else if (type == ArrowTypes.PLUS)
                {
                    a.p0 = CadVector.Create(0, 0, 0);
                    a.p1 = CadVector.Create(-len, w2, 0);
                    a.p2 = CadVector.Create(-len, 0, -w2);
                    a.p3 = CadVector.Create(-len, -w2, 0);
                    a.p4 = CadVector.Create(-len, 0, w2);
                }

            }
            else
            {
                if (type == ArrowTypes.CROSS)
                {
                    a.p0 = CadVector.Create(0, 0, 0);
                    a.p1 = CadVector.Create(len, w2, w2);
                    a.p2 = CadVector.Create(len, w2, -w2);
                    a.p3 = CadVector.Create(len, -w2, -w2);
                    a.p4 = CadVector.Create(len, -w2, w2);
                }
                else if (type == ArrowTypes.PLUS)
                {
                    a.p0 = CadVector.Create(0, 0, 0);
                    a.p1 = CadVector.Create(len, w2, 0);
                    a.p2 = CadVector.Create(len, 0, -w2);
                    a.p3 = CadVector.Create(len, -w2, 0);
                    a.p4 = CadVector.Create(len, 0, w2);
                }
            }

            return a;
        }

        public static ArrowHead operator +(ArrowHead a, CadVector d)
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
