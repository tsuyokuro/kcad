//#define USE_GDI_VIEW
using Plotter.Controller;
using Plotter;

namespace KCad.ViewModel
{
    public abstract class ViewModelContext
    {
        protected PlotterController mController;

        public PlotterController Controller
        {
            get => mController;
        }

        public void Redraw()
        {
            ThreadUtil.RunOnMainThread(mController.Redraw, true);
        }

        public abstract void DrawModeUpdated(DrawTools.DrawMode mode);
    }
}
