using System.Windows;
using Plotter.Controller;
using Plotter;

namespace KCad.ViewModel
{
    public interface ICadMainWindow
    {
        Window GetWindow();
        void SetCurrentFileName(string file_name);
        void OpenPopupMessage(string text, PlotterCallback.MessageType messageType);
        void ClosePopupMessage();
        void SetPlotterView(IPlotterView view);
    }
}
