//#define USE_GDI_VIEW
using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using KCad;
using KCad.Controls;
using System.Drawing.Printing;
using Plotter.Controller;
using Plotter.Settings;
using KCad.Dialogs;
using System.Text.RegularExpressions;
using Plotter.svg;
using System.Xml.Linq;
using Plotter;

namespace KCad.ViewModel
{
    public partial class PlotterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PlotterController mController;

        public PlotterController Controller
        {
            get => mController;
        }

        private Dictionary<string, Action> CommandMap;

        private Dictionary<string, KeyAction> KeyMap;

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

            get => mSelectMode;
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

            get => mCreatingFigureType;
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

            get => mMeasureMode;
        }


        private ViewModes mViewMode = ViewModes.NONE;

        public ViewModes ViewMode
        {
            set
            {
#if (USE_GDI_VIEW)
                bool changed = ChangeViewModeGdi(value);
#else
                bool changed = ChangeViewMode(value);
#endif
                if (changed)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewMode)));
                }
            }

            get => mViewMode;
        }

        public DrawContext CurrentDC => mController?.CurrentDC;

        private SettingsVeiwModel mSettingsVeiwModel;

        public SettingsVeiwModel Settings
        {
            get => mSettingsVeiwModel;
        }

#region Tree view
        private void UpdateTreeView(bool remakeTree)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                UpdateTreeViewProc(remakeTree);
            }, true);
        }

        private void UpdateTreeViewProc(bool remakeTree)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            if (SettingsHolder.Settings.FilterObjectTree)
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

            ThreadUtil.RunOnMainThread(() => {
                mCadObjectTreeView.SetVPos(index);
            }, true);
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

        public void ObjectTreeView_ItemCommand(CadObjTreeItem treeItem, string cmd)
        {
            if (!(treeItem is CadFigTreeItem))
            {
                return;
            }

            CadFigTreeItem figItem = (CadFigTreeItem)treeItem;

            if (cmd == CadFigTreeItem.ITEM_CMD_CHANGE_NAME)
            {
                CadFigure fig = figItem.Fig;

                InputStringDialog dlg = new InputStringDialog();

                dlg.Message = KCad.Properties.Resources.string_input_fig_name;

                if (fig.Name != null)
                {
                    dlg.InputString = fig.Name;
                }

                bool? dlgRet = dlg.ShowDialog();

                if (dlgRet.Value)
                {
                    fig.Name = dlg.InputString;
                    UpdateTreeView(false);
                }
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

            get => mLayerListView;
        }

        private MainWindow mMainWindow;

        private PlotterView PlotterView1 = null;

        private PlotterViewGL PlotterViewGL1 = null;


        private IPlotterView mPlotterView = null;

        public System.Windows.Forms.Control CurrentView
        {
            get => mPlotterView.FormsControl;
        }

        Window mEditorWindow;

        MoveKeyHandler mMoveKeyHandler;

        private string mCurrentFileName = null;

        public string CurrentFileName
        {
            get => mCurrentFileName;

            private set
            {
                mCurrentFileName = value;

                if (mCurrentFileName != null)
                {
                    mMainWindow.FileName.Content = mCurrentFileName;
                }
                else
                {
                    mMainWindow.FileName.Content = "";
                }
            }
        }

        public PlotterViewModel(MainWindow mainWindow)
        {
            mController = new PlotterController();

            mSettingsVeiwModel = new SettingsVeiwModel(mController);

            mMainWindow = mainWindow;

            InitCommandMap();
            InitKeyMap();

            SelectMode = mController.SelectMode;
            CreatingFigureType = mController.CreatingFigType;

            mController.Observer.StateChanged = StateChanged;

            mController.Observer.LayerListChanged =  LayerListChanged;

            //mController.Observer.DataChanged = DataChanged;

            mController.Observer.CursorPosChanged = CursorPosChanged;

            mController.Observer.UpdateObjectTree = UpdateTreeView;

            mController.Observer.SetObjectTreePos = SetTreeViewPos;

            mController.Observer.FindObjectTreeItem = FindTreeViewItem;

            mController.Observer.OpenPopupMessage = OpenPopupMessage;

            mController.Observer.ClosePopupMessage = ClosePopupMessage;

            mController.Observer.CursorLocked = CursorLocked;

            mController.Observer.ChangeMouseCursor = ChangeMouseCursor;

            mController.Observer.HelpOfKey = HelpOfKey;

            //LayerListChanged(mController, mController.GetLayerListInfo());

            mController.UpdateLayerList();

#if USE_GDI_VIEW
            PlotterView1 = new PlotterView();
#endif
            PlotterViewGL1 = PlotterViewGL.Create();

            ViewMode = ViewModes.FRONT;
            ViewMode = ViewModes.FREE;  // 一旦GL側を設定してViewをLoadしておく
            ViewMode = ViewModes.FRONT;

            mMoveKeyHandler = new MoveKeyHandler(Controller);
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
            //if (view == mPlotterView)
            //{
            //    return;
            //}

            if (mPlotterView != null)
            {
                mPlotterView.SetController(null);
            }

            mPlotterView = view;

            mPlotterView.SetController(mController);

            mController.CurrentDC = view.DrawContext;

            mMainWindow.SetMainView(view);
        }

        CadObjectTreeView mCadObjectTreeView;

        public CadObjectTreeView ObjectTreeView
        {
            set
            {
                if (mCadObjectTreeView != null)
                {
                    mCadObjectTreeView.CheckChanged -= ObjectTreeView_CheckChanged;
                    mCadObjectTreeView.ItemCommand -= ObjectTreeView_ItemCommand;
                }

                mCadObjectTreeView = value;

                if (mCadObjectTreeView != null)
                {
                    mCadObjectTreeView.CheckChanged += ObjectTreeView_CheckChanged;
                    mCadObjectTreeView.ItemCommand += ObjectTreeView_ItemCommand;
                }
            }

            get => mCadObjectTreeView;
        }

        private void ObjectTreeView_CheckChanged(CadObjTreeItem item)
        {
            mController.CurrentFigure = 
                TreeViewUtil.GetCurrentFigure(item, mController.CurrentFigure);

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
                { "flip_with_vector", FlipWithVector },
                { "flip_and_copy_with_vector", FlipAndCopyWithVector },
                { "flip_normal", FlipNormal },
                { "rotate_with_point", RotateWithPoint },
                { "grid_settings", GridSettings },
                { "add_layer", AddLayer },
                { "remove_layer", RemoveLayer },
                { "centroid", AddCentroid },
                { "select_all", SelectAll },
                { "snap_settings", SnapSettings },
                { "show_editor", ShowEditor },
                { "export_svg", ExportSVG },
            };
        }

        private void InitKeyMap()
        {
            KeyMap = new Dictionary<string, KeyAction>
            {
                { "ctrl+z", new KeyAction(Undo , null,
                    AnsiEsc.BGreen + "Ctrl+Z" + AnsiEsc.Reset + " Undo")},

                { "ctrl+y", new KeyAction(Redo , null,
                    AnsiEsc.BGreen + "Ctrl+Y" + AnsiEsc.Reset + " Rendo")},

                { "ctrl+c", new KeyAction(Copy , null,
                    AnsiEsc.BGreen + "Ctrl+C" + AnsiEsc.Reset + " Copy")},

                { "ctrl+insert", new KeyAction(Copy , null, null)},

                { "ctrl+v", new KeyAction(Paste ,null,
                    AnsiEsc.BGreen + "Ctrl+C" + AnsiEsc.Reset + " Paste")},

                { "shift+insert", new KeyAction(Paste , null)},

                { "delete", new KeyAction(Remove , null)},

                { "ctrl+s", new KeyAction(Save , null,
                    AnsiEsc.BGreen + "Ctrl+S" + AnsiEsc.Reset + " Save")},

                { "ctrl+a", new KeyAction(SelectAll , null,
                    AnsiEsc.BGreen + "Ctrl+A" + AnsiEsc.Reset + " Select All")},

                { "escape", new KeyAction(Cancel , null)},

                { "ctrl+p", new KeyAction(InsPoint , null,
                    AnsiEsc.BGreen + "Ctrl+P" + AnsiEsc.Reset + " Inser Point")},

                { "f3", new KeyAction(SearchNearPoint , null,
                    AnsiEsc.BGreen + "F3" + AnsiEsc.Reset + " Search near Point")},

                { "f2", new KeyAction(CursorLock , null,
                    AnsiEsc.BGreen + "F2" + AnsiEsc.Reset + " Lock Cursor")},

                { "left", new KeyAction(MoveKeyDown, MoveKeyUp)},

                { "right", new KeyAction(MoveKeyDown, MoveKeyUp)},

                { "up", new KeyAction(MoveKeyDown, MoveKeyUp)},

                { "down", new KeyAction(MoveKeyDown, MoveKeyUp)},

                { "m", new KeyAction(AddMark, null,
                    AnsiEsc.BGreen + "M" + AnsiEsc.Reset + " Add snap point")},

                { "ctrl+m", new KeyAction(CleanMark, null,
                    AnsiEsc.BGreen + "Ctrl+M" + AnsiEsc.Reset + " Clear snap points")},
            };
        }

        public List<string> HelpOfKey(string keyword)
        {
            List<string> ret = new List<string>();

            if (keyword == null)
            {
                foreach (KeyAction a in KeyMap.Values)
                {
                    if (a.Description == null) continue;

                    ret.Add(a.Description);
                }

                return ret;
            }

            Regex re = new Regex(keyword, RegexOptions.IgnoreCase);

            foreach (KeyAction a in KeyMap.Values)
            {
                if (a.Description == null) continue;

                if (re.Match(a.Description).Success)
                {
                    ret.Add(a.Description);
                }
            }

            return ret;
        }

        public void ExecCommand(string cmd)
        {
            Action action;
            CommandMap.TryGetValue(cmd, out action);

            action?.Invoke();
        }

        public bool ExecShortcutKey(string keyCmd, bool down)
        {
            KeyAction ka;
            KeyMap.TryGetValue(keyCmd, out ka);

            if (down)
            {
                ka?.Down?.Invoke();
            }
            else
            {
                ka?.Up?.Invoke();
            }

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

        public void FlipWithVector()
        {
            mController.FlipWithVector();
        }

        public void FlipAndCopyWithVector()
        {
            mController.FlipAndCopyWithVector();
        }

        public void RotateWithPoint()
        {
            mController.RotateWithPoint();
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

#if USE_GDI_VIEW
            PlotterView1.SetWorldScale(1.0);
#endif
            PlotterViewGL1.SetWorldScale(1.0);

            mController.ClearAll();
            Redraw();
        }

        public void Load()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CadFileAccessor.LoadFile(ofd.FileName, this);
                CurrentFileName = ofd.FileName;
            }
        }

        public void Save()
        {
            if (CurrentFileName != null)
            {
                CadFileAccessor.SaveFile(CurrentFileName, this);
                return;
            }

            SaveAs();
        }

        public void SaveAs()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CadFileAccessor.SaveFile(sfd.FileName, this);
                CurrentFileName = sfd.FileName;
            }
        }

        public void GridSettings()
        {
            GridSettingsDialog dlg = new GridSettingsDialog();

            dlg.GridSize = Settings.GridSize;

            dlg.Owner = mMainWindow;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                Settings.GridSize = dlg.GridSize;

                Redraw();
            }
        }

        public void SnapSettings()
        {
            SnapSettingsDialog dlg = new SnapSettingsDialog();

            dlg.Owner = mMainWindow;

            dlg.PointSnapRange = Settings.PointSnapRange;
            dlg.LineSnapRange = Settings.LineSnapRange;

            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                Settings.PointSnapRange = dlg.PointSnapRange;
                Settings.LineSnapRange = dlg.LineSnapRange;

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

        public void ExportSVG()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<CadFigure> figList = Controller.DB.GetSelectedFigList();

                SvgExporter exporter = new SvgExporter();

                XDocument doc = exporter.ToSvg(figList, Controller.CurrentDC,
                    Controller.PageSize.Width, Controller.PageSize.Height);

                try
                {
                    doc.Save(sfd.FileName);
                    ItConsole.println("Success Export SVG: " + sfd.FileName);

                    System.Diagnostics.Process.Start(
                        "EXPLORER.EXE", $@"/select,""{sfd.FileName}""");
                }
                catch (Exception e)
                {
                    ItConsole.printError(e.Message);
                }
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

        public void SearchNearPoint()
        {
            mController.MoveCursorToNearPoint(mPlotterView.DrawContext);
            Redraw();
        }

        public void CursorLock()
        {
            mController.CursorLock();
        }

        public void MoveKeyDown()
        {
            mMoveKeyHandler.MoveKeyDown();
        }

        public void MoveKeyUp()
        {
            mMoveKeyHandler.MoveKeyUp();
        }


        public void AddMark()
        {
            mController.AddExtendSnapPoint();
            Redraw();
        }

        public void CleanMark()
        {
            mController.ClearExtendSnapPointList();
            Redraw();
        }

        #endregion

        // Handle events from PlotterController
        #region Event From PlotterController

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

        private void CursorPosChanged(PlotterController sender, Vector3d pt, Plotter.Controller.CursorType type)
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

        private void CursorLocked(bool locked)
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                mPlotterView.CursorLocked(locked);
            }, true);
        }

        private void ChangeMouseCursor(PlotterObserver.MouseCursorType cursorType)
        {
            //DOut.pl("ViewModel: ChangeMouseCursor");

            ThreadUtil.RunOnMainThread(() =>
            {
                mPlotterView.ChangeMouseCursor(cursorType);
            }, true);
        }

        #endregion Event From PlotterController


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
                    mController.SetCurrentLayer(layer.ID);

                    Redraw();
                }
            }
        }
        #endregion LayerList


        // Keyboard handling
        #region Keyboard handling
        private string GetModifyerKeysString()
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
            string ks = GetModifyerKeysString();

            ks += e.Key.ToString().ToLower();

            return ks;
        }


        public bool OnKeyDown(object sender, KeyEventArgs e)
        {
            string ks = KeyString(e);
            return ExecShortcutKey(ks, true);
        }

        public bool OnKeyUp(object sender, KeyEventArgs e)
        {
            string ks = KeyString(e);
            return ExecShortcutKey(ks, false);
        }
        #endregion Keyboard handling

        #region helper
        public void Redraw()
        {
            ThreadUtil.RunOnMainThread(() =>
            {
                mController.Redraw();
            }, true);
        }
        #endregion helper

        #region print
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

            Controller.PrintPage(g, pageSize, deviceSize);
        }
        #endregion print

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

            dlg.WorldScale = mPlotterView.DrawContext.WorldScale;

            bool? result = dlg.ShowDialog();

            if (result ?? false)
            {
                SetWorldScale(dlg.WorldScale);
                Redraw();
            }
        }

        public void SetWorldScale(double scale)
        {
#if USE_GDI_VIEW
            PlotterView1.SetWorldScale(scale);
#endif
            PlotterViewGL1.SetWorldScale(scale);
        }

        public void TextCommand(string s)
        {
            mController.TextCommand(s);
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

            DrawContext currentDC = mPlotterView == null? null : mPlotterView.DrawContext;
            DrawContext nextDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            IPlotterView view = mPlotterView;

            switch (mViewMode)
            {
                case ViewModes.FRONT:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);
                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BACK:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.TOP:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, -Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BOTTOM:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.RIGHT:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.LEFT:
                    PlotterViewGL1.EnablePerse(false);
                    view = PlotterViewGL1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.FREE:
                    PlotterViewGL1.EnablePerse(true);
                    view = PlotterViewGL1;
                    nextDC = view.DrawContext;
                    break;
            }

            if (currentDC != null) currentDC.Deactive();
            if (nextDC != null) nextDC.Active();

            SetView(view);
            Redraw();
            return true;
        }

        private bool ChangeViewModeGdi(ViewModes newMode)
        {
            if (mViewMode == newMode)
            {
                return false;
            }

            mViewMode = newMode;

            DrawContext currentDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            DrawContext nextDC = mPlotterView == null ? null : mPlotterView.DrawContext;
            IPlotterView view = mPlotterView;

            switch (mViewMode)
            {
                case ViewModes.FRONT:
                    view = PlotterView1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);
                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BACK:
                    view = PlotterView1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitZ * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.TOP:
                    view = PlotterView1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, -Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.BOTTOM:
                    view = PlotterView1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitY * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitZ);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.RIGHT:
                    view = PlotterView1;
                    view.DrawContext.SetCamera(
                        Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.LEFT:
                    view = PlotterView1;
                    view.DrawContext.SetCamera(
                        -Vector3d.UnitX * DrawContext.STD_EYE_DIST,
                        Vector3d.Zero, Vector3d.UnitY);

                    nextDC = view.DrawContext;
                    break;

                case ViewModes.FREE:
                    PlotterViewGL1.EnablePerse(true);
                    view = PlotterViewGL1;
                    nextDC = view.DrawContext;
                    break;
            }

            if (currentDC != null) currentDC.Deactive();
            if (nextDC != null) nextDC.Active();

            SetView(view);
            Redraw();
            return true;
        }


        public void SetupTextCommandView(AutoCompleteTextBox textBox)
        {
            textBox.CandidateList = Controller.ScriptEnv.AutoCompleteList;
        }

        public void Open()
        {
            Settings.Load();
        }

        public void Close()
        {
            Settings.Save();

            if (mEditorWindow != null)
            {
                mEditorWindow.Close();
                mEditorWindow = null;
            }
        }
    }
}
