using Plotter.Controller;

namespace Plotter
{
    public interface IPlotterView
    {
        DrawContext DrawContext
        {
            get;
        }

        System.Windows.Forms.Control FormsControl
        {
            get;
        }

        void SetController(PlotterController controller);

        void CursorLocked(bool locked);

        void ChangeMouseCursor(PlotterObserver.MouseCursorType cursorType);
    }
}
