using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Drawing;
using KCad;
using System.Drawing.Printing;

namespace Plotter
{
    public class SelectModeConverter : EnumBoolConverter<PlotterController.SelectModes> { }
    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }

    public enum ViewModes
    {
        NONE,
        FRONT,
        BACK,
        TOP,
        BOTTOM,
        RIGHT,
        LEFT,
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

        PlotterController.Interaction mInteractOut = new PlotterController.Interaction();

        public PlotterController.Interaction InteractOut
        {
            set
            {
                mInteractOut = value;
                mController.InteractOut = mInteractOut;
            }
        }

        private Dictionary<string, Action> CommandMap;

        private Dictionary<string, Action> KeyMap;

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
                bool changed = ChangeViewMode(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewMode)));
            }

            get
            {
                return mViewMode;
            }
        }

        public bool SnapToGrid
        {
            set
            {
                mController.SnapToGrid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));
            }

            get
            {
                return mController.SnapToGrid;
            }
        }

        public bool SnapToFigure
        {
            set
            {
                mController.SnapToFigure = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToFigure)));
            }

            get
            {
                return mController.SnapToFigure;
            }
        }

        public bool SnapToLine
        {
            set
            {
                mController.SnapToLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            }

            get
            {
                return mController.SnapToLine;
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
                    int idx = GetLayerListIndex(mController.CurrentLayer.ID);
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

        private Window mMainWindow;

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

        public PlotterViewModel(Window mainWindow, WindowsFormsHost viewHost)
        {
            mMainWindow = mainWindow;

            InitCommandMap();
            InitKeyMap();

            mViewHost = viewHost;

            SelectMode = mController.SelectMode;
            FigureType = mController.CreatingFigType;

            mController.StateChanged = StateChanged;

            mController.LayerListChanged =  LayerListChanged;

            mController.DataChanged = DataChanged;

            mController.CursorPosChanged = CursorPosChanged;

            plotterView1 = new PlotterView();
            plotterViewGL1 = PlotterViewGL.Create();

            SetView(plotterView1);

            ViewMode = ViewModes.FRONT;
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
        private void InitCommandMap()
        {
            CommandMap = new Dictionary<string, Action>{
                { "load", Load },
                { "save",Save },
                { "print",StartPrint },
                { "undo",undo },
                { "redo",redo },
                { "copy",Copy },
                { "paste",Paste },
                { "separate",separateFigure },
                { "bond",bondFigure },
                { "to_bezier",toBezier },
                { "cut_segment",cutSegment },
                { "add_center_point", addCenterPoint },
                { "to_loop", ToLoop },
                { "to_unloop", ToUnloop },
                { "clear_layer", ClearLayer },
                { "flip_x", FlipX },
                { "flip_y", FlipY },
                { "flip_z", FlipZ },
                { "flip_normal", FlipNormal },
                { "grid_settings", GridSettings },
                { "add_layer", AddLayer },
                { "remove_layer", RemoveLayer },
                { "centroid", GetCentroid },
            };
        }

        private void InitKeyMap()
        {
            KeyMap = new Dictionary<string, Action>
            {
                { "ctrl+z", undo },
                { "ctrl+y", redo },
                { "ctrl+c", Copy },
                { "ctrl+insert", Copy },
                { "ctrl+v", Paste },
                { "shift+insert", Paste },
                { "delete", remove },
                { "ctrl+s", Save },
            };
        }

        public void ExecCommand(string cmd)
        {
            if (!CommandMap.ContainsKey(cmd))
            {
                return;
            }

            Action action = CommandMap[cmd];

            action?.Invoke();
        }

        public void ExecShortcutKey(string keyCmd)
        {
            if (!KeyMap.ContainsKey(keyCmd))
            {
                return;
            }

            Action action = KeyMap[keyCmd];

            action?.Invoke();
        }

        #endregion


        // Actions
        #region Actions
        public void undo()
        {
            DrawContext dc = StartDraw();
            mController.undo(dc);
            EndDraw();
        }

        public void redo()
        {
            DrawContext dc = StartDraw();
            mController.redo(dc);
            EndDraw();
        }

        public void remove()
        {
            DrawContext dc = StartDraw();
            mController.remove(dc);
            EndDraw();
        }

        public void separateFigure()
        {
            DrawContext dc = StartDraw();
            mController.separateFigures(dc);
            EndDraw();
        }

        public void bondFigure()
        {
            DrawContext g = StartDraw();
            mController.bondFigures(g);
            EndDraw();
        }

        public void toBezier()
        {
            DrawContext dc = StartDraw();
            mController.toBezier(dc);
            EndDraw();
        }

        public void cutSegment()
        {
            DrawContext dc = StartDraw();
            mController.cutSegment(dc);
            EndDraw();
        }

        public void addCenterPoint()
        {
            DrawContext dc = StartDraw();
            mController.addCenterPoint(dc);
            EndDraw();
        }

        public void ToLoop()
        {
            DrawContext dc = StartDraw();
            mController.SetLoop(dc, true);
            EndDraw();
        }

        public void ToUnloop()
        {
            DrawContext dc = StartDraw();
            mController.SetLoop(dc, false);
            EndDraw();
        }

        public void FlipX()
        {
            DrawContext dc = StartDraw();
            mController.FlipX(dc);
            EndDraw();
        }

        public void FlipY()
        {
            DrawContext dc = StartDraw();
            mController.FlipY(dc);
            EndDraw();
        }

        public void FlipZ()
        {
            DrawContext dc = StartDraw();
            mController.FlipZ(dc);
            EndDraw();
        }

        public void FlipNormal()
        {
            DrawContext dc = StartDraw();
            mController.FlipNormal(dc);
            EndDraw();
        }

        public void ClearLayer()
        {
            DrawContext dc = StartDraw();
            mController.ClearLayer(dc, 0);
            EndDraw();
        }

        public void Copy()
        {
            DrawContext dc = StartDraw();
            mController.Copy(dc);
            EndDraw();
        }

        public void Paste()
        {
            DrawContext dc = StartDraw();
            mController.Paste(dc);
            EndDraw();
        }

        public void Load()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFile(ofd.FileName);
            }
        }

        public void Save()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(sfd.FileName);
            }
        }

        public void GridSettings()
        {
            GridSettingsDialog dlg = new GridSettingsDialog();

            dlg.GridSize = mController.Grid.GridSize;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                mController.Grid.GridSize = dlg.GridSize;

                DrawContext dc = StartDraw();
                mController.DrawAll(dc);
                EndDraw();
            }
        }

        public void AddLayer()
        {
            mController.AddLayer(null);
        }

        public void RemoveLayer()
        {
            mController.RemoveLayer(mController.CurrentLayer.ID);
            Draw();
        }

        public void GetCentroid()
        {
            DrawContext dc = StartDraw();
            mController.GetCentroid(dc);
            EndDraw();
        }

        #endregion



        // Handle events from PlotterController
        #region Event From PlotterController

        public void DataChanged(PlotterController sender, bool redraw)
        {
            if (redraw)
            {
                DrawAll();
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
                int idx = GetLayerListIndex(layerListInfo.CurrentID);
                mLayerListView.SelectedIndex = idx;
            }
        }

        private int GetLayerListIndex(uint id)
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
            Draw(clearFlag:true);
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

            ExecShortcutKey(ks);
        }
        #endregion


        // Menu handler
        public void MenuItemClicked(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = (MenuItem)sender;

            string cmd = menuitem.Tag.ToString();

            ExecCommand(cmd);
        }

        // Button handler
        public void ButtonClicked(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string cmd = btn.Tag.ToString();

            ExecCommand(cmd);
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

            DrawContext dc = mPlotterView.StartDraw();

            mController.Clear(dc);

            mController.Draw(dc);

            mPlotterView.EndDraw();
        }
        #endregion


        #region helper
        private DrawContext StartDraw()
        {
            return mPlotterView.StartDraw();
        }

        private void EndDraw()
        {
            mPlotterView.EndDraw();
        }

        private void Draw(bool clearFlag=true)
        {
            DrawContext dc = mPlotterView.StartDraw();
            if (clearFlag)
            {
                mController.Clear(dc);
            }
            mController.Draw(dc);
            mPlotterView.EndDraw();
        }

        private void DrawAll()
        {
            DrawContext dc = mPlotterView.StartDraw();
            mController.Clear(dc);
            mController.DrawAll(dc);
            mPlotterView.EndDraw();
        }
        #endregion

        #region "print"
        public void StartPrint()
        {
            PrintDocument pd =
                new PrintDocument();

            PageSettings storePageSettings = pd.DefaultPageSettings;

            pd.DefaultPageSettings.Landscape = mPlotterView.DrawContext.PageSize.IsLandscape();

            pd.PrintPage += PrintPage;

            System.Windows.Forms.PrintDialog pdlg = new System.Windows.Forms.PrintDialog();

            pdlg.Document = pd;

            if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pd.Print();
            }

            pd.DefaultPageSettings = storePageSettings;
        }

        private void PrintPage(object sender,
            System.Drawing.Printing.PrintPageEventArgs e)
        {
            DrawPage(e.Graphics);
        }

        private void DrawPage(System.Drawing.Graphics g)
        {
            DrawContextGDI dc = new DrawContextGDI();

            dc.graphics = g;
            dc.SetupTools(DrawTools.ToolsType.PRINTER);
            dc.PageSize = mPlotterView.PageSize;

            // Default printers's unit is 1/100 inch
            dc.SetUnitPerInch(100.0);

            dc.SetMatrix(
                mController.CurrentDC.ViewMatrix,
                mController.CurrentDC.ProjectionMatrix
                );

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

            mController.Print(dc);
        }
        #endregion


        #region Command handling
        public void TextCommand(string s)
        {
            MessageOut(s);
            mController.ScriptEnv.command(s);
            Draw(true);
        }

        //public void MenuCommand(string tag)
        //{
        //    ExecCommand(tag);
        //}

        public void DebugCommand(string s)
        {
            DrawContext dc = mPlotterView.StartDraw();
            mController.debugCommand(dc, s);
            mPlotterView.EndDraw();
        }
        #endregion


        private void MessageOut(String s)
        {
            mInteractOut.print(s);
        }

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
                DrawContext dc = mPlotterView.StartDraw();
                mController.startCreateFigure(mFigureType, dc);
                mPlotterView.EndDraw();
            }
            else if (prev != CadFigure.Types.NONE)
            {
                DrawContext dc = mPlotterView.StartDraw();
                mController.endCreateFigure(dc);
                mController.Draw(dc);
                mPlotterView.EndDraw();
            }

            return true;
        }

        private bool ChangeViewMode(ViewModes newMode)
        {
            if (mViewMode == newMode)
            {
                return false;
            }

            mViewMode = newMode;

            switch (mViewMode)
            {
                case ViewModes.FRONT:
                    SetView(plotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.Zero, -Vector3d.UnitZ, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.BACK:
                    SetView(plotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.Zero, Vector3d.UnitZ, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.TOP:
                    SetView(plotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.Zero, -Vector3d.UnitY, -Vector3d.UnitZ);
                    DrawAll();
                    break;

                case ViewModes.BOTTOM:
                    SetView(plotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.Zero, Vector3d.UnitY, Vector3d.UnitZ);
                    DrawAll();
                    break;

                case ViewModes.RIGHT:
                    SetView(plotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.Zero, -Vector3d.UnitX, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.LEFT:
                    SetView(plotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.Zero, Vector3d.UnitX, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.FREE:
                    {
                        plotterViewGL1.Size = plotterView1.Size;
                        plotterViewGL1.DrawContext.SetUnitPerMilli(plotterView1.DrawContext.UnitPerMilli);
                        SetView(plotterViewGL1);
                        DrawAll();
                    }
                    break;
            }

            return true;
        }

        public void SetupTextCommandView(AutoCompleteBox textBox)
        {
            textBox.ItemsSource = new List<string>()
            {
                "rect(",
                "distance",
                "revOrder",
                "group",
                "ungroup",
                "addLayer",
            };

            textBox.ItemFilter = ScriptFilter;
        }

        public AutoCompleteFilterPredicate<object> ScriptFilter
        {
            get { return (str, obj) => (obj as string).Contains(str); }
        }
    }
}
