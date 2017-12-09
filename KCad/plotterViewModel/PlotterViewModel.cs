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
using System.Collections;

namespace Plotter
{
    public class SelectModeConverter : EnumBoolConverter<PlotterController.SelectModes> { }
    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
    public class MeasureModeConverter : EnumBoolConverter<PlotterController.MeasureModes> { }

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

        private string mStrCursorPos = "";

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

        private string mStrCursorPos2 = "";

        public string StrCursorPos2
        {
            set
            {
                mStrCursorPos2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos2)));
            }

            get
            {
                return mStrCursorPos2;
            }
        }

        private CadVector mCursorPos;

        public CadVector CursorPos
        {
            set
            {
                if (!String.IsNullOrEmpty(mStrCursorPos) && mCursorPos.VectorEquals(value))
                {
                    return;
                }

                mCursorPos = value;

                String s = string.Format("({0:0.00},{1:0.00},{2:0.00})",
                    mCursorPos.x, mCursorPos.y, mCursorPos.z);

                StrCursorPos = s;
            }

            get
            {
                return mCursorPos;
            }
        }

        private CadVector mCursorPos2;

        public CadVector CursorPos2
        {
            set
            {
                if (!String.IsNullOrEmpty(mStrCursorPos2) && mCursorPos2.VectorEquals(value))
                {
                    return;
                }

                mCursorPos2 = value;

                String s = string.Format("({0:0.00},{1:0.00},{2:0.00})",
                    mCursorPos2.x, mCursorPos2.y, mCursorPos2.z);

                StrCursorPos2 = s;
            }

            get
            {
                return mCursorPos2;
            }
        }
    }

    public class PlotterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PlotterController mController = new PlotterController();

        public PlotterController.Interaction InteractOut
        {
            set
            {
                mController.InteractOut = value;
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

        private PlotterController.MeasureModes mMeasureMode = PlotterController.MeasureModes.NONE;

        public PlotterController.MeasureModes MeasureMode
        {
            set
            {
                bool changed = UpdateMeasuerType(value);

                if (changed)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasureMode)));
                }
            }

            get
            {
                return mMeasureMode;
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

        #region Snap settings
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

        public bool SnapToPoint
        {
            set
            {
                mController.SnapToPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            }

            get
            {
                return mController.SnapToPoint;
            }
        }

        public bool SnapToSegment
        {
            set
            {
                mController.SnapToSegment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            }

            get
            {
                return mController.SnapToSegment;
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
        #endregion

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


        private PlotterView PlotterView1 = null;

        private PlotterViewGL PlotterViewGL1 = null;


        private IPlotterView mPlotterView = null;

        public System.Windows.Forms.Control CurrentView
        {
            get
            {
                return mPlotterView.FromsControl;
            }
        }

        public TextCommandHistory CommandHistory = new TextCommandHistory();

        public PlotterViewModel(Window mainWindow, WindowsFormsHost viewHost)
        {
            mMainWindow = mainWindow;
            mViewHost = viewHost;

            InitCommandMap();
            InitKeyMap();

            SelectMode = mController.SelectMode;
            FigureType = mController.CreatingFigType;

            mController.StateChanged = StateChanged;

            mController.LayerListChanged =  LayerListChanged;

            mController.DataChanged = DataChanged;

            mController.CursorPosChanged = CursorPosChanged;

            PlotterView1 = new PlotterView();
            PlotterViewGL1 = PlotterViewGL.Create();

            //SetView(PlotterView1);

            ViewMode = ViewModes.FREE;  // 一旦GL側を設定してViewをLoadしておく
            ViewMode = ViewModes.FRONT;
        }

        private void SetView(IPlotterView view)
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

        CadObjectTreeView mCadObjectTreeView;

        public void SetObjectTreeView(CadObjectTreeView treeView)
        {
            mCadObjectTreeView = treeView;
            mController.SetObjectTreeView(treeView);

            if (mCadObjectTreeView != null)
            {
                mCadObjectTreeView.CheckChanged += ObjectTreeView_CheckChanged;
            }
        }

        private void ObjectTreeView_CheckChanged(object sender, EventArgs e)
        {
            DrawAll();
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
                { "undo",Undo },
                { "redo",Redo },
                { "copy",Copy },
                { "paste",Paste },
                { "separate",SeparateFigure },
                { "bond",BondFigure },
                { "to_bezier",ToBezier },
                { "cut_segment",CutSegment },
                { "ins_point",InsPoint },
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
                { "centroid", AddCentroid },
                { "select_all", SelectAll },
                { "snap_settings", SnapSettings },
            };
        }

        private void InitKeyMap()
        {
            KeyMap = new Dictionary<string, Action>
            {
                { "ctrl+z", Undo },
                { "ctrl+y", Redo },
                { "ctrl+c", Copy },
                { "ctrl+insert", Copy },
                { "ctrl+v", Paste },
                { "shift+insert", Paste },
                { "delete", Remove },
                { "ctrl+s", Save },
                { "ctrl+a", SelectAll },
                { "escape", Cancel },
                { "ctrl+p", InsPoint },
                //{ "ctrl+oemplus", SearchNearestPoint },
                { "f2", SearchNearestPoint },
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

        public bool ExecShortcutKey(string keyCmd)
        {
            if (!KeyMap.ContainsKey(keyCmd))
            {
                return false;
            }

            Action action = KeyMap[keyCmd];

            action?.Invoke();

            return true;
        }

        #endregion


        // Actions
        #region Actions
        public void Undo()
        {
            mController.Undo();
            RedrawAll();
        }

        public void Redo()
        {
            mController.Redo();
            RedrawAll();
        }

        public void Remove()
        {
            mController.Remove();
            RedrawAll();
        }

        public void SeparateFigure()
        {
            mController.SeparateFigures();
            RedrawAll();
        }

        public void BondFigure()
        {
            mController.BondFigures();
            RedrawAll();
        }

        public void ToBezier()
        {
            mController.ToBezier();
            RedrawAll();
        }

        public void CutSegment()
        {
            mController.CutSegment();
            RedrawAll();
        }

        public void InsPoint()
        {
            mController.InsPointToLastSelectedSeg();
            RedrawAll();
        }

        public void ToLoop()
        {
            mController.SetLoop(true);
            RedrawAll();
        }

        public void ToUnloop()
        {
            mController.SetLoop(false);
            RedrawAll();
        }

        public void FlipX()
        {
            mController.FlipX();
            RedrawAll();
        }

        public void FlipY()
        {
            mController.FlipY();
            RedrawAll();
        }

        public void FlipZ()
        {
            mController.FlipZ();
            RedrawAll();
        }

        public void FlipNormal()
        {
            mController.FlipNormal();
            RedrawAll();
        }

        public void ClearLayer()
        {
            mController.ClearLayer(0);
            RedrawAll();
        }

        public void Copy()
        {
            mController.Copy();
            RedrawAll();
        }

        public void Paste()
        {
            mController.Paste();
            RedrawAll();
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

            dlg.Owner = mMainWindow;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                mController.Grid.GridSize = dlg.GridSize;

                RedrawAll();
            }
        }

        public void SnapSettings()
        {
            SnapSettingsDialog dlg = new SnapSettingsDialog();

            dlg.Owner = mMainWindow;

            dlg.PointSnapRange = mController.PointSnapRange;
            dlg.LineSnapRange = mController.LineSnapRange;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                mController.PointSnapRange = dlg.PointSnapRange;
                mController.LineSnapRange = dlg.LineSnapRange;

                RedrawAll();
            }
        }

        public void RedrawAll()
        {
            DrawContext dc = StartDraw();
            mController.Clear(dc);
            mController.DrawAll(dc);
            EndDraw();
        }

        public void AddLayer()
        {
            mController.AddLayer(null);
            RedrawAll();
        }

        public void RemoveLayer()
        {
            mController.RemoveLayer(mController.CurrentLayer.ID);
            RedrawAll();
        }

        public void AddCentroid()
        {
            mController.AddCentroid();
            RedrawAll();
        }

        public void SelectAll()
        {
            mController.SelectAllInCurrentLayer();
            RedrawAll();
        }

        public void Cancel()
        {
            mController.Cancel();
            RedrawAll();
        }

        public void SearchNearestPoint()
        {
            mController.MoveCursorNearestPoint(mPlotterView.DrawContext);
            RedrawAll();
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

            if (MeasureMode != si.MeasureMode)
            {
                MeasureMode = si.MeasureMode;
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

        private void CursorPosChanged(PlotterController sender, CadVector pt, CursorType type)
        {
            if (type == CursorType.TRACKING)
            {
                FreqChangedInfo.CursorPos = pt;
            }
            else if (type == CursorType.LAST_DOWN)
            {
                FreqChangedInfo.CursorPos2 = pt;
            }
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

                    RedrawAll();
                }
            }
            else
            {
            }
        }
        #endregion


        // Keyboard handling
        #region Keyboard handling
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

        private string KeyString(KeyEventArgs e)
        {
            string ks = ModifyerKeysStr();

            ks += e.Key.ToString().ToLower();

            return ks;
        }


        public bool OnKeyDown(object sender, KeyEventArgs e)
        {
            string ks = KeyString(e);
            return KeyMap.ContainsKey(ks);
        }

        public bool OnKeyUp(object sender, KeyEventArgs e)
        {
            string ks = KeyString(e);
            return ExecShortcutKey(ks);
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
            RedrawAll();
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
            DrawContextPrinter dc = new DrawContextPrinter(mController.CurrentDC, g, mPlotterView.PageSize);
            mController.Print(dc);
        }
        #endregion


        #region Command handling
        public void TextCommand(string s)
        {
            mController.ScriptEnv.command(s);
            CommandHistory.Add(s);
            //DrawAll();
        }

        public void DebugCommand(string s)
        {
            DrawContext dc = StartDraw();

            mController.Clear(dc);

            mController.debugCommand(dc, s);

            mController.DrawAll(dc);
            EndDraw();
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
                MeasureMode = PlotterController.MeasureModes.NONE;
                mController.StartCreateFigure(mFigureType);

                RedrawAll();
            }
            else if (prev != CadFigure.Types.NONE)
            {
                mController.EndCreateFigure();

                RedrawAll();
            }

            return true;
        }

        private bool UpdateMeasuerType(PlotterController.MeasureModes newType)
        {
            var prev = mMeasureMode;

            if (mMeasureMode == newType)
            {
                // 現在のタイプを再度選択したら解除する
                mMeasureMode = PlotterController.MeasureModes.NONE;
            }
            else
            {
                mMeasureMode = newType;
            }

            if (mMeasureMode != PlotterController.MeasureModes.NONE)
            {
                FigureType = CadFigure.Types.NONE;
                mController.StartMeasure(newType);
            }
            else if (prev != PlotterController.MeasureModes.NONE)
            {
                mController.EndMeasure();
                RedrawAll();
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
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.UnitZ, Vector3d.Zero, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.BACK:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(-Vector3d.UnitZ, Vector3d.Zero, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.TOP:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.UnitY, Vector3d.Zero, -Vector3d.UnitZ);
                    DrawAll();
                    break;

                case ViewModes.BOTTOM:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(-Vector3d.UnitY, Vector3d.Zero, Vector3d.UnitZ);
                    DrawAll();
                    break;

                case ViewModes.RIGHT:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.UnitX, Vector3d.Zero, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.LEFT:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(-Vector3d.UnitX, Vector3d.Zero, Vector3d.UnitY);
                    DrawAll();
                    break;

                case ViewModes.FREE:
                    PlotterViewGL1.Size = PlotterView1.Size;
                    PlotterViewGL1.DrawContext.SetUnitPerMilli(PlotterView1.DrawContext.UnitPerMilli);
                    SetView(PlotterViewGL1);
                    DrawAll();
                    break;
            }

            return true;
        }

        public void SetupTextCommandView(AutoCompleteTextBox textBox)
        {
            textBox.CandidateList = Controller.ScriptEnv.AutoCompleteList;
        }

        public AutoCompleteFilterPredicate<object> ScriptFilter
        {
            get { return (str, obj) => (obj as string).Contains(str); }
        }

        public void LoadSettings()
        {
            PlotterSettings settings = new PlotterSettings();

            settings.Load();

            SnapToPoint = settings.SnapToPoint;
            SnapToSegment = settings.SnapToSegment;
            SnapToLine = settings.SnapToLine;
            SnapToGrid = settings.SnapToGrid;

            mController.Grid.GridSize = settings.GridSize;

            mController.PointSnapRange = settings.PointSnapRange;

            mController.LineSnapRange = settings.LineSnapRange;
        }

        public void SaveSettings()
        {
            PlotterSettings settings = new PlotterSettings();

            settings.SnapToPoint = SnapToPoint;
            settings.SnapToSegment = SnapToSegment;
            settings.SnapToLine = SnapToLine;
            settings.SnapToGrid = SnapToGrid;

            settings.GridSize = mController.Grid.GridSize;

            settings.PointSnapRange = mController.PointSnapRange;

            settings.LineSnapRange = mController.LineSnapRange;

            settings.Save();
        }

        public void MessageSelected(List<string> messages)
        {
            mController.ScriptEnv.MessageSelected(messages);

            DrawAll();
        }
    }

    public class TextCommandHistory
    {
        private List<string> History = new List<string>();
        int Pos = 0;

        private string empty = "";

        public void Add(string s)
        {
            History.Add(s);
            Pos = History.Count;
        }

        public string Rewind()
        {
            Pos--;

            if (Pos<0)
            {
                Pos = 0;
                return empty;
            }

            return History[Pos];
        }


        public string Forward()
        {
            Pos++;

            if (Pos >= History.Count)
            {
                Pos = History.Count;
                return empty;
            }

            return History[Pos];
        }
    }
}
