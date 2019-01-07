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