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

        System.Windows.Forms.Control FromsControl
        {
            get;
        }

        void SetController(PlotterController controller);
    }
}
