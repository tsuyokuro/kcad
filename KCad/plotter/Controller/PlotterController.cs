﻿//#define COPY_AS_JSON

using KCad;
using MessagePack;
using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CadDataTypes;

namespace Plotter.Controller
{
    //public delegate void StateChanged(PlotterController sender, PlotterController.StateInfo si);

    //public delegate void LayerListChanged(PlotterController sender, PlotterController.LayerListInfo layerListInfo);

    //public delegate void RequestContextMenu(PlotterController sender, PlotterController.StateInfo si, int x, int y);

    //public delegate void DataChanged(PlotterController sender, bool redraw);

    //public delegate void CursorPosChanged(PlotterController sender, CadVector pt, CursorType type);

    public enum CursorType
    {
        TRACKING,
        LAST_DOWN,
    }

    public partial class PlotterController
    {
        public enum States
        {
            SELECT,
            RUBBER_BAND_SELECT,
            START_DRAGING_POINTS,
            DRAGING_POINTS,
            START_CREATE,
            CREATING,
            MEASURING,
        }

        public enum SelectModes
        {
            POINT,
            OBJECT,
        }

        public enum MeasureModes
        {
            NONE,
            POLY_LINE,
        }

        public struct StateInfo
        {
            public States State;
            public SelectModes SelectMode;
            public CadFigure.Types CreatingFigureType;
            public int CreatingFigurePointCnt;
            public MeasureModes MeasureMode;

            public void set(PlotterController pc)
            {
                State = pc.State;
                SelectMode = pc.SelectMode;
                CreatingFigureType = pc.CreatingFigType;
                CreatingFigurePointCnt = 0;

                if (pc.FigureCreator != null)
                {
                    CreatingFigurePointCnt = pc.FigureCreator.Figure.PointCount;
                }

                MeasureMode = pc.MeasureMode;
            }
        }

        public struct LayerListInfo
        {
            public List<CadLayer> LayerList;
            public uint CurrentID;
        }

        public class Interaction
        {
            public PrintFunc println = (a) => { };
            public PrintFunc print = (a) => { };
            public FormatPrintFunc printf = (a, b) => { };
            public Action clear = () => { };
        }

        public Interaction InteractOut { set; get; } = new Interaction();

        private CadObjectDB mDB = new CadObjectDB();

        public States State
        {
            private set;
            get;
        } = States.SELECT;

        public CadObjectDB DB
        {
            get
            {
                return mDB;
            }
        }


        public SelectModes SelectMode
        {
            set;
            get;
        } = SelectModes.POINT;

        public CadLayer CurrentLayer
        {
            get
            {
                return mDB.CurrentLayer;
            }

            set
            {
                mDB.CurrentLayer = value;
                UpdateTreeView(true);
            }
        }


        CadFigure.Types mCreatingFigType = CadFigure.Types.NONE;

        public CadFigure.Types CreatingFigType
        {
            private set
            {
                mCreatingFigType = value;
            }
            get
            {
                return mCreatingFigType;
            }
        }

        public MeasureModes MeasureMode = MeasureModes.NONE;

        private CadFigure.Creator FigureCreator = null;
        private CadFigure.Creator MeasureFigureCreator = null;

        public HistoryManager HistoryMan = null;

        public SelectList SelList = new SelectList();

        public SelectSegmentList SelSegList = new SelectSegmentList();

        private List<CadFigure> EditFigList = new List<CadFigure>();


        public bool ContinueCreate { set; get; } = false;

        // sender, stateInfo
        public Action<PlotterController, StateInfo> StateChanged;

        // sender, stateInfo, x, y
        public Action<PlotterController, StateInfo, int, int> RequestContextMenu;

        // sender, layerListInfo
        private Action<PlotterController, LayerListInfo> mLayerListChanged = (a, b) => { };

        public Action<PlotterController, LayerListInfo> LayerListChanged
        {
            set
            {
                mLayerListChanged = value;
                if (mLayerListChanged == null)
                {
                    mLayerListChanged = (a, b) => { };
                }

                NotifyLayerInfo();
            }
        }

