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
using static System.Drawing.Printing.PrinterSettings;
using CadDataTypes;
using Plotter.Controller;
using KCad.Dialogs;
using System.IO;
using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System.Threading.Tasks;
using MessagePack;

namespace Plotter
{
    public class SelectModeConverter : EnumBoolConverter<SelectModes> { }
    public class FigureTypeConverter : EnumBoolConverter<CadFigure.Types> { }
    public class MeasureModeConverter : EnumBoolConverter<MeasureModes> { }
    public class ViewModeConverter : EnumBoolConverter<ViewModes> { }

    public partial class PlotterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PlotterController mController;

        public PlotterController Controller
        {
            get
            {
                return mController;
            }
        }

        private Dictionary<string, Action> CommandMap;

        private Dictionary<string, Action> KeyMap;

        private SelectModes mSelectMode = SelectModes.POINT;

        public ObservableCollection<LayerHolder> LayerList = new ObservableCollection<LayerHolder>();

        public CursorPosViewModel CursorPosVM = new CursorPosViewModel();

        public SelectModes SelectMode
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


        private CadFigure.Types mCreatingFigureType = CadFigure.Types.NONE;

        public CadFigure.Types CreatingFigureType
        {
            set
            {
                bool changed = ChangeFigureType(value);

                if (changed)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreatingFigureType)));
                }
            }

            get
            {
                return mCreatingFigureType;
            }
        }

        private MeasureModes mMeasureMode = MeasureModes.NONE;

        public MeasureModes MeasureMode
        {
            set
            {
                bool changed = ChangeMeasuerType(value);

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
                SettingsHolder.Settings.SnapToGrid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToGrid;
            }
        }

        public bool SnapToPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToPoint;
            }
        }

        public bool SnapToSegment
        {
            set
            {
                SettingsHolder.Settings.SnapToSegment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToSegment;
            }
        }

        public bool SnapToLine
        {
            set
            {
                SettingsHolder.Settings.SnapToLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToLine;
            }
        }
        #endregion

        #region Tree view
        public bool FilterTreeView
        {
            set
            {
                SettingsHolder.Settings.FilterTreeView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTreeView)));

                if (Controller != null)
                {
                    Controller.UpdateTreeView(true);
                }
            }

            get
            {
                return SettingsHolder.Settings.FilterTreeView;
            }
        }

        private void UpdateTreeView(bool remakeTree)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            if (SettingsHolder.Settings.FilterTreeView)
            {
                CadLayerTreeItem item = new CadLayerTreeItem();
                item.AddChildren(Controller.CurrentLayer, fig =>
                {
                    return fig.HasSelectedPointInclueChild();
                });

                mCadObjectTreeView.AttachRoot(item);
                mCadObjectTreeView.Redraw();
            }
            else
            {
                if (remakeTree)
                {
                    CadLayerTreeItem item = new CadLayerTreeItem(Controller.CurrentLayer);
                    mCadObjectTreeView.AttachRoot(item);
                    mCadObjectTreeView.Redraw();
                }
                else
                {
                    mCadObjectTreeView.Redraw();
                }
            }
        }

        private void SetTreeViewPos(int index)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            mCadObjectTreeView.SetVPos(index);
        }

        private int FindTreeViewItem(uint id)
        {
            int idx = mCadObjectTreeView.Find((item) =>
            {
                if (item is CadFigTreeItem)
                {
                    CadFigTreeItem figItem = (CadFigTreeItem)item;

                    if (figItem.Fig.ID == id)
                    {
                        return true;
                    }
                }

                return false;
            });

            return idx;
        }

        #endregion



        #region 表示設定

        public bool DrawFaceOutline
        {
            set
            {
                SettingsHolder.Settings.DrawFaceOutline = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawFaceOutline)));

                Redraw();
            }

            get
            {
                return SettingsHolder.Settings.DrawFaceOutline;
            }
        }

        public bool FillFace
        {
            set
            {
                SettingsHolder.Settings.FillFace = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillFace)));

                Redraw();
            }

            get
            {
                return SettingsHolder.Settings.FillFace;
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

        private MainWindow mMainWindow;

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

        Window mEditorWindow;

        public PlotterViewModel(MainWindow mainWindow, WindowsFormsHost viewHost)
        {
            mController = new PlotterController();

            mMainWindow = mainWindow;
            mViewHost = viewHost;

            InitCommandMap();
            InitKeyMap();

            SelectMode = mController.SelectMode;
            CreatingFigureType = mController.CreatingFigType;

            mController.Observer.StateChanged = StateChanged;

            mController.Observer.LayerListChanged =  LayerListChanged;

            mController.Observer.DataChanged = DataChanged;

            mController.Observer.CursorPosChanged = CursorPosChanged;

            mController.Observer.UpdateTreeView = UpdateTreeView;

            mController.Observer.SetTreeViewPos = SetTreeViewPos;

            mController.Observer.FindTreeViewItem = FindTreeViewItem;

            mController.Observer.OpenPopupMessage = OpenPopupMessage;

            mController.Observer.ClosePopupMessage = ClosePopupMessage;

            LayerListChanged(mController, mController.GetLayerListInfo());

            PlotterView1 = new PlotterView();
            PlotterViewGL1 = PlotterViewGL.Create();

            ViewMode = ViewModes.FREE;  // 一旦GL側を設定してViewをLoadしておく
            ViewMode = ViewModes.FRONT;
        }

        private void OpenPopupMessage(string text, PlotterObserver.MessageType messageType)
        {
            mMainWindow.OpenPopupMessage(text, messageType);
        }

        private void ClosePopupMessage()
        {
            mMainWindow.ClosePopupMessage();
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

            if (mCadObjectTreeView != null)
            {
                mCadObjectTreeView.CheckChanged += ObjectTreeView_CheckChanged;
            }
        }

        private void ObjectTreeView_CheckChanged(object sender, EventArgs e)
        {
            Redraw();
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
                { "new_doc", NewDocument },
                { "load", Load },
                { "save",Save },
                { "save_as",SaveAs },
                { "print",StartPrint },
                { "page_setting",PageSetting },
                { "doc_setting",DocSetting },
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
                { "flip_with_vector", FlipWithVector },
                { "flip_normal", FlipNormal },
                { "grid_settings", GridSettings },
                { "add_layer", AddLayer },
                { "remove_layer", RemoveLayer },
                { "centroid", AddCentroid },
                { "select_all", SelectAll },
                { "snap_settings", SnapSettings },
                { "show_editor", ShowEditor },
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
            Redraw();
        }

        public void Redo()
        {
            mController.Redo();
            Redraw();
        }

        public void Remove()
        {
            mController.Remove();
            Redraw();
        }

        public void SeparateFigure()
        {
            mController.SeparateFigures();
            Redraw();
        }

        public void BondFigure()
        {
            mController.BondFigures();
            Redraw();
        }

        public void ToBezier()
        {
            mController.ToBezier();
            Redraw();
        }

        public void CutSegment()
        {
            mController.CutSegment();
            Redraw();
        }

        public void InsPoint()
        {
            mController.InsPoint();
            Redraw();
        }

        public void ToLoop()
        {
            mController.SetLoop(true);
            Redraw();
        }

        public void ToUnloop()
        {
            mController.SetLoop(false);
            Redraw();
        }

        public void FlipX()
        {
            mController.FlipX();
            Redraw();
        }

        public void FlipY()
        {
            mController.FlipY();
            Redraw();
        }

        public void FlipZ()
        {
            mController.FlipZ();
            Redraw();
        }

        public void FlipWithVector()
        {
            mController.FlipWithVector();
        }

        public void FlipNormal()
        {
            mController.FlipNormal();
            Redraw();
        }

        public void ClearLayer()
        {
            mController.ClearLayer(0);
            Redraw();
        }

        public void Copy()
        {
            mController.Copy();
            Redraw();
        }

        public void Paste()
        {
            mController.Paste();
            Redraw();
        }

        public void NewDocument()
        {
            CurrentFileName = null;

            PlotterView1.DrawContext.WorldScale = 1.0;
            PlotterViewGL1.DrawContext.WorldScale = 1.0;

            mController.ClearAll();
            Redraw();
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
            if (CurrentFileName != null)
            {
                SaveFile(CurrentFileName);
                return;
            }

            SaveAs();
        }

        public void SaveAs()
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

                Redraw();
            }
        }

        public void SnapSettings()
        {
            SnapSettingsDialog dlg = new SnapSettingsDialog();

            dlg.Owner = mMainWindow;

            dlg.PointSnapRange = SettingsHolder.Settings.PointSnapRange;
            dlg.LineSnapRange = SettingsHolder.Settings.LineSnapRange;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                SettingsHolder.Settings.PointSnapRange = dlg.PointSnapRange;
                SettingsHolder.Settings.LineSnapRange = dlg.LineSnapRange;

                Redraw();
            }
        }

        public void ShowEditor()
        {
            if (mEditorWindow == null)
            {
                mEditorWindow = new EditorWindow(mController.ScriptEnv);
                mEditorWindow.Owner = mMainWindow;
                mEditorWindow.Show();

                mEditorWindow.Closed += delegate
                {
                    mEditorWindow = null;
                };
            }
            else
            {
                mEditorWindow.Activate();
            }
        }

        public void AddLayer()
        {
            mController.AddLayer(null);
            Redraw();
        }

        public void RemoveLayer()
        {
            mController.RemoveLayer(mController.CurrentLayer.ID);
            Redraw();
        }

        public void AddCentroid()
        {
            mController.AddCentroid();
            Redraw();
        }

        public void SelectAll()
        {
            mController.SelectAllInCurrentLayer();
            Redraw();
        }

        public void Cancel()
        {
            mController.Cancel();
            Redraw();
        }

        public void SearchNearestPoint()
        {
            mController.MoveCursorNearestPoint(mPlotterView.DrawContext);
            Redraw();
        }

        #endregion



        // Handle events from PlotterController
        #region Event From PlotterController

        public void DataChanged(PlotterController sender, bool redraw)
        {
            if (redraw)
            {
                Redraw();
            }
        }

        public void StateChanged(PlotterController sender, PlotterStateInfo si)
        {
            if (CreatingFigureType != si.CreatingFigureType)
            {
                CreatingFigureType = si.CreatingFigureType;
            }

            if (MeasureMode != si.MeasureMode)
            {
                MeasureMode = si.MeasureMode;
            }
        }

        public void LayerListChanged(PlotterController sender, LayerListInfo layerListInfo)
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

        private void CursorPosChanged(PlotterController sender, CadVector pt, Plotter.Controller.CursorType type)
        {
            if (type == Plotter.Controller.CursorType.TRACKING)
            {
                CursorPosVM.CursorPos = pt;
            }
            else if (type == Plotter.Controller.CursorType.LAST_DOWN)
            {
                CursorPosVM.CursorPos2 = pt;
            }
        }

        #endregion


        // Layer list handling
        #region LayerList
        public void LayerListItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LayerHolder lh = (LayerHolder)sender;
            //Draw(clearFlag:true);
            Redraw();
        }

        public void LayerListSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0)
            {
                LayerHolder layer = (LayerHolder)args.AddedItems[0];

                if (mController.CurrentLayer.ID != layer.ID)
                {
                    mController.setCurrentLayer(layer.ID);

                    Redraw();
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

        #region helper
        private void Redraw()
        {
            mController.Redraw(mController.CurrentDC);
        }
        #endregion

        #region "print"
        public void StartPrint()
        {
            PrintDocument pd =
                new PrintDocument();

            PageSettings storePageSettings = pd.DefaultPageSettings;

            pd.DefaultPageSettings.PaperSize = Controller.PageSize.GetPaperSize();

            pd.DefaultPageSettings.Landscape = Controller.PageSize.mLandscape;

            pd.PrintPage += PrintPage;

            System.Windows.Forms.PrintDialog pdlg = new System.Windows.Forms.PrintDialog();

            pdlg.Document = pd;

            if (pdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pd.Print();
            }

            pd.DefaultPageSettings = storePageSettings;
        }

        private void PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            CadSize2D deviceSize = new CadSize2D(e.PageBounds.Size.Width, e.PageBounds.Size.Height);
            CadSize2D pageSize = new CadSize2D(Controller.PageSize.Width, Controller.PageSize.Height);

            DrawContextPrinter dc = new DrawContextPrinter(mController.CurrentDC, g, pageSize, deviceSize);

            mController.Print(dc);
        }

        #endregion

        public void PageSetting()
        {
            System.Windows.Forms.PageSetupDialog pageDlg = new System.Windows.Forms.PageSetupDialog();

            PageSettings pageSettings = new PageSettings();

            pageSettings.PaperSize = Controller.PageSize.GetPaperSize();
            pageSettings.Landscape = Controller.PageSize.mLandscape;

            pageDlg.PageSettings = pageSettings;

            System.Windows.Forms.DialogResult result = pageDlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Controller.PageSize.Setup(pageDlg.PageSettings);

                Redraw();
            }
        }

        public void DocSetting()
        {
            DocumentSettingsDialog dlg = new DocumentSettingsDialog();

            dlg.Owner = mMainWindow;

            dlg.WoldScale = PlotterView1.DrawContext.WorldScale;

            bool? result = dlg.ShowDialog();

            if (result ?? false)
            {
                SetWorldScale(dlg.WoldScale);
                Redraw();
            }
        }

        public void SetWorldScale(double scale)
        {
            PlotterView1.DrawContext.WorldScale = scale;
            PlotterViewGL1.DrawContext.WorldScale = scale;
        }

        public void TextCommand(string s)
        {
            //mController.ScriptEnv.ExecuteCommandSync(s);
            mController.ScriptEnv.ExecuteCommandAsync(s);
        }

        private bool ChangeFigureType(CadFigure.Types newType)
        {
            var prev = mCreatingFigureType;

            if (mCreatingFigureType == newType)
            {
                // 現在のタイプを再度選択したら解除する
                mCreatingFigureType = CadFigure.Types.NONE;
            }
            else
            {
                mCreatingFigureType = newType;
            }

            if (mCreatingFigureType != CadFigure.Types.NONE)
            {
                MeasureMode = MeasureModes.NONE;
                mController.StartCreateFigure(mCreatingFigureType);

                Redraw();
            }
            else if (prev != CadFigure.Types.NONE)
            {
                mController.EndCreateFigure();

                Redraw();
            }

            return prev != mCreatingFigureType;
        }

        private bool ChangeMeasuerType(MeasureModes newType)
        {
            var prev = mMeasureMode;

            if (mMeasureMode == newType)
            {
                // 現在のタイプを再度選択したら解除する
                mMeasureMode = MeasureModes.NONE;
            }
            else
            {
                mMeasureMode = newType;
            }

            if (mMeasureMode != MeasureModes.NONE)
            {
                CreatingFigureType = CadFigure.Types.NONE;
                mController.StartMeasure(newType);
            }
            else if (prev != MeasureModes.NONE)
            {
                mController.EndMeasure();
                Redraw();
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
                    Redraw();
                    break;

                case ViewModes.BACK:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(-Vector3d.UnitZ, Vector3d.Zero, Vector3d.UnitY);
                    Redraw();
                    break;

                case ViewModes.TOP:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.UnitY, Vector3d.Zero, -Vector3d.UnitZ);
                    Redraw();
                    break;

                case ViewModes.BOTTOM:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(-Vector3d.UnitY, Vector3d.Zero, Vector3d.UnitZ);
                    Redraw();
                    break;

                case ViewModes.RIGHT:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(Vector3d.UnitX, Vector3d.Zero, Vector3d.UnitY);
                    Redraw();
                    break;

                case ViewModes.LEFT:
                    SetView(PlotterView1);
                    mPlotterView.DrawContext.SetCamera(-Vector3d.UnitX, Vector3d.Zero, Vector3d.UnitY);
                    Redraw();
                    break;

                case ViewModes.FREE:
                    PlotterViewGL1.Size = PlotterView1.Size;
                    PlotterViewGL1.DrawContext.UnitPerMilli = PlotterView1.DrawContext.UnitPerMilli;
                    SetView(PlotterViewGL1);
                    Redraw();
                    break;
            }

            return true;
        }

        public void SetupTextCommandView(AutoCompleteTextBox textBox)
        {
            textBox.CandidateList = Controller.ScriptEnv.AutoCompleteList;
        }

        public void LoadSettings()
        {
            PlotterSettings settings = SettingsHolder.Settings;

            //PlotterSettings settings = new PlotterSettings();

            settings.Load();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawFaceOutline)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillFace)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTreeView)));

            mController.Grid.GridSize = settings.GridSize;

            //mController.PointSnapRange = settings.PointSnapRange;

            //mController.LineSnapRange = settings.LineSnapRange;
        }

        public void SaveSettings()
        {
            PlotterSettings settings = SettingsHolder.Settings;

            settings.GridSize = mController.Grid.GridSize;

            settings.Save();
        }

        public void Open()
        {
            LoadSettings();
        }

        public void Close()
        {
            SaveSettings();

            if (mEditorWindow != null)
            {
                mEditorWindow.Close();
                mEditorWindow = null;
            }
        }
    }
}
