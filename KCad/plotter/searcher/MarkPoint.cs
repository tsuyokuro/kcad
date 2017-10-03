using System;

namespace Plotter
{
    public struct MarkPoint
    {
        public bool IsValid;

        public CadLayer Layer;
        
        public uint LayerID
        {
            get
            {
                if (Layer == null)
                {
                    return 0;
                }

                return Layer.ID;
            }
        }

        public CadFigure Figure;

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

        public int PointIndex;

        public CadVector Point;

        public CadVector ViewPoint;

        public double DistanceX;
        public double DistanceY;

        public void reset()
        {
            this = default(MarkPoint);

            IsValid = false;

            Figure = null;

            DistanceX = CadConst.MaxValue;
            DistanceY = CadConst.MaxValue;
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
