using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;


namespace Plotter
{
    public class EnumBoolConverter<T> : IValueConverter
    {
        // Convert parameter to Enum
        private T ConvertParameter(object parameter)
        {
            string parameterString = parameter as string;
            return (T)Enum.Parse(typeof(T), parameterString);
        }

        // Enum -> bool
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            T parameterValue = ConvertParameter(parameter);

            return parameterValue.Equals(value);
        }

        // bool -> Enum
        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            // ignore case that true->false
            if (!(bool)value)
                return System.Windows.DependencyProperty.UnsetValue;

            return ConvertParameter(parameter);
        }
    }

    public class SelectModeConverter : EnumBoolConverter<PlotterController.SelectModes> { }

    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }



    public class PlotterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PlotterController mPlotter = null;

        private PlotterView mPlotterView = null;

        public PlotterController.Interaction InteractOut =
            new PlotterController.Interaction();

        public PlotterController.Interaction InteractIn =
            new PlotterController.Interaction();

        private PlotterController.SelectModes mSelectMode = PlotterController.SelectModes.POINT;

        public PlotterController.SelectModes SelectMode
        {
            set
            {
                mSelectMode = value;
                mPlotter.SelectMode = mSelectMode;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectMode)));
            }

            get
            {
                return mSelectMode;
            }
        }


        private CadFigure.Types mFigureType = CadFigure.Types.NONE;

        public CadFigure.Types FigureType
        {
            set
            {
                var prev = mFigureType;

                if (mFigureType == value)
                {
                    mFigureType = CadFigure.Types.NONE;
                }
                else
                {
                    mFigureType = value;
                }

                if (mFigureType != CadFigure.Types.NONE)
                {
                    mPlotter.startCreateFigure(mFigureType);
                }
                else if (prev != CadFigure.Types.NONE)
                {
                    mPlotter.endCreateFigure();

                    DrawContext dc = mPlotterView.startDraw();
                    mPlotter.draw(dc);
                    mPlotterView.endDraw();
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FigureType)));
            }

            get
            {
                return mFigureType;
            }
        }

        public PlotterViewModel(PlotterView plotterView)
        {
            mPlotterView = plotterView;
            mPlotter = mPlotterView.Controller;
            SelectMode = mPlotter.SelectMode;
            FigureType = mPlotter.CreatingFigType;

            mPlotter.StateChanged = StateChanged;

            InteractIn.print = MessageOut;

            mPlotter.Interact = InteractIn;
        }

        public void StateChanged(PlotterController sender, PlotterController.StateInfo si)
        {
            if (FigureType != si.CreatingFigureType)
            {
                FigureType = si.CreatingFigureType;
            }
        }

        public void debugCommand(string s)
        {
            DrawContext dc = mPlotterView.startDraw();
            mPlotter.debugCommand(dc, s);
            mPlotterView.endDraw();
        }

        public void menuCommand(string tag)
        {
            switch (tag)
            {
                case "load":
                    break;

                case "save":
                    break;

                case "print":
                    startPrint();
                    break;

            }
        }

        public void textCommand(string s)
        {
            mPlotter.command(s);
            DrawContext dc = mPlotterView.startDraw();
            mPlotter.draw(dc);
            mPlotterView.endDraw();
        }

        public void perviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void MessageOut(String s)
        {
            InteractOut.print(s);
        }

        public void SaveFile(String fname)
        {
            mPlotter.SaveToJsonFile(fname);
        }

        public void LoadFile(String fname)
        {
            mPlotter.LoadFromJsonFile(fname);

            DrawContext dc = mPlotterView.startDraw();

            mPlotter.clear(dc);

            mPlotter.draw(dc);

            mPlotterView.endDraw();
        }

        #region "print"
        public void startPrint()
        {
            System.Drawing.Printing.PrintDocument pd =
                new System.Drawing.Printing.PrintDocument();

            pd.PrintPage += printPage;
                //new System.Drawing.Printing.PrintPageEventHandler(printPage);

            System.Windows.Forms.PrintDialog pdlg = new System.Windows.Forms.PrintDialog();

            pdlg.Document = pd;

            if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pd.Print();
            }
        }

        private void printPage(object sender,
            System.Drawing.Printing.PrintPageEventArgs e)
        {
            drawPage(e.Graphics);
        }

        private void drawPage(System.Drawing.Graphics g)
        {
            DrawContext dc = new DrawContext();

            dc.graphics = g;
            dc.Tools.setupPrinterSet();
            dc.PageSize = mPlotterView.getPageSize();

            // Default printers's unit is 1/100 inch
            dc.setUnitPerInch(100.0, 100.0);

            CadPixelPoint org = default(CadPixelPoint);

            org.x = dc.PageSize.widthInch / 2.0 * 100;
            org.y = dc.PageSize.heightInch / 2.0 * 100;

            dc.ViewOrg = org;

            mPlotter.print(dc);
        }
        #endregion
    }
}
