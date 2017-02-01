using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public interface IPlotterView
    {
        DrawContext DrawContext
        {
            get;
        }

        PaperPageSize PageSize
        {
            get;
        }

        void SetController(PlotterController controller);

        DrawContext startDraw();
        void endDraw();
    }
}