        // sender, redraw
        private Action<PlotterController, bool> mDataChanged = (a, b) => { };

        public Action<PlotterController, bool> DataChanged
        {
            set
            {
                mDataChanged = value;
                if (mDataChanged == null)
                {
                    mDataChanged = (a, b) => { };
                }
            }
        }

        // sender, cursorPos, cursorType
        private Action<PlotterController, CadVector, CursorType> mCursorPosChanged = (a, b, c) => { };

        public Action<PlotterController, CadVector, CursorType> CursorPosChanged
        {
            set
            {
                mCursorPosChanged = value;
                if (mCursorPosChanged == null)
                {
                    mCursorPosChanged = (a, b, c) => { };
                }
            }
            get
            {
                return mCursorPosChanged;
            }
        }


        public List<CadFigure> TempFigureList = new List<CadFigure>();

        public DrawContext CurrentDC = null;

        public ScriptEnvironment ScriptEnv;

        public ViewController ViewCtrl;


        #region Constructor
        public PlotterController()
        {
            CadLayer layer = mDB.NewLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            ViewCtrl = new ViewController();

            HistoryMan = new HistoryManager(mDB);

            ScriptEnv = new ScriptEnvironment(this);

            ObjDownPoint.Valid = false;

            InitHid();
        }
        #endregion

        #region TreeView
        CadObjectTreeView mCadObjectTreeView;

        public void SetObjectTreeView(CadObjectTreeView treeView)
        {
            mCadObjectTreeView = treeView;
        }

        public void UpdateTreeView(bool remakeTree)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            if (SettingsHolder.Settings.FilterTreeView)
            {
                CadLayerTreeItem item = new CadLayerTreeItem();
                item.AddChildren(CurrentLayer, fig => { return fig.HasSelectedPoint(); });

                mCadObjectTreeView.AttachRoot(item);
                mCadObjectTreeView.Redraw();
            }
            else
            {
                if (remakeTree)
                {
                    CadLayerTreeItem item = new CadLayerTreeItem(CurrentLayer);
                    mCadObjectTreeView.AttachRoot(item);
                    mCadObjectTreeView.Redraw();
                }
                else
                {
                    mCadObjectTreeView.Redraw();
                }
            }
        }

        public void SetTreeViewPos(int index)
        {
            if (mCadObjectTreeView == null)
            {
                return;
            }

            mCadObjectTreeView.SetVPos(index);
        }

        public int FindTreeViewItem(uint id)
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

        #endregion TreeView

        #region Notify
        public void NotifyDataChanged(bool redraw)
        {
            mDataChanged(this, redraw);
        }

        private void NotifyLayerInfo()
        {
            LayerListInfo layerInfo = default(LayerListInfo);
            layerInfo.LayerList = mDB.LayerList;
            layerInfo.CurrentID = CurrentLayer.ID;

            mLayerListChanged(this, layerInfo);
        }

        private void NotifyStateChange()
        {
            if (StateChanged == null)
            {
                return;
            }

            StateInfo si = default(StateInfo);
            si.set(this);

            StateChanged(this, si);
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

            CadOpe ope = CadOpe.CreateSetCloseOpe(CurrentLayer.ID, FigureCreator.Figure.ID, true);
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
        }

        public void Redo()
        {
            ClearSelection();
            HistoryMan.redo();
            UpdateTreeView(true);
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

            dc.Drawing.Clear();
        }

        public void DrawAll(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = CurrentDC;
            }

