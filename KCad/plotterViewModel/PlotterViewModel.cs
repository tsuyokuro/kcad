using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Plotter
{
    public class SelectModeConverter : EnumBoolConverter<PlotterController.SelectModes> { }
    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }

    public class FreqChangedInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string mStrCursorPos;

        public string StrCursorPos
        {
            set
            {
                mStrCursorPos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos)));
            }

            get
            {
                return mStrCursorPos;
            }
        }

        private CadPoint mCursorPos;

        public CadPoint CursorPos
        {
            set
            {
                mCursorPos = value;

                String s = string.Format("({0:0.000},{1:0.000},{2:0.000})",
                    mCursorPos.x, mCursorPos.y, mCursorPos.z);

                StrCursorPos = s;
            }

            get
            {
                return mCursorPos;
            }
        }
    }

    public class PlotterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PlotterController mPlotterController = new PlotterController();

        private IPlotterView mPlotterView = null;

        public PlotterController.Interaction InteractOut =
            new PlotterController.Interaction();

        public PlotterController.Interaction InteractIn =
            new PlotterController.Interaction();

        private Dictionary<string, Action> commandMap;

        private Dictionary<string, Action> keyMap;

        private PlotterController.SelectModes mSelectMode = PlotterController.SelectModes.POINT;

        public ObservableCollection<LayerHolder> LayerList = new ObservableCollection<LayerHolder>();

        public FreqChangedInfo FreqChangedInfo = new FreqChangedInfo();

        public PlotterController.SelectModes SelectMode
        {
            set
            {
                mSelectMode = value;
                mPlotterController.SelectMode = mSelectMode;

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
                    mPlotterController.startCreateFigure(mFigureType);
                }
                else if (prev != CadFigure.Types.NONE)
                {
                    mPlotterController.endCreateFigure();

                    DrawContext dc = mPlotterView.startDraw();
                    mPlotterController.draw(dc);
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
                    int idx = getLayerListIndex(mPlotterController.CurrentLayer.ID);
                    value.SelectedIndex = idx;
                }

                mLayerListView = value;
            }

            get
            {
                return mLayerListView;
            }
        }

        public PlotterController Controller
        {
            get
            {
                return mPlotterController;
            }
        }

        public PlotterViewModel()
        {
            initCommandMap();
            initKeyMap();

            SelectMode = mPlotterController.SelectMode;
            FigureType = mPlotterController.CreatingFigType;

            mPlotterController.StateChanged = StateChanged;

            InteractIn.print = MessageOut;

            mPlotterController.Interact = InteractIn;

            mPlotterController.LayerListChanged =  LayerListChanged;

            mPlotterController.DataChanged = DataChanged;

            mPlotterController.CursorPosChanged = CursorPosChanged;
        }

        public void SetView(IPlotterView view)
        {
            if (view == mPlotterView)
            {
                return;
            }

            if (mPlotterView != null)
            {
                mPlotterView.SetController(null);
            }

            mPlotterView = view;
            mPlotterView.SetController(mPlotterController);
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

        public void DataChanged(PlotterController sender, bool redraw)
        {
            if (redraw)
            {
                draw(true);
            }
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

        private void CursorPosChanged(PlotterController sender, CadPoint pt)
        {
            FreqChangedInfo.CursorPos = pt;
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

                if (mPlotterController.CurrentLayer.ID != layer.ID)
                {
                    mPlotterController.setCurrentLayer(layer.ID);
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


        // Button handler
        public void ButtonClicked(object sender, RoutedEventArgs e)
        {
            // TODO implements function for add layer and remove layer.
            Button btn = (Button)sender;
            switch (btn.Tag.ToString())
            {
                case "add_layer":
                    mPlotterController.addLayer(null);
                    break;

                case "remove_layer":
                    mPlotterController.removeLayer(mPlotterController.CurrentLayer.ID);
                    draw();
                    break;

                case "axis_xy":
                    mPlotterView.DrawContext.ViewMatrix = UMatrixs.ViewXY;
                    mPlotterView.DrawContext.ViewMatrixInv = UMatrixs.ViewXYInv;
                    mPlotterView.DrawContext.Perspective = false;
                    draw();
                    break;

                case "axis_xz":
                    mPlotterView.DrawContext.ViewMatrix = UMatrixs.ViewXZ;
                    mPlotterView.DrawContext.ViewMatrixInv = UMatrixs.ViewXZInv;
                    mPlotterView.DrawContext.Perspective = false;
                    draw();
                    break;

                case "axis_zy":
                    mPlotterView.DrawContext.ViewMatrix = UMatrixs.ViewZY;
                    mPlotterView.DrawContext.ViewMatrixInv = UMatrixs.ViewZYInv;
                    mPlotterView.DrawContext.Perspective = false;
                    draw();
                    break;

                case "axis_xyz":
                    break;
            }
        }


        // Save / Load
        #region File
        private void SaveFile(String fname)
        {
            mPlotterController.SaveToJsonFile(fname);
        }

        private void LoadFile(String fname)
        {
            mPlotterController.LoadFromJsonFile(fname);

            DrawContext dc = mPlotterView.startDraw();

            mPlotterController.clear(dc);

            mPlotterController.draw(dc);

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

        private void draw(bool clearFlag=true)
        {
            DrawContext dc = mPlotterView.startDraw();
            if (clearFlag)
            {
                mPlotterController.clear(dc);
            }
            mPlotterController.draw(dc);
            mPlotterView.endDraw();
        }
        #endregion


        #region Actions
        public void undo()
        {
            DrawContext dc = startDraw();
            mPlotterController.undo(dc);
            endDraw();
        }

        public void redo()
        {
            DrawContext dc = startDraw();
            mPlotterController.redo(dc);
            endDraw();
        }

        public void remove()
        {
            DrawContext dc = startDraw();
            mPlotterController.remove(dc);
            endDraw();
        }

        public void separateFigure()
        {
            DrawContext dc = startDraw();
            mPlotterController.separateFigures(dc);
            endDraw();
        }

        public void bondFigure()
        {
            DrawContext g = startDraw();
            mPlotterController.bondFigures(g);
            endDraw();
        }

        public void toBezier()
        {
            DrawContext dc = startDraw();
            mPlotterController.toBezier(dc);
            endDraw();
        }

        public void cutSegment()
        {
            DrawContext dc = startDraw();
            mPlotterController.cutSegment(dc);
            endDraw();
        }

        public void addCenterPoint()
        {
            DrawContext dc = startDraw();
            mPlotterController.addCenterPoint(dc);
            endDraw();
        }

        public void Copy()
        {
            DrawContext dc = startDraw();
            mPlotterController.Copy(dc);
            endDraw();
        }

        public void Paste()
        {
            DrawContext dc = startDraw();
            mPlotterController.Paste(dc);
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
            DrawContextWin dc = new DrawContextWin();

            dc.graphics = g;
            dc.setupTools(DrawTools.ToolsType.PRINTER);
            dc.PageSize = mPlotterView.PageSize;

            // Default printers's unit is 1/100 inch
            dc.setUnitPerInch(100.0);

            CadPoint org = default(CadPoint);

            org.x = dc.PageSize.widthInch / 2.0 * 100;
            org.y = dc.PageSize.heightInch / 2.0 * 100;

            dc.ViewOrg = org;

            mPlotterController.print(dc);
        }
        #endregion


        #region Command handling
        public void textCommand(string s)
        {
            MessageOut(s);
            mPlotterController.command(s);
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
            mPlotterController.debugCommand(dc, s);
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
