using System.Collections.Generic;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public void Remove()
        {
            StartEdit();

            RemoveSelectedPoints();

            EndEdit();
        }

        public void FlipX()
        {
            StartEdit();
            Flip(TargetCoord.X);
            EndEdit();
        }

        public void FlipY()
        {
            StartEdit();
            Flip(TargetCoord.Y);
            EndEdit();
        }

        public void FlipZ()
        {
            StartEdit();
            Flip(TargetCoord.Z);
            EndEdit();
        }

        public void InsPoint()
        {
            StartEdit();
            if (InsPointToLastSelectedSeg())
            {
                EndEdit();
            }
            else
            {
                AbendEdit();
            }
        }
    }
}