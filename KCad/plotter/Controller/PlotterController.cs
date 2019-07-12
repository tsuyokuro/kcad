using CadDataTypes;
using OpenTK;
using Plotter.Controller.TaskRunner;
using Plotter.Settings;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public enum States
        {
            SELECT,
            RUBBER_BAND_SELECT,
            START_DRAGING_POINTS,
            DRAGING_POINTS,
            DRAGING_VIEW_ORG,
            START_CREATE,
            CREATING,
            MEASURING,
        }

        private CadObjectDB mDB = new CadObjectDB();
        public CadObjectDB DB => mDB;

        private States mState = States.SELECT;
        public States State
        {
            private set
            {
                mState = value;

                if (mInteractCtrl.IsActive)
                {
                    mInteractCtrl.Cancel();
                }
            }

            get => mState;
        }

        private States mBackState;


        private PaperPageSize mPageSize = new PaperPageSize(PaperKind.A4, false);
        public PaperPageSize PageSize
        {
            get => mPageSize;
            set => mPageSize = value;
        }

        public SelectModes SelectMode
        {
            set;
            get;
        } = SelectModes.POINT;

        public CadLayer CurrentLayer
        {
            get => mDB.CurrentLayer;

            set
            {
                mDB.CurrentLayer = value;
                UpdateTreeView(true);
            }
        }


        CadFigure.Types mCreatingFigType = CadFigure.Types.NONE;

        public CadFigure.Types CreatingFigType
        {
            private set => mCreatingFigType = value;
            get => mCreatingFigType;
        }

        private MeasureModes mMeasureMode = MeasureModes.NONE;
        public MeasureModes MeasureMode
        {
            get => mMeasureMode;
        }

        private CadFigure.Creator mFigureCreator = null;
        public CadFigure.Creator FigureCreator
        {
            get => mFigureCreator;
        }

        public CadFigure.Creator MeasureFigureCreator = null;


        public HistoryManager HistoryMan = null;

        private List<CadFigure> EditFigList = new List<CadFigure>();

        public bool ContinueCreate { set; get; } = false;


        public PlotterObserver Observer = new PlotterObserver();


        public List<CadFigure> TempFigureList = new List<CadFigure>();

        public DrawContext CurrentDC = null;

        public ScriptEnvironment ScriptEnv;

        public ViewController ViewCtrl;

        public PlotterTaskRunner mPlotterTaskRunner;

        private ContextMenuManager mContextMenuMan;

        public ContextMenuManager ContextMenuMan
        {
            get => mContextMenuMan;
        }

        #region Constructor
        public PlotterController()
        {
            CadLayer layer = mDB.NewLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            ViewCtrl = new ViewController();

            HistoryMan = new HistoryManager(mDB);

            ScriptEnv = new ScriptEnvironment(this);

            mContextMenuMan = new ContextMenuManager(this);

            mPlotterTaskRunner = new PlotterTaskRunner(this);

            ObjDownPoint = VectorExt.InvalidVector3d;

            InitHid();
        }
        #endregion


        #region TreeView
        public void UpdateTreeView(bool remakeTree)
        {
            Observer.UpdateTreeView(remakeTree);
        }

        public void SetTreeViewPos(int index)
        {
            Observer.SetTreeViewPos(index);
        }

        public int FindTreeViewItem(uint id)
        {
            return Observer.FindTreeViewItem(id);
        }

        #endregion TreeView


        #region Notify

        public void UpdateLayerList()
        {
            Observer.LayerListChanged(this, GetLayerListInfo());
        }

        private LayerListInfo GetLayerListInfo()
        {
            LayerListInfo layerInfo = default(LayerListInfo);
            layerInfo.LayerList = mDB.LayerList;
            layerInfo.CurrentID = CurrentLayer.ID;

            return layerInfo;
        }

        private void NotifyStateChange()
        {
            PlotterStateInfo si = default(PlotterStateInfo);
            si.set(this);

            Observer.StateChanged(this, si);
        }

        #endregion Notify


        #region Start and End creating figure

        public void StartCreateFigure(CadFigure.Types type)
        {
            State = States.START_CREATE;
            CreatingFigType = type;
        }

        public void EndCreateFigure()
        {
            if (mFigureCreator != null)
            {
                mFigureCreator.EndCreate(CurrentDC);
                mFigureCreator = null;
            }

            NextState();
        }

        public void CloseFigure()
        {
            if (FigureCreator != null)
            {
                FigureCreator.Figure.IsLoop = true;

                CadOpe ope = new CadOpeSetClose(CurrentLayer.ID, FigureCreator.Figure.ID, true);
                HistoryMan.foward(ope);

                FigureCreator.EndCreate(CurrentDC);
            }

            NextState();
        }

        private void NextState()
        {
            if (State == States.CREATING)
            {
                if (ContinueCreate)
                {
                    mFigureCreator = null;
                    StartCreateFigure(CreatingFigType);
                }
                else
                {
                    mFigureCreator = null;
                    CreatingFigType = CadFigure.Types.NONE;
                    State = States.SELECT;

                    UpdateTreeView(true);
                    NotifyStateChange();
                }
            }
        }

        public void StartMeasure(MeasureModes mode)
        {
            State = States.MEASURING;
            mMeasureMode = mode;
            MeasureFigureCreator =
                CadFigure.Creator.Get(
                    CadFigure.Types.POLY_LINES,
                    CadFigure.Create(CadFigure.Types.POLY_LINES)
                    );
        }

        public void EndMeasure()
        {
            State = States.SELECT;
            mMeasureMode = MeasureModes.NONE;
            MeasureFigureCreator = null;
        }

        #endregion Start and End creating figure


        #region "undo redo"

        public void Undo()
        {
            ClearSelection();
            HistoryMan.undo();
            UpdateTreeView(true);
            UpdateLayerList();
        }

        public void Redo()
        {
            ClearSelection();
            HistoryMan.redo();
            UpdateTreeView(true);
            UpdateLayerList();
        }

        #endregion


        #region "Draw methods"

        public void PushDraw()
        {
            CurrentDC.Push();
        }

        public void Redraw(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = CurrentDC;
            }

            dc.StartDraw();
            Clear(dc);
            DrawAll(dc);
            dc.EndDraw();
            dc.Push();
        }

        public void Clear(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = CurrentDC;
            }

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));
        }

        public void DrawAll(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = CurrentDC;
            }

            DrawBase(dc);
            DrawDragLine(dc);
            DrawCrossCursor(dc);
            Draw(dc);
            DrawSelectedItems(dc);
            DrawLastPoint(dc);

            DrawHighlightPoint(dc);
            DrawAccordingState(dc);
        }

        public void DrawBase(DrawContext dc)
        {
            if (SettingsHolder.Settings.DrawAxis)
            {
                dc.Drawing.DrawAxis();
            }
            else
            {
                dc.Drawing.DrawCrossScrn(dc.GetPen(DrawTools.PEN_AXIS), dc.WorldPointToDevPoint(Vector3d.Zero), 8);
            }

            dc.Drawing.DrawPageFrame(PageSize.Width, PageSize.Height, Vector3d.Zero);
            DrawGrid(dc);
        }

        public void Draw(DrawContext dc)
        {
            if (dc == null) return;

            lock (DB)
            {
                foreach (CadLayer layer in mDB.LayerList)
                {
                    if (!layer.Visible) continue;

                    // Skip current layer.
                    // It will be drawn at the end.
                    if (layer == CurrentLayer) { continue; }

                    DrawPen pen = dc.GetPen(DrawTools.PEN_PALE_FIGURE);

                    dc.Drawing.Draw(layer.FigureList, pen);
                }

                // Draw current layer at last
                if (CurrentLayer != null && CurrentLayer.Visible)
                {
                    DrawPen pen = dc.GetPen(DrawTools.PEN_DEFAULT_FIGURE);
                    dc.Drawing.Draw(CurrentLayer.FigureList, pen);
                }

                dc.Drawing.Draw(TempFigureList, dc.GetPen(DrawTools.PEN_TEST_FIGURE));

                if (MeasureFigureCreator != null)
                {
                    MeasureFigureCreator.Figure.Draw(dc, dc.GetPen(DrawTools.PEN_MEASURE_FIGURE));
                }
            }
        }

        public void DrawGrid(DrawContext dc)
        {
            if (SettingsHolder.Settings.SnapToGrid)
            {
                dc.Drawing.DrawGrid(mGridding);
            }
        }

        public void DrawSelectedItems(DrawContext dc)
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                dc.Drawing.DrawSelected(layer.FigureList, dc.GetPen(DrawTools.PEN_SELECT_POINT));
            }
        }

        public void DrawLastPoint(DrawContext dc)
        {
            dc.Drawing.DrawMarkCursor(
                dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER),
                LastDownPoint,
                ControllerConst.MARK_CURSOR_SIZE);

            if (ObjDownPoint.IsValid())
            {
                dc.Drawing.DrawMarkCursor(
                    dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER2),
                    ObjDownPoint,
                    ControllerConst.MARK_CURSOR_SIZE);
            }
        }

        public void DrawDragLine(DrawContext dc)
        {
            if (State != States.DRAGING_POINTS)
            {
                return;
            }

            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_DRAG_LINE),
                LastDownPoint, dc.DevPointToWorldPoint(CrossCursor.Pos));
        }

        public void DrawCrossCursor(DrawContext dc)
        {
            dc.Drawing.DrawCrossCursorScrn(CrossCursor, dc.GetPen(DrawTools.PEN_CURSOR2));

            if (CursorLocked)
            {
                dc.Drawing.DrawCrossScrn(
                    dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT),
                    CrossCursor.Pos,
                    ControllerConst.CURSOR_LOCK_MARK_SIZE);
            }
        }

        public void DrawSelRect(DrawContext dc)
        {
            dc.Drawing.DrawRectScrn(
                dc.GetPen(DrawTools.PEN_TEMP_FIGURE),
                RubberBandScrnPoint0,
                RubberBandScrnPoint1);
        }

        public void DrawAllFigure(DrawContext dc)
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                if (!layer.Visible)
                {
                    continue;
                }

                dc.Drawing.Draw(layer.FigureList, dc.GetPen(DrawTools.PEN_DEFAULT_FIGURE));
            }
        }

        public void DrawAccordingState(DrawContext dc)
        {
            switch (State)
            {
                case States.SELECT:
                    break;

                case States.START_DRAGING_POINTS:
                    break;

                case States.RUBBER_BAND_SELECT:
                    DrawSelRect(dc);
                    break;

                case States.DRAGING_POINTS:
                    break;

                case States.START_CREATE:
                    break;

                case States.CREATING:
                    if (FigureCreator != null)
                    {
                        Vector3d p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        FigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
                    }
                    break;

                case States.MEASURING:
                    if (MeasureFigureCreator != null)
                    {
                        Vector3d p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        MeasureFigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
                    }
                    break;
            }

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Draw(dc, SnapPoint);
            }
        }

        public void DrawHighlightPoint(DrawContext dc)
        {
            dc.Drawing.DrawHighlightPoints(HighlightPointList);
        }

        #endregion


        #region Private editing figure methods

        public void ClearSelection()
        {
            CurrentFigure = null;

            LastSelPoint = null;
            LastSelSegment = null;

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ClearSelectedFlags();
            }
        }

        private CadOpeFigureSnapShotList mSnapShotList;

        public List<CadFigure> StartEdit()
        {
            EditFigList = DB.GetSelectedFigList();
            return StartEdit(EditFigList);
        }

        public List<CadFigure> StartEdit(List<CadFigure> targetList)
        {
            mSnapShotList = new CadOpeFigureSnapShotList();

            mSnapShotList.StoreBefore(targetList);

            foreach (CadFigure fig in targetList)
            {
                if (fig != null)
                {
                    fig.StartEdit();
                }
            }

            return targetList;
        }

        public void AbendEdit()
        {
            mSnapShotList = null;
        }

        public void EndEdit()
        {
            EndEdit(EditFigList);
        }

        public void EndEdit(List<CadFigure> targetList)
        {
            foreach (CadFigure fig in targetList)
            {
                if (fig != null)
                {
                    fig.EndEdit();
                }
            }

            CadOpeList root = new CadOpeList();

            CadOpeList rmOpeList = RemoveInvalidFigure();

            root.Add(rmOpeList);

            mSnapShotList.StoreAfter(DB);
            root.Add(mSnapShotList);

            HistoryMan.foward(root);

            mSnapShotList = null;
        }

        public void CancelEdit()
        {
            foreach (CadFigure fig in EditFigList)
            {
                if (fig != null)
                {
                    fig.CancelEdit();
                }
            }
        }

        private CadOpeList RemoveInvalidFigure()
        {
            CadOpeList opeList = new CadOpeList();

            int removeCnt = 0;

            foreach (CadLayer layer in mDB.LayerList)
            {
                IReadOnlyList<CadFigure> list = layer.FigureList;

                int i = list.Count - 1;

                for (; i >= 0; i--)
                {
                    CadFigure fig = list[i];

                    if (fig.IsGarbage())
                    {
                        CadOpe ope = new CadOpeRemoveFigure(layer, fig.ID);
                        opeList.OpeList.Add(ope);

                        layer.RemoveFigureByIndex(i);

                        removeCnt++;
                    }
                }
            }

            if (removeCnt > 0)
            {
                UpdateTreeView(true);
            }

            return opeList;
        }

        public void MoveSelectedPoints(Vector3d delta)
        {
            StartEdit();
            MoveSelectedPoints(null, delta);
            EndEdit();
        }

        private void MoveSelectedPoints(DrawContext dc, Vector3d delta)
        {
            List<uint> figIDList = DB.GetSelectedFigIDList();

            //delta.z = 0;

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    fig.MoveSelectedPointsFromStored(dc, delta);
                }
            }
        }

        #endregion


        #region Getting selection
        public bool HasSelect()
        {
            bool ret = false;

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachFigF(fig =>
                {
                    if (fig.HasSelectedPointInclueChild())
                    {
                        ret = true;
                        return false;
                    }

                    return true;
                });
            }

            return ret;
        }

        public List<CadFigure> GetSelectedFigureList()
        {
            List<CadFigure> figList = new List<CadFigure>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint())
                    {
                        figList.Add(fig);
                    }
                });
            }

            return figList;
        }

        public List<CadFigure> GetSelectedRootFigureList()
        {
            List<CadFigure> figList = new List<CadFigure>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachRootFig(fig =>
                {
                    if (fig.HasSelectedPointInclueChild())
                    {
                        figList.Add(fig);
                    }
                });
            }

            return figList;
        }

        #endregion Getting selection


        public void Cancel()
        {
            if (CursorLocked)
            {
                CursorLocked = false;
            }

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Cancel();
            }


            if (State == States.START_CREATE || State == States.CREATING)
            {
                StartCreateFigure(CadFigure.Types.NONE);
                NotifyStateChange();
            }
            else if (State == States.DRAGING_POINTS)
            {
                CancelEdit();

                State = States.SELECT;
                ClearSelection();
            }
            else if (State == States.MEASURING)
            {
                State = States.SELECT;
                mMeasureMode = MeasureModes.NONE;
                MeasureFigureCreator = null;

                NotifyStateChange();
            }
        }

        public void SetDB(CadObjectDB db)
        {
            mDB = db;

            HistoryMan = new HistoryManager(mDB);

            UpdateLayerList();

            UpdateTreeView(true);

            Redraw(CurrentDC);
        }

        public void SetCurrentLayer(uint id)
        {
            mDB.CurrentLayerID = id;
            UpdateTreeView(true);
        }

        public void TextCommand(string s)
        {
            //ScriptEnv.ExecuteCommandSync(s);
            ScriptEnv.ExecuteCommandAsync(s);
        }

        public void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            PlotterPrinter.PrintPage(this, printerGraphics, pageSize, deviceSize);
        }
    }
}
