//#define COPY_AS_JSON
//#define PRINT_WITH_GL_ONLY
//#define PRINT_WITH_GDI_ONLY

using System.Collections.Generic;
using CadDataTypes;
using System.Drawing.Printing;
using Plotter.Controller.TaskRunner;
using System.Drawing;
using GLUtil;

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

        States mState = States.SELECT;
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

        public States mBackState;

        public CadObjectDB DB => mDB;

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

        public MeasureModes MeasureMode = MeasureModes.NONE;


        public CadFigure.Creator FigureCreator = null;

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

        #region Constructor
        public PlotterController()
        {
            CadLayer layer = mDB.NewLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            ViewCtrl = new ViewController();

            HistoryMan = new HistoryManager(mDB);

            ScriptEnv = new ScriptEnvironment(this);

            mPlotterTaskRunner = new PlotterTaskRunner(this);

            ObjDownPoint.Valid = false;

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
        //public void NotifyDataChanged(bool redraw)
        //{
        //    Observer.DataChanged(this, redraw);
        //}

        private void UpdateLayerList()
        {
            Observer.LayerListChanged(this, GetLayerListInfo());
        }

        public LayerListInfo GetLayerListInfo()
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
    #endregion

    #region Start and End creating figure

        public void StartCreateFigure(CadFigure.Types type)
        {
            State = States.START_CREATE;
            CreatingFigType = type;
        }

        public void EndCreateFigure()
        {
            CreatingFigType = CadFigure.Types.NONE;

            State = States.SELECT;

            if (FigureCreator != null)
            {
                FigureCreator.EndCreate(CurrentDC);
                FigureCreator = null;
            }

            UpdateTreeView(true);

            NotifyStateChange();
        }

        public void EndCreateFigureState()
        {
            if (FigureCreator != null)
            {
                FigureCreator.EndCreate(CurrentDC);
                FigureCreator = null;
            }

            NextState();
        }

        public void StartMeasure(MeasureModes mode)
        {
            State = States.MEASURING;
            MeasureMode = mode;
            MeasureFigureCreator =
                CadFigure.Creator.Get(
                    CadFigure.Types.POLY_LINES,
                    CadFigure.Create(CadFigure.Types.POLY_LINES)
                    );
        }

        public void EndMeasure()
        {
            State = States.SELECT;
            MeasureMode = MeasureModes.NONE;
            MeasureFigureCreator = null;
        }

        public void CloseFigure()
        {
            FigureCreator.Figure.IsLoop = true;

            FigureCreator.EndCreate(CurrentDC);

            CadOpe ope = new CadOpeSetClose(CurrentLayer.ID, FigureCreator.Figure.ID, true);
            HistoryMan.foward(ope);

            NextState();
        }
        #endregion

        public void setCurrentLayer(uint id)
        {
            mDB.CurrentLayerID = id;
            UpdateTreeView(true);
        }

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
            dc.Drawing.DrawAxis();
            dc.Drawing.DrawPageFrame(PageSize.Width, PageSize.Height, CadVertex.Zero);
            DrawGrid(dc);
        }

        public void Draw(DrawContext dc)
        {
            if (dc == null) return;

            lock (DB)
            {
                foreach (CadLayer layer in mDB.LayerList)
                {
                    if (layer.Visible)
                    {
                        DrawPen pen = dc.GetPen(DrawTools.PEN_DEFAULT_FIGURE);

                        if (layer.ID != CurrentLayer.ID)
                        {
                            pen = dc.GetPen(DrawTools.PEN_PALE_FIGURE);
                        }

                        dc.Drawing.Draw(layer.FigureList, pen);
                    }
                }

                dc.Drawing.Draw(TempFigureList, dc.GetPen(DrawTools.PEN_TEST_FIGURE));

                if (MeasureFigureCreator != null)
                {
                    MeasureFigureCreator.Figure.Draw(dc, dc.GetPen(DrawTools.PEN_MEASURE_FIGURE));
                }
            }
        }

        public void PushCurrent()
        {
            CurrentDC.Push();
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

            if (ObjDownPoint.Valid)
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

        public void PrintPage(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            DOut.pl($"Dev Width:{deviceSize.Width} Height:{deviceSize.Height}");
#if PRINT_WITH_GL_ONLY
            PrintPageGL(printerGraphics, pageSize, deviceSize);
#elif PRINT_WITH_GDI_ONLY
            PrintPageGDI(printerGraphics, pageSize, deviceSize);
#else
            PrintPageSwitch(printerGraphics, pageSize, deviceSize);
#endif
        }


        public void PrintPageSwitch(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(CurrentDC.GetType() == typeof(DrawContextGLPers)))
            {
                DrawContextPrinter dc = new DrawContextPrinter(CurrentDC, printerGraphics, pageSize, deviceSize);
                DrawAllFigure(dc);
            }
            else
            {
                Bitmap bmp = GetPrintableBmp(pageSize, deviceSize);
                printerGraphics.DrawImage(bmp, 0, 0);
            }
        }

        //public void PrintPageGDI(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        //{
        //    DrawContextPrinter dc = new DrawContextPrinter(CurrentDC, printerGraphics, pageSize, deviceSize);
        //    DrawAllFigure(dc);
        //}

        //public void PrintPageGL(Graphics printerGraphics, CadSize2D pageSize, CadSize2D deviceSize)
        //{
        //    Bitmap bmp = GetPrintableBmp(pageSize, deviceSize);

        //    if (bmp != null)
        //    {
        //        printerGraphics.DrawImage(bmp, 0, 0);
        //    }
        //}

        public Bitmap GetPrintableBmp(CadSize2D pageSize, CadSize2D deviceSize)
        {
            if (!(CurrentDC is DrawContextGL))
            {
                return null;
            }

            DrawContext dc = CurrentDC.CreatePrinterContext(pageSize, deviceSize);

            dc.SetupTools(DrawTools.ToolsType.PRINTER_GL);

            FrameBufferW fb = new FrameBufferW();
            fb.Create((int)deviceSize.Width, (int)deviceSize.Height);

            fb.Begin();

            dc.StartDraw();

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));

            DrawAllFigure(dc);

            dc.EndDraw();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            return bmp;
        }


#endregion

#region Private editing figure methods

        private void NextState()
        {
            if (State == States.CREATING)
            {
                if (ContinueCreate)
                {
                    FigureCreator = null;
                    StartCreateFigure(CreatingFigType);
                }
                else
                {
                    FigureCreator = null;
                    CreatingFigType = CadFigure.Types.NONE;
                    State = States.SELECT;
                    NotifyStateChange();
                }
            }
        }

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

                for (; i>=0; i--)
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

        public void MoveSelectedPoints(CadVertex delta)
        {
            StartEdit();
            MoveSelectedPoints(null, delta);
            EndEdit();
        }

        private void MoveSelectedPoints(DrawContext dc, CadVertex delta)
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

        public void ClearLayer(uint layerID)
        {
            if (layerID == 0)
            {
                layerID = CurrentLayer.ID;
            }
            
            CadLayer layer = mDB.GetLayer(layerID);

            if (layer == null) return;

            CadOpeList opeList = layer.Clear();

            HistoryMan.foward(opeList);
        }

        public void AddLayer(string name)
        {
            CadLayer layer = mDB.NewLayer();

            layer.Name = name;

            CurrentLayer = layer;

            mDB.LayerList.Add(layer);

            UpdateLayerList();

            ItConsole.println("Layer added.  Name:" + layer.Name + " ID:" + layer.ID);
        }

        public void RemoveLayer(uint id)
        {
            if (mDB.LayerList.Count == 1)
            {
                return;
            }

            CadLayer layer = mDB.GetLayer(id);

            if (layer == null)
            {
                return;
            }

            int index = mDB.LayerIndex(id);

            int nextCurrentIdx = -1;
                       
            if (CurrentLayer.ID == id)
            {
                nextCurrentIdx = mDB.LayerIndex(CurrentLayer.ID);
            }

            CadOpeRemoveLayer ope = new CadOpeRemoveLayer(layer, index);
            HistoryMan.foward(ope);

            mDB.RemoveLayer(id);

            if (nextCurrentIdx >= 0)
            {
                if (nextCurrentIdx > mDB.LayerList.Count-1)
                {
                    nextCurrentIdx = mDB.LayerList.Count - 1;
                }

                CurrentLayer = mDB.LayerList[nextCurrentIdx];
            }

            UpdateLayerList();
            ItConsole.println("Layer removed.  Name:" + layer.Name + " ID:" + layer.ID);
        }

        public void SelectAllInCurrentLayer()
        {
            foreach (CadFigure fig in CurrentLayer.FigureList)
            {
                fig.Select();
            }
        }

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
                MeasureMode = MeasureModes.NONE;
                MeasureFigureCreator = null;

                NotifyStateChange();
            }
        }

        public void SelectById(uint id, int idx, bool clearSelect=true)
        {
            CadFigure fig = mDB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            if (idx >= fig.PointCount)
            {
                return;
            }

            if (clearSelect)
            {
                ClearSelection();
            }

            if (idx>=0)
            {
                fig.SelectPointAt(idx, true);
            }
            else
            {
                fig.Select();
            }

            CurrentFigure = fig;
        }

        //public void ScaleSelectedFigure(CadVector org, double scale)
        //{
        //    StartEdit();

        //    List<uint> idlist = DB.GetSelectedFigIDList();

        //    foreach (uint id in idlist)
        //    {
        //        CadFigure fig = DB.GetFigure(id);

        //        if (fig == null)
        //        {
        //            continue;
        //        }

        //        ScaleFugure(org, scale, fig);
        //    }

        //    EndEdit();
        //}

        //public void ScaleFugure(CadVector org, double scale, CadFigure fig)
        //{
        //    int n = fig.PointList.Count;

        //    for (int i = 0; i < n; i++)
        //    {
        //        CadVector p = fig.PointList[i];
        //        p -= org;
        //        p *= scale;
        //        p += org;

        //        fig.SetPointAt(i, p);
        //    }
        //}

        //
        // p0 を原点として単位ベクトル v を軸に t ラジアン回転する
        //
        //public void RotateSelectedFigure(CadVector org, CadVector axisDir, double t)
        //{
        //    List<uint> idlist = DB.GetSelectedFigIDList();

        //    foreach (uint id in idlist)
        //    {
        //        CadFigure fig = DB.GetFigure(id);

        //        if (fig == null)
        //        {
        //            continue;
        //        }

        //        CadUtil.RotateFigure(fig, org, axisDir, t);
        //    }
        //}

        public void SelectFigure(uint figID)
        {
            CadFigure fig = DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            fig.Select();
        }

        public void ClearAll()
        {
            PageSize = new PaperPageSize();

            mDB.ClearAll();
            HistoryMan.Clear();

            UpdateLayerList();
            UpdateTreeView(true);
        }

        public void SetDB(CadObjectDB db)
        {
            mDB = db;

            HistoryMan = new HistoryManager(mDB);

            UpdateLayerList();

            UpdateTreeView(true);

            Redraw(CurrentDC);
        }

        public void Copy()
        {
            PlotterClipboard.CopyFiguresAsBin(this);
        }

        public void Paste()
        {
            PlotterClipboard.PasteFiguresAsBin(this);
            UpdateTreeView(true);
        }

        public void MovePointsFromStored(List<CadFigure> figList, CadVertex d)
        {
            if (figList == null)
            {
                return;
            }

            if (figList.Count == 0)
            {
                return;
            }

            foreach (CadFigure fig in figList)
            {
                fig.MoveSelectedPointsFromStored(CurrentDC, d);
            }
        }

        public void TextLine(string s)
        {
            //ScriptEnv.ExecuteCommandSync(s);
            ScriptEnv.ExecuteCommandAsync(s);
        }
    }
}
