using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;


namespace Plotter
{
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

        private Dictionary<string, Action> keyMap;

        private PlotterController.SelectModes mSelectMode = PlotterController.SelectModes.POINT;

        public ObservableCollection<LayerHolder> LayerList = new ObservableCollection<LayerHolder>();

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

        ListBox mLayerListView;

        public ListBox LayerListView
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
            initKeyMap();

            mPlotterView = plotterView;
            mPlotter = mPlotterView.Controller;
            SelectMode = mPlotter.SelectMode;
            FigureType = mPlotter.CreatingFigType;

            mPlotter.StateChanged = StateChanged;

            InteractIn.print = MessageOut;

            mPlotter.Interact = InteractIn;

            mPlotter.LayerListChanged =  LayerListChanged;
        }

        #region Maps
        private void initCommandMap()
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

        private void initKeyMap()
        {
            keyMap = new Dictionary<string, Action>
            {
                { "ctrl+z", undo },
                { "ctrl+y", redo },
                { "ctrl+c", Copy },
                { "ctrl+insert", Copy },
                { "ctrl+v", Paste },
                { "shift+insert", Paste },
                { "delete", remove },
                { "ctrl+s", save },
            };
        }
        #endregion

        // Handle events from PlotterController
        #region Event From PlotterController
        public void StateChanged(PlotterController sender, PlotterController.StateInfo si)
        {
            if (FigureType != si.CreatingFigureType)
            {
                FigureType = si.CreatingFigureType;
            }
        }

        public void LayerListChanged(PlotterController sender, PlotterController.LayerListInfo layerListInfo)
        {
            foreach (LayerHolder lh in LayerList)
            {
                lh.PropertyChanged -= LayerListItemPropertyChanged;
            }

            LayerList.Clear();

            foreach (CadLayer layer in layerListInfo.LayerList)
            {
                LayerHolder layerHolder = new LayerHolder(layer);
                layerHolder.PropertyChanged += LayerListItemPropertyChanged;

                LayerList.Add(layerHolder);
            }

            if (mLayerListView != null)
            {
                int idx = getLayerListIndex(layerListInfo.CurrentID);
                mLayerListView.SelectedIndex = idx;
            }
        }

        private int getLayerListIndex(uint id)
        {
            int idx = 0;
            foreach (LayerHolder layer in LayerList)
            {
                if (layer.ID == id)
                {
                    return idx;
                }
                idx++;
            }

            return -1;
        }
        #endregion


        // Layer list handling
        #region LayerList
        public void LayerListItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LayerHolder lh = (LayerHolder)sender;
            draw(clearFlag:true);
        }

        public void LayerListSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0)
            {
                LayerHolder layer = (LayerHolder)args.AddedItems[0];

                if (mPlotter.CurrentLayer.ID != layer.ID)
                {
                    mPlotter.setCurrentLayer(layer.ID);
                }
            }
            else
            {
            }
        }
        #endregion


        // Keyboard handling
        #region Keyboard handling
        public void perviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private string ModifyerKeysStr()
        {
            ModifierKeys modifierKeys = Keyboard.Modifiers;

            string s = "";

            if ((modifierKeys & ModifierKeys.Control) != ModifierKeys.None)
            {
                s += "ctrl+";
            }

            if ((modifierKeys & ModifierKeys.Shift) != ModifierKeys.None)
            {
                s += "shift+";
            }

            if ((modifierKeys & ModifierKeys.Alt) != ModifierKeys.None)
            {
                s += "alt+";
            }

            return s;
        }


        public void onKeyDown(object sender, KeyEventArgs e)
        {
        }

        public void onKeyUp(object sender, KeyEventArgs e)
        {
            string ks = ModifyerKeysStr();

            ks += e.Key.ToString().ToLower();

            if (!keyMap.ContainsKey(ks))
            {
                return;
            }

            Action action = keyMap[ks];

            action?.Invoke();
        }
        #endregion


        // Save / Load
        #region File
        private void SaveFile(String fname)
        {
            mPlotter.SaveToJsonFile(fname);
        }

        private void LoadFile(String fname)
        {
            mPlotter.LoadFromJsonFile(fname);

            DrawContext dc = mPlotterView.startDraw();

            mPlotter.clear(dc);

            mPlotter.draw(dc);

            mPlotterView.endDraw();
        }
        #endregion


        #region helper
        private DrawContext startDraw()
        {
            return mPlotterView.startDraw();
        }

        private void endDraw()
        {
            mPlotterView.endDraw();
        }

        private void draw(bool clearFlag)
        {
            DrawContext dc = mPlotterView.startDraw();
            if (clearFlag)
            {
                mPlotter.clear(dc);
            }
            mPlotter.draw(dc);
            mPlotterView.endDraw();
        }
        #endregion


        #region Actions
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
        #endregion


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
            MessageOut(s);
            mPlotter.command(s);
            draw(true);
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


        #region Others
        private void MessageOut(String s)
        {
            InteractOut.print(s);
        }
        #endregion
    }
}
