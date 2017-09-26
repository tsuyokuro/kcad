using System;

namespace Plotter
{
    public struct MarkPoint
    {
        public static UInt32 X_MATCH = 1;
        public static UInt32 Y_MATCH = 2;
        public static UInt32 Z_MATCH = 4;

        public uint LayerID;
        public uint FigureID
        {
            get
            {
                if (Figure == null)
                {
                    return 0;
                }

                return Figure.ID;
            }
        }

        public CadFigure Figure;
        public int PointIndex;

        public CadVector Point;

        public CadVector ViewPoint;

        public uint Flag;

        public double DistanceX;
        public double DistanceY;
        public double DistanceZ;

        public void reset()
        {
            this = default(MarkPoint);

            Figure = null;
            Flag = 0;

            DistanceX = CadConst.MaxValue;
            DistanceY = CadConst.MaxValue;
            DistanceZ = CadConst.MaxValue;
        }

        public void dump(DebugOut dout)
        {
            dout.println("MarkPoint {");
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PointIndex:" + PointIndex.ToString());
            dout.println("}");
        }
    }
}