            DrawBase(dc);
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
            dc.Drawing.DrawPageFrame();
            DrawGrid(dc);
        }

        public void Draw(DrawContext dc)
        {
            if (dc == null) return;

            foreach (CadLayer layer in mDB.LayerList)
            {
                if (layer.Visible)
                {
                    int pen = DrawTools.PEN_DEFAULT_FIGURE;

                    if (layer.ID != CurrentLayer.ID)
                    {
                        pen = DrawTools.PEN_PALE_FIGURE;
                    }

                    dc.Drawing.Draw(layer, pen);
                }
            }

            dc.Drawing.Draw(TempFigureList, DrawTools.PEN_TEST_FIGURE);

            if (MeasureFigureCreator != null)
            {
                MeasureFigureCreator.Figure.Draw(dc, DrawTools.PEN_MEASURE_FIGURE);
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
                dc.Drawing.DrawSelected(layer);
            }
        }

        public void DrawLastPoint(DrawContext dc)
        {
            dc.Drawing.DrawDownPointCursor(
                DrawTools.PEN_LAST_POINT_MARKER, LastDownPoint);

            if (ObjDownPoint.Valid)
            {
                dc.Drawing.DrawDownPointCursor(
                    DrawTools.PEN_LAST_POINT_MARKER2, ObjDownPoint);
            }
        }

        public void DrawCrossCursor(DrawContext dc)
        {
            dc.Drawing.DrawCrossCursorScrn(CrossCursor);

            if (CursorLocked)
            {
                dc.Drawing.DrawCrossScrn(DrawTools.PEN_POINT_HIGHTLITE, CrossCursor.Pos, 8);
            }
        }

        public void DrawSelRect(DrawContext dc)
        {
            dc.Drawing.DrawRectScrn(DrawTools.PEN_TEMP_FIGURE, RubberBandScrnPoint0, RubberBandScrnPoint1);
        }

        public void Print(DrawContext dc)
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                dc.Drawing.Draw(layer);
            }
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

            SelList.clear();
            SelSegList.Clear();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ClearSelectedFlags();
            }
        }

        private HashSet<uint> SelListToIDSet()
        {
            HashSet<uint> idSet = new HashSet<uint>();

            foreach (SelectItem a in SelList.List)
            {
                if (!idSet.Contains(a.FigureID))
                {
                    idSet.Add(a.FigureID);
                }
            }

            return idSet;
        }

        public List<uint> GetSelectedFigIDList()
        {
            List<uint> idList = new List<uint>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint())
                    {
                        idList.Add(fig.ID);
                    }
                });
            }
            return idList;
        }

        public List<CadFigure> GetSelectedFigList()
        {
            List<CadFigure> list = new List<CadFigure>();

            HashSet<CadFigure> fset = new HashSet<CadFigure>(); 


            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachFig(fig =>
                {
                    if (fig.HasSelectedPoint())
                    {
                        fset.Add(fig);
                        if (fig.Parent != null)
                        {
                            fset.Add(fig.Parent);
                        }
                    }
                });
            }

            list.AddRange(fset);

            return list;
        }

        private CadOpeFigureSnapShotList mSnapShotList;

        public void StartEdit()
        {
            EditFigList = GetSelectedFigList();

            mSnapShotList = new CadOpeFigureSnapShotList();

            mSnapShotList.StoreBefore(EditFigList);

            foreach (CadFigure fig in EditFigList)
            {
                if (fig != null)
                {
                    fig.StartEdit();
                }
            }
        }

        public void AbendEdit()
        {
            mSnapShotList = null;
        }

        public void EndEdit()
        {
            foreach (CadFigure fig in EditFigList)
            {
                if (fig != null)
                {
                    fig.EndEdit();
                }
            }

            UpdateSelectItemPoints();

            CadOpeList root = new CadOpeList();

            CadOpeList rmOpeList = RemoveInvalidFigure();

            root.Add(rmOpeList);

            mSnapShotList.StoreAfter(DB);
            root.Add(mSnapShotList);

            HistoryMan.foward(root);


            mSnapShotList = null;

            NotifySelectList();
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

        private void UpdateSelectItemPoints()
        {
            HashSet<SelectItem> removeSels = new HashSet<SelectItem>();

            foreach (SelectItem item in SelList.List)
            {
                if (!item.update())
                {
                    removeSels.Add(item);
                }
            }

            SelList.RemoveAll(a => removeSels.Contains(a));


            HashSet<MarkSeg> removeSegs = new HashSet<MarkSeg>();

            foreach (MarkSeg item in SelSegList.List)
            {
                if (!item.Update())
                {
                    removeSegs.Add(item);
                }
            }

            SelSegList.List.RemoveAll(a => removeSegs.Contains(a));
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
                        CadOpe ope = CadOpe.CreateRemoveFigureOpe(layer, fig.ID);
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

        public void MoveSelectedPoints(CadVector delta)
        {
            StartEdit();
            MoveSelectedPoints(null, delta);
            EndEdit();
        }

        private void MoveSelectedPoints(DrawContext dc, CadVector delta)
        {
            List<uint> figIDList = GetSelectedFigIDList();

            //delta.z = 0;

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    fig.MoveSelectedPoints(dc, delta);
                }
            }
        }


        #endregion

        private List<CadFigure> GetSelectedFigureList()
        {
            List<CadFigure> figList = new List<CadFigure>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ForEachFig(fig =>
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

            NotifyLayerInfo();

            InteractOut.println("Layer added.  Name:" + layer.Name + " ID:" + layer.ID);
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

            int nextCurrentIdx = -1;

            if (CurrentLayer.ID == id)
            {
                nextCurrentIdx = mDB.LayerIndex(CurrentLayer.ID);
            }

            mDB.RemoveLayer(id);

            if (nextCurrentIdx >= 0)
            {
                if (nextCurrentIdx > mDB.LayerList.Count-1)
                {
                    nextCurrentIdx = mDB.LayerList.Count - 1;
                }

                CurrentLayer = mDB.LayerList[nextCurrentIdx];
            }

            NotifyLayerInfo();
            InteractOut.println("Layer removed.  Name:" + layer.Name + " ID:" + layer.ID);
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

            if (mInteractCtrl.CurrentMode != InteractCtrl.Mode.NONE)
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

        public void ScaleSelectedFigure(CadVector org, double scale)
        {
            StartEdit();

            List<uint> idlist = GetSelectedFigIDList();

            foreach (uint id in idlist)
            {
                CadFigure fig = DB.GetFigure(id);

                if (fig == null)
                {
                    continue;
                }

                ScaleFugure(org, scale, fig);
            }

            EndEdit();
        }

        public void ScaleFugure(CadVector org, double scale, CadFigure fig)
        {
            int n = fig.PointList.Count;

            for (int i = 0; i < n; i++)
            {
                CadVector p = fig.PointList[i];
                p -= org;
                p *= scale;
                p += org;

                fig.SetPointAt(i, p);
            }
        }

        //
        // p0 を原点として単位ベクトル v を軸に t ラジアン回転する
        //
        public void RotateSelectedFigure(CadVector org, CadVector axisDir, double t)
        {
            List<uint> idlist = GetSelectedFigIDList();

            foreach (uint id in idlist)
            {
                CadFigure fig = DB.GetFigure(id);

                if (fig == null)
                {
                    continue;
                }

                CadUtil.RotateFigure(fig, org, axisDir, t);
            }
        }

        /// <summary>
        /// 選択されたPointをselListに追加する
        /// </summary>
        /// <param name="selList">追加されるSelectList</param>
        /// 
        public void CollectSelList(SelectList selList)
        {
            foreach (CadLayer layer in DB.LayerList)
            {
                if (layer.Locked || layer.Visible == false)
                {
                    continue;
                }

                foreach (CadFigure fig in layer.FigureList)
                {
                    for (int i = 0; i < fig.PointCount; i++)
                    {
                        if (fig.PointList[i].Selected)
                        {
                            selList.add(layer.ID, fig, i);
                        }
                    }
                }
            }
        }

        public void ClearAll()
        {
            mDB.ClearAll();
            HistoryMan.Clear();

            NotifyLayerInfo();
            UpdateTreeView(true);
        }


        public void print(string s)
        {
            InteractOut.print(s);
        }

        public void println(string s)
        {
            InteractOut.println(s);
        }

        public void printf(string format, params object[] args)
        {
            InteractOut.printf(format, args);
        }
    }
}
