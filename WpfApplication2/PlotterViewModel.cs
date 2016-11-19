using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
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

        private Dictionary<string, Action> commandMap;

        private PlotterController.SelectModes mSelectMode = PlotterController.SelectModes.POINT;

        public ObservableCollection<CadLayer> LayerList = new ObservableCollection<CadLayer>();

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

        ListView mLayerListView;

        public ListView LayerListView
        {
            set
            {
                if (value == null)
                {
                    if (mLayerListView != null)
                    {
                        mLayerListView.SelectionChanged -= LayerListSelectionChanged;
                    }
                }
                else
                {
                    value.SelectionChanged += LayerListSelectionChanged;
                    int idx = getLayerListIndex(mPlotter.CurrentLayer.ID);
                    value.SelectedIndex = idx;
                }

                mLayerListView = value;
            }

            get
            {
                return mLayerListView;
            }
        }

        public PlotterViewModel(PlotterView plotterView)
        {
            initCommandMap();

            mPlotterView = plotterView;
            mPlotter = mPlotterView.Controller;
            SelectMode = mPlotter.SelectMode;
            FigureType = mPlotter.CreatingFigType;

            mPlotter.StateChanged = StateChanged;

            InteractIn.print = MessageOut;

            mPlotter.Interact = InteractIn;

            mPlotter.LayerListChanged =  LayerListChanged;
        }

        public void initCommandMap()
        {
            commandMap = new Dictionary<string, Action>{
                { "load", load },
                { "save",save},
                { "print",startPrint},
                { "undo",undo},
                { "redo",redo},
                { "copy",Copy},
                { "paste",Paste},
                { "separate",separateFigure},
                { "bond",bondFigure},
                { "to_bezier",toBezier},
                { "cut_segment",cutSegment},
                { "add_center_point", addCenterPoint},
            };
        }

        public void StateChanged(PlotterController sender, PlotterController.StateInfo si)
        {
            if (FigureType != si.CreatingFigureType)
            {
                FigureType = si.CreatingFigureType;
            }
        }

        public void LayerListChanged(PlotterController sender, PlotterController.LayerListInfo layerListInfo)
        {
            LayerList.Clear();

            foreach (CadLayer layer in layerListInfo.LayerList)
            {
                LayerList.Add(layer);
            }

            if (mLayerListView != null)
            {
                int idx = getLayerListIndex(layerListInfo.CurrentID);
                mLayerListView.SelectedIndex = idx;
            }
        }

        public void LayerListSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0)
            {
                CadLayer layer = (CadLayer)args.AddedItems[0];

                if (mPlotter.CurrentLayer.ID != layer.ID)
                {
                    mPlotter.setCurrentLayer(layer.ID);
                }
            }
            else
            {
            }
        }

        private void draw()
        {
            DrawContext dc = mPlotterView.startDraw();
            mPlotter.draw(dc);
            mPlotterView.endDraw();
        }

        private int getLayerListIndex(uint id)
        {
            int idx = 0;
            foreach (CadLayer layer in LayerList)
            {
                if (layer.ID == id)
                {
                    return idx;
                }
                idx++;
            }

            return -1;
        }

        public void perviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        public void onKeyDown(object sender, KeyEventArgs e)
        {

            ModifierKeys modifierKeys = Keyboard.Modifiers;

            if ((modifierKeys & ModifierKeys.Control) != ModifierKeys.None)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        undo();
                        break;

                    case Key.Y:
                        redo();
                        break;

                    case Key.C:
                    case Key.Insert:
                        Copy();
                        break;

                    case Key.V:
                        Paste();
                        break;
                }
            }
            else if ((modifierKeys & ModifierKeys.Shift) != ModifierKeys.None)
            {
                switch (e.Key)
                {
                    case Key.Insert:
                        Paste();
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        remove();
                        break;
                }
            }
        }

        public void onKeyUp(object sender, KeyEventArgs e)
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

        private DrawContext startDraw()
        {
            return mPlotterView.startDraw();
        }

        private void endDraw()
        {
            mPlotterView.endDraw();
        }


        public void undo()
        {
            DrawContext dc = startDraw();
            mPlotter.undo(dc);
            endDraw();
        }

        public void redo()
        {
            DrawContext dc = startDraw();
            mPlotter.redo(dc);
            endDraw();
        }

        public void remove()
        {
            DrawContext dc = startDraw();
            mPlotter.remove(dc);
            endDraw();
        }

        public void separateFigure()
        {
            DrawContext dc = startDraw();
            mPlotter.separateFigures(dc);
            endDraw();
        }

        public void bondFigure()
        {
            DrawContext g = startDraw();
            mPlotter.bondFigures(g);
            endDraw();
        }

        public void toBezier()
        {
            DrawContext dc = startDraw();
            mPlotter.toBezier(dc);
            endDraw();
        }

        public void cutSegment()
        {
            DrawContext dc = startDraw();
            mPlotter.cutSegment(dc);
            endDraw();
        }

        public void addCenterPoint()
        {
            DrawContext dc = startDraw();
            mPlotter.addCenterPoint(dc);
            endDraw();
        }

        public void Copy()
        {
            DrawContext dc = startDraw();
            mPlotter.Copy(dc);
            endDraw();
        }

        public void Paste()
        {
            DrawContext dc = startDraw();
            mPlotter.Paste(dc);
            endDraw();
        }

        public void load()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFile(ofd.FileName);
            }
        }

        public void save()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(sfd.FileName);
            }
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
            dc.PageSize = mPlotterView.PageSize;

            // Default printers's unit is 1/100 inch
            dc.setUnitPerInch(100.0, 100.0);

            CadPixelPoint org = default(CadPixelPoint);

            org.x = dc.PageSize.widthInch / 2.0 * 100;
            org.y = dc.PageSize.heightInch / 2.0 * 100;

            dc.ViewOrg = org;

            mPlotter.print(dc);
        }
        #endregion


        #region Command handling
        public void textCommand(string s)
        {
            mPlotter.command(s);
        }

        public void menuCommand(string tag)
        {
            Action action = commandMap[tag];
            action?.Invoke();
        }

        public void debugCommand(string s)
        {
            DrawContext dc = mPlotterView.startDraw();
            mPlotter.debugCommand(dc, s);
            mPlotterView.endDraw();
        }
        #endregion
    }
}
