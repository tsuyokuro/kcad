using System;

namespace Plotter
{
    public struct MarkPoint
    {
        public enum Types : byte
        {
            POINT = 0,
            RELATIVE_POINT = 1,
            IDEPEND_POINT = 2,
        }

        public Types Type { get; set; }

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

        public CadPoint Point;

        public CadPoint ViewPoint;

        public uint Flag;

        public double DistX;
        public double DistY;
        public double DistZ;

        public void reset()
        {
            this = default(MarkPoint);

            Figure = null;
            Flag = 0;

            DistX = CadConst.MaxValue;
            DistY = CadConst.MaxValue;
            DistZ = CadConst.MaxValue;
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
