using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Plotter
{

    public delegate void StateChanged(PlotterController sender, PlotterController.StateInfo si);

    public delegate void LayerListChanged(PlotterController sender, PlotterController.LayerListInfo layerListInfo);

    public delegate void RequestContextMenu(PlotterController sender, PlotterController.StateInfo si, int x, int y);

    public delegate void DataChanged(PlotterController sender, bool redraw);


    public enum CursorType
    {
        TRACKING,
        LAST_DOWN,
    }


    public delegate void CursorPosChanged(PlotterController sender, CadVector pt, CursorType type);

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

                if (pc.CreatingFigure != null)
                {
                    CreatingFigurePointCnt = pc.CreatingFigure.PointCount;
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
            public delegate void PrintFunc(string s);
            public PrintFunc println = (a) => { };
            public PrintFunc print = (a) => { };
            public VoidFunc clear = () => { };
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

        MeasureModes mMeasureMode = MeasureModes.NONE;

        public MeasureModes MeasureMode
        {
            set
            {
                mMeasureMode = value;
            }
            get
            {
                return mMeasureMode;
            }
        }

        private CadFigure MeasureFigure = null;

        private CadFigure CreatingFigure
        {
            set;
            get;

        } = null;

        private HistoryManager mHistoryManager = null;

        public HistoryManager HistoryManager
        {
            get
            {
                return mHistoryManager;
            }
        }

        private SelectList mSelList = new SelectList();

        public SelectList SelList
        {
            get
            {
                return mSelList;
            }
        }

        private SelectSegmentList mSelectedSegs = new SelectSegmentList();

        public SelectSegmentList SelSegList
        {
            get
            {
                return mSelectedSegs;
            }
        }

        private List<uint> EditIdList = new List<uint>();


        public bool ContinueCreate { set; get; } = false;

        public StateChanged StateChanged;

        public RequestContextMenu RequestContextMenu;

        #region Delegators
        private LayerListChanged mLayerListChanged = (a, b) => { };

        public LayerListChanged LayerListChanged
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

        private DataChanged mDataChanged = (a, b) => { };

        public DataChanged DataChanged
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

        private CursorPosChanged mCursorPosChanged = (a, b, c) => { };

        public CursorPosChanged CursorPosChanged
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
        #endregion

        public ObservableCollection<CadObjectItem> ObjectTreeItemsSource;

        private List<CadFigure> TempFigureList = new List<CadFigure>();

        DrawContext mCurrentDC;

        public DrawContext CurrentDC
        {
            get
            {
                return mCurrentDC;
            }

            set
            {
                mCurrentDC = value;
            }
        }

        public ScriptEnvironment ScriptEnv;


        #region Constructor
        public PlotterController()
        {
            //CrossCursor.DirX = CadVector.Create(1, 1, 0).UnitVector();
            //CrossCursor.DirY = CadVector.Create(1, -2, 0).UnitVector();

            CadLayer layer = mDB.NewLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            mHistoryManager = new HistoryManager(mDB);

            ScriptEnv = new ScriptEnvironment(this);


            InitHid();
        }
        #endregion


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

            //NotifyStateChange();

            // Creation start when specify the first coordinate.
            // So, at the moment, not yet a creation start.
        }

        public void EndCreateFigure()
        {
            CreatingFigType = CadFigure.Types.NONE;

            State = States.SELECT;

            if (CreatingFigure != null)
            {
                CreatingFigure.EndCreate(CurrentDC);
                CreatingFigure = null;
            }

            NotifyStateChange();
        }

        public void EndCreateFigureState()
        {
            if (CreatingFigure != null)
            {
                CreatingFigure.EndCreate(CurrentDC);
                CreatingFigure = null;
            }

            NextState();
        }

        public void StartMeasure(MeasureModes mode)
        {
            State = States.MEASURING;
            MeasureMode = mode;
            MeasureFigure = new CadFigure(CadFigure.Types.POLY_LINES);
        }

        public void EndMeasure()
        {
            State = States.SELECT;
            MeasureMode = MeasureModes.NONE;
            MeasureFigure = null;
        }

        public void CloseFigure()
        {
            CreatingFigure.IsLoop = true;

            CreatingFigure.EndCreate(CurrentDC);

            CadOpe ope = CadOpe.CreateSetCloseOpe(CurrentLayer.ID, CreatingFigure.ID, true);
            mHistoryManager.foward(ope);

            NextState();
        }
        #endregion

        public void setCurrentLayer(uint id)
        {
            mDB.CurrentLayerID = id;
        }

        #region "undo redo"
        public void Undo()
        {
            ClearSelection();
            mHistoryManager.undo();
        }

        public void Redo()
        {
            ClearSelection();
            mHistoryManager.redo();
        }
        #endregion


        #region "Draw methods"
        public void Clear(DrawContext dc)
        {
            if (dc == null) return;
            dc.Drawing.Clear();
        }

        public void DrawAll(DrawContext dc)
        {
            DrawCrossCursor(dc);
            Draw(dc);
            DrawSelectedItems(dc);
            DrawLastPoint(dc);

            DrawHighlightPoint(dc);
            DrawAccordingState(dc);
        }

        public void Draw(DrawContext dc)
        {
            if (dc == null) return;

            dc.Drawing.DrawAxis();
            DrawGrid(dc);
            dc.Drawing.DrawPageFrame();

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

            if (MeasureFigure != null)
            {
                MeasureFigure.Draw(dc, DrawTools.PEN_MEASURE_FIGURE);
            }
        }

        public void DrawGrid(DrawContext dc)
        {
            if (mGridding.Enable)
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

            if (mObjDownPoint != null)
            {
                dc.Drawing.DrawDownPointCursor(
                    DrawTools.PEN_LAST_POINT_MARKER2, mObjDownPoint.Value);
            }
        }

        public void DrawCursor(DrawContext dc)
        {
            dc.Drawing.DrawCursorScrn(mSnapPointScrn);
        }

        public void DrawCrossCursor(DrawContext dc)
        {
            CrossCursor.Pos = mSnapPointScrn;
            dc.Drawing.DrawCrossCursorScrn(CrossCursor);
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


        //-----------------------------------------------------------------------------------------
        // Edit figure methods

        #region Private editing figure methods

        private void NextState()
        {
            if (State == States.CREATING)
            {
                if (ContinueCreate)
                {
                    CadFigure.Types type = CreatingFigure.Type;
                    CreatingFigure = null;
                    StartCreateFigure(type);
                }
                else
                {
                    CreatingFigure = null;
                    CreatingFigType = CadFigure.Types.NONE;
                    State = States.SELECT;
                    NotifyStateChange();
                }
            }
        }

        private void ClearSelection()
        {
            mSelList.clear();
            mSelectedSegs.Clear();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.ClearSelectedFlags();
            }
        }

        private HashSet<uint> SelListToIDSet()
        {
            HashSet<uint> idSet = new HashSet<uint>();

            foreach (SelectItem a in mSelList.List)
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
                foreach (CadFigure fig in layer.FigureList)
                {
                    foreach (CadVector p in fig.PointList)
                    {
                        if (p.Selected)
                        {
                            idList.Add(fig.ID);
                            break;
                        }
                    }
                }
            }
            return idList;
        }

        public void StartEdit()
        {
            EditIdList = GetSelectedFigIDList();

            foreach (uint id in EditIdList)
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    fig.StartEdit();
                }
            }
        }

        public void EndEdit()
        {
            DiffDataList ddl = new DiffDataList();

            List<uint> figIDList = EditIdList;

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    DiffData dd = fig.EndEdit();

                    if (dd != null)
                    {
                        ddl.DiffDatas.Add(dd);
                    }
                }
            }

            if (ddl.DiffDatas.Count > 0)
            {
                CadOpeList root = CadOpe.CreateListOpe();

                CadOpe ope = CadOpe.CreateDiffOpe(ddl);
                root.OpeList.Add(ope);

                CadOpeList fopeList = RemoveInvalidFigure();
                root.OpeList.Add(fopeList);

                mHistoryManager.foward(root);
            }

            UpdateSelectItemPoints();
        }

        public void CancelEdit()
        {
            EditIdList = GetSelectedFigIDList();

            foreach (uint id in EditIdList)
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    fig.CancelEdit();
                }
            }
        }


        private void UpdateSelectItemPoints()
        {
            HashSet<SelectItem> removeSels = new HashSet<SelectItem>();

            foreach (SelectItem item in mSelList.List)
            {
                if (!item.update())
                {
                    removeSels.Add(item);
                }
            }

            mSelList.RemoveAll(a => removeSels.Contains(a));


            HashSet<MarkSeg> removeSegs = new HashSet<MarkSeg>();

            foreach (MarkSeg item in mSelectedSegs.List)
            {
                if (!item.Update())
                {
                    removeSegs.Add(item);
                }
            }

            mSelectedSegs.List.RemoveAll(a => removeSegs.Contains(a));
        }

        private CadOpeList RemoveInvalidFigure()
        {
            CadOpeList opeList = new CadOpeList();

            foreach (CadLayer layer in mDB.LayerList)
            {
                IReadOnlyList<CadFigure> list = layer.FigureList;

                int i = list.Count - 1;

                for (; i>=0; i--)
                {
                    CadFigure fig = list[i];

                    if (fig.PointCount == 0)
                    {
                        CadOpe ope = CadOpe.CreateRemoveFigureOpe(layer, fig.ID);
                        opeList.OpeList.Add(ope);

                        layer.RemoveFigureByIndex(i);
                    }
                }
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

        private void RemoveSelectedPoints()
        {
            List<uint> figIDList = GetSelectedFigIDList();
            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.GetFigure(id);
                fig.RemoveSelected();
            }
        }

        #endregion

        #region "Copy and paste"
        public void Copy()
        {
            CopyFigures();
        }

        public void Paste()
        {
            PasteFigure();
        }

        public void CopyFigures()
        {
            List<uint> figIdList = GetSelectedFigIDList();

            var figList = new List<CadFigure>();

            figIdList.ForEach(id =>
            {
                CadFigure fig = mDB.GetFigure(id);
                if (fig != null)
                {
                    figList.Add(fig);
                }
            });

            Clipboard.SetData("List.CadFiguer", figList);
        }

        public void PasteFigure()
        {
            if (Clipboard.ContainsData("List.CadFiguer"))
            {
                CadVector pp = default(CadVector);

                pp = LastDownPoint;

                Log.d("paste");
                List<CadFigure> list = (List<CadFigure>)Clipboard.GetData("List.CadFiguer");

                CadRect cr = CadUtil.GetContainsRect(list);

                CadVector d = pp - cr.p0;

                d.z = 0;

                CadOpeList opeRoot = CadOpe.CreateListOpe();

                foreach (CadFigure fig in list)
                {
                    fig.MoveAllPoints(d);
                    mDB.AddFigure(fig);
                    CurrentLayer.AddFigure(fig);

                    CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, fig.ID);
                    opeRoot.OpeList.Add(ope);

                    fig.Select();
                }

                mHistoryManager.foward(opeRoot);
            }
        }

        #endregion

        #region "File access"
        public void SaveToJsonFile(String fname)
        {
            StreamWriter writer = new StreamWriter(fname);

            JObject jo = mDB.ToJson();

            writer.Write(jo.ToString());
            writer.Close();
        }

        public void LoadFromJsonFile(String fname)
        {
            StreamReader reader = new StreamReader(fname);

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jo = JObject.Parse(js);

            CadObjectDB db = new CadObjectDB();
            db.FromJson(jo);

            mDB = db;

            mHistoryManager.DB = mDB;

            mHistoryManager = new HistoryManager(mDB);

            NotifyLayerInfo();
        }
        #endregion

        public void ClearLayer(uint layerID)
        {
            if (layerID == 0)
            {
                layerID = CurrentLayer.ID;
            }
            
            CadLayer layer = mDB.GetLayer(layerID);

            if (layer == null) return;

            CadOpeList opeList = layer.Clear();

            mHistoryManager.foward(opeList);
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
                MeasureFigure = null;

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

            SetCurrentFigure(fig);
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
        public void RotateSelectedFigure(CadVector p0, CadVector v, double t)
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

                RotateFigure(p0, v, t, fig);
            }

            EndEdit();
        }

        public void RotateFigure(CadVector p0, CadVector v, double t, CadFigure fig)
        {
            CadQuaternion q = CadQuaternion.RotateQuaternion(v, t);
            CadQuaternion r = q.Conjugate(); ;

            CadQuaternion qp;

            int n = fig.PointList.Count;

            for (int i = 0; i < n; i++)
            {
                CadVector p = fig.PointList[i];

                p -= p0;

                qp = CadQuaternion.FromPoint(p);

                qp = r * qp;
                qp = qp * q;

                p = qp.ToPoint();

                p += p0;

                fig.SetPointAt(i, p);
            }
        }
    }
}
