//#define USE_GDI_VIEW
using System.Windows;
using Plotter.Controller;
using Plotter;

namespace KCad.ViewModel
{
    public interface ICadMainWindow
    {
        Window GetWindow();
        void SetCurrentFileName(string file_name);
        void OpenPopupMessage(string text, PlotterObserver.MessageType messageType);
        void ClosePopupMessage();
        void SetPlotterView(IPlotterView view);
    }
}
