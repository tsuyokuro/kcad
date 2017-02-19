using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;


namespace Plotter
{
    public class SelectModeConverter : EnumBoolConverter<PlotterController.SelectModes> { }
    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }

    public enum ViewModes
    {
        NONE,
        XY,
        XZ,
        ZY,
        FREE,
    }

    public class ViewModeConverter : EnumBoolConverter<ViewModes> { }

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

        private PlotterController mController = new PlotterController();

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
                mController.SelectMode = mSelectMode;

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
                bool changed = UpdateFigureType(value);

                if (changed)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FigureType)));
                }
            }

            get
            {
                return mFigureType;
            }
        }


        private ViewModes mViewMode = ViewModes.NONE;

        public ViewModes ViewMode
        {
            set
            {
                bool changed = UpdateViewMode(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewMode)));
            }

            get
            {
                return mViewMode;
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
                    int idx = getLayerListIndex(mController.CurrentLayer.ID);
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
                return mController;
            }
        }



        private WindowsFormsHost mViewHost = null;


        private PlotterView plotterView1 = null;

        private PlotterViewGL plotterViewGL1 = null;


        private IPlotterView mPlotterView = null;

        public System.Windows.Forms.Control CurrentView
        {
            get
            {
                return mPlotterView.FromsControl;
            }
        } 

        public PlotterViewModel(WindowsFormsHost viewHost)
        {
            initCommandMap();
            initKeyMap();

            mViewHost = viewHost;

            SelectMode = mController.SelectMode;
            FigureType = mController.CreatingFigType;

            mController.StateChanged = StateChanged;

            InteractIn.print = MessageOut;

            mController.Interact = InteractIn;

            mController.LayerListChanged =  LayerListChanged;

            mController.DataChanged = DataChanged;

            mController.CursorPosChanged = CursorPosChanged;

            plotterView1 = new PlotterView();
            plotterViewGL1 = PlotterViewGL.Create();

            SetView(plotterView1);

            ViewMode = ViewModes.XY;
        }

        public void SetView(IPlotterView view)
        {
            if (mPlotterView == view)
            {
                return;
            }

            if (view == mPlotterView)
            {
                return;
            }

            if (mPlotterView != null)
            {
                mPlotterView.SetController(null);
            }

            mPlotterView = view;
            mPlotterView.SetController(mController);

            mController.CurrentDC = view.DrawContext;

            mViewHost.Child = view.FromsControl;
        }

        public void ViewFocus()
        {
            if (CurrentView != null)
            {
                CurrentView.Focus();
            }
        }

        #region Maps
        private void initCommandMap()
        {
            commandMap = new Dictionary<string, Action>{
                { "load", load },
                { "save",save },
                { "print",startPrint },
                { "undo",undo },
                { "redo",redo },
                { "copy",Copy },
                { "paste",Paste },
                { "separate",separateFigure },
                { "bond",bondFigure },
                { "to_bezier",toBezier },
                { "cut_segment",cutSegment },
                { "add_center_point", addCenterPoint },
                { "mirror_x", mirrorX },
                { "mirror_y", mirrorY },
                { "mirror_z", mirrorZ },
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

                if (mController.CurrentLayer.ID != layer.ID)
                {
                    mController.setCurrentLayer(layer.ID);
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
            Button btn = (Button)sender;
            switch (btn.Tag.ToString())
            {
                case "add_layer":
                    mController.addLayer(null);
                    break;

                case "remove_layer":
                    mController.removeLayer(mController.CurrentLayer.ID);
                    draw();
                    break;
            }
        }


        // Save / Load
        #region File
        private void SaveFile(String fname)
        {
            mController.SaveToJsonFile(fname);
        }

        private void LoadFile(String fname)
        {
            mController.LoadFromJsonFile(fname);

            DrawContext dc = mPlotterView.startDraw();

            mController.clear(dc);

            mController.draw(dc);

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
                mController.clear(dc);
            }
            mController.draw(dc);
            mPlotterView.endDraw();
        }
        #endregion


        #region Actions
        public void undo()
        {
            DrawContext dc = startDraw();
            mController.undo(dc);
            endDraw();
        }

        public void redo()
        {
            DrawContext dc = startDraw();
            mController.redo(dc);
            endDraw();
        }

        public void remove()
        {
            DrawContext dc = startDraw();
            mController.remove(dc);
            endDraw();
        }

        public void separateFigure()
        {
            DrawContext dc = startDraw();
            mController.separateFigures(dc);
            endDraw();
        }

        public void bondFigure()
        {
            DrawContext g = startDraw();
            mController.bondFigures(g);
            endDraw();
        }

        public void toBezier()
        {
            DrawContext dc = startDraw();
            mController.toBezier(dc);
            endDraw();
        }

        public void cutSegment()
        {
            DrawContext dc = startDraw();
            mController.cutSegment(dc);
            endDraw();
        }

        public void addCenterPoint()
        {
            DrawContext dc = startDraw();
            mController.addCenterPoint(dc);
            endDraw();
        }

        public void mirrorX()
        {
            DrawContext dc = startDraw();
            mController.mirrorX(dc);
            endDraw();
        }

        public void mirrorY()
        {
            DrawContext dc = startDraw();
            mController.mirrorY(dc);
            endDraw();
        }

        public void mirrorZ()
        {
            DrawContext dc = startDraw();
            mController.mirrorZ(dc);
            endDraw();
        }

        public void Copy()
        {
            DrawContext dc = startDraw();
            mController.Copy(dc);
            endDraw();
        }

        public void Paste()
        {
            DrawContext dc = startDraw();
            mController.Paste(dc);
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
            DrawContextGDI dc = new DrawContextGDI();

            dc.graphics = g;
            dc.SetupTools(DrawTools.ToolsType.PRINTER);
            dc.PageSize = mPlotterView.PageSize;

            // Default printers's unit is 1/100 inch
            dc.SetUnitPerInch(100.0);

            dc.ViewMatrix = mController.CurrentDC.ViewMatrix;
            dc.ProjectionMatrix = mController.CurrentDC.ProjectionMatrix;

            if (mController.CurrentDC is DrawContextGL)
            {
                dc.WoldScale = 0.2;
                dc.UnitPerMilli = 1.0;
                dc.DeviceScaleX = mController.CurrentDC.ViewWidth / 2.0;
                dc.DeviceScaleY = -mController.CurrentDC.ViewHeight / 2.0;
            }

            CadPoint org = default(CadPoint);

            org.x = dc.PageSize.widthInch / 2.0 * 100;
            org.y = dc.PageSize.heightInch / 2.0 * 100;

            dc.ViewOrg = org;

            mController.print(dc);
        }
        #endregion


        #region Command handling
        public void textCommand(string s)
        {
            MessageOut(s);
            mController.command(s);
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
            mController.debugCommand(dc, s);
            mPlotterView.endDraw();
        }
        #endregion


        #region Others
        private void MessageOut(String s)
        {
            InteractOut.print(s);
        }
        #endregion

        private bool UpdateFigureType(CadFigure.Types newType)
        {
            var prev = mFigureType;

            if (mFigureType == newType)
            {
                // 現在のタイプを再度選択したら解除する
                mFigureType = CadFigure.Types.NONE;
            }
            else
            {
                mFigureType = newType;
            }

            if (mFigureType != CadFigure.Types.NONE)
            {
                DrawContext dc = mPlotterView.startDraw();
                mController.startCreateFigure(mFigureType, dc);
                mPlotterView.endDraw();
            }
            else if (prev != CadFigure.Types.NONE)
            {
                DrawContext dc = mPlotterView.startDraw();
                mController.endCreateFigure(dc);
                mController.draw(dc);
                mPlotterView.endDraw();
            }

            return true;
        }

        private bool UpdateViewMode(ViewModes newMode)
        {
            if (mViewMode == newMode)
            {
                return false;
            }

            mViewMode = newMode;

            switch (mViewMode)
            {
                case ViewModes.XY:
                    SetView(plotterView1);

                    mPlotterView.DrawContext.ViewMatrix = UMatrixs.ViewXY;
                    mPlotterView.DrawContext.ViewMatrixInv = UMatrixs.ViewXYInv;
                    draw();
                    break;

                case ViewModes.XZ:
                    SetView(plotterView1);

                    mPlotterView.DrawContext.ViewMatrix = UMatrixs.ViewXZ;
                    mPlotterView.DrawContext.ViewMatrixInv = UMatrixs.ViewXZInv;
                    draw();
                    break;

                case ViewModes.ZY:
                    SetView(plotterView1);

                    mPlotterView.DrawContext.ViewMatrix = UMatrixs.ViewZY;
                    mPlotterView.DrawContext.ViewMatrixInv = UMatrixs.ViewZYInv;
                    draw();
                    break;

                case ViewModes.FREE:
                    SetView(plotterViewGL1);

                    draw();
                    break;
            }

            return true;
        }
    }
}
