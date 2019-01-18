using Plotter.Controller;

namespace Plotter
{
    public interface IPlotterView
    {
        DrawContext DrawContext
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
