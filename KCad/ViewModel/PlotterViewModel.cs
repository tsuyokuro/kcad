//#define USE_GDI_VIEW
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using KCad.Controls;
using System.Drawing.Printing;
using Plotter.Controller;
using KCad.Dialogs;
using System.Text.RegularExpressions;
using Plotter.svg;
using System.Xml.Linq;
using Plotter;

namespace KCad.ViewModel
{

    public class PlotterViewModel : ViewModelContext, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class KeyAction
        {
            public Action Down;
            public Action Up;
            public string Description;

            public KeyAction(Action down, Action up, string description = null)
            {
                Down = down;
                Up = up;
                Description = description;
            }
        }

        private Dictionary<string, Action> CommandMap;

        private Dictionary<string, KeyAction> KeyMap;

        public CursorPosViewModel CursorPosVM = new CursorPosViewModel();

        public ObjectTreeViewModel ObjTreeVM;

        public LayerListViewModel LayerListVM;

        private SelectModes mSelectMode = SelectModes.OBJECT;
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

        private SettingsVeiwModel SettingsVM;
        public SettingsVeiwModel Settings
        {
            get => SettingsVM;
        }

        private ICadMainWindow mMainWindow;

#if USE_GDI_VIEW
        private PlotterView PlotterView1 = null;
#endif
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
                ChangeCurrentFileName(mCurrentFileName);
            }
        }

        public PlotterViewModel(ICadMainWindow mainWindow)
        {
            mController = new PlotterController();

            SettingsVM = new SettingsVeiwModel(this);

            ObjTreeVM = new ObjectTreeViewModel(this);

            LayerListVM = new LayerListViewModel(this);

            mMainWindow = mainWindow;

            InitCommandMap();
            InitKeyMap();

            SelectMode = mController.SelectMode;
            CreatingFigureType = mController.CreatingFigType;

            mController.Observer.StateChanged = StateChanged;

            mController.Observer.CursorPosChanged = CursorPosChanged;

            mController.Observer.OpenPopupMessage = OpenPopupMessage;

            mController.Observer.ClosePopupMessage = ClosePopupMessage;

            mController.Observer.CursorLocked = CursorLocked;

            mController.Observer.ChangeMouseCursor = ChangeMouseCursor;

            mController.Observer.HelpOfKey = HelpOfKey;


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

        #region handling IMainWindow
        private void ChangeCurrentFileName(string fname)
        {
            if (fname != null)
            {
                mMainWindow.SetCurrentFileName(fname);
            }
            else
            {
                mMainWindow.SetCurrentFileName("");
            }
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

            mMainWindow.SetPlotterView(view);
        }
#endregion handling IMainWindow

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

            dlg.Owner = mMainWindow.GetWindow();

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

            dlg.Owner = mMainWindow.GetWindow();

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
                mEditorWindow.Owner = mMainWindow.GetWindow();
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

            dlg.Owner = mMainWindow.GetWindow();

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

#if (USE_GDI_VIEW)
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
#endif

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
