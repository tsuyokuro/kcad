using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Plotter
{

    public delegate void StateChanged(PlotterController sender, PlotterController.StateInfo si);

    public delegate void LayerListChanged(PlotterController sender, PlotterController.LayerListInfo layerListInfo);

    public delegate void RequestContextMenu(PlotterController sender, PlotterController.StateInfo si, int x, int y);

    public delegate void DataChanged(PlotterController sender, bool redraw);

    public delegate void CursorPosChanged(PlotterController sender, CadPoint pt);

    public partial class PlotterController
    {
        public enum States
        {
            SELECT,
            START_DRAGING_POINTS,
            DRAGING_POINTS,
            START_CREATE,
            CREATING,
        }

        public enum SelectModes
        {
            POINT,
            OBJECT,
        }

        public struct StateInfo
        {
            public States State;
            public SelectModes SelectMode;
            public CadFigure.Types CreatingFigureType;
            public int CreatingFigurePointCnt;

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
            }
        }

        public struct LayerListInfo
        {
            public List<CadLayer> LayerList;
            public uint CurrentID;
        }

        public class Interaction
        {
            public delegate void Print(string s);
            public Print print = (a) => { };
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

        private CadFigure CreatingFigure { set; get; } = null;



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

        private CursorPosChanged mCursorPosChanged = (a, b) => { };

        public CursorPosChanged CursorPosChanged
        {
            set
            {
                mCursorPosChanged = value;
                if (mCursorPosChanged == null)
                {
                    mCursorPosChanged = (a, b) => { };
                }
            }
            get
            {
                return mCursorPosChanged;
            }
        }
        #endregion

        private List<CadFigure> TempFigureList = new List<CadFigure>();

        public DrawContext CurrentDC
        {
            get; set;
        }

        public ScriptEnvironment ScriptEnv;

        public PlotterController()
        {
            CadLayer layer = mDB.newLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            mHistoryManager = new HistoryManager(mDB);

            ScriptEnv = new ScriptEnvironment(this);

            initHid();
        }


        #region Notify
        private void NotifyDataChanged()
        {
            mDataChanged(this, false);
        }

        private void RequestRedraw()
        {
            mDataChanged(this, true);
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

        public void startCreateFigure(CadFigure.Types type, DrawContext dc)
        {
            State = States.START_CREATE;
            CreatingFigType = type;

            //NotifyStateChange();

            // Creation start when specify the first coordinate.
            // So, at the moment, not yet a creation start.
        }

        public void endCreateFigure(DrawContext dc)
        {
            CreatingFigType = CadFigure.Types.NONE;

            State = States.SELECT;

            if (CreatingFigure != null)
            {
                CreatingFigure.endCreate(dc);
                CreatingFigure = null;
            }

            NotifyStateChange();
        }

        public void endCreateFigureState(DrawContext dc)
        {
            if (CreatingFigure != null)
            {
                CreatingFigure.endCreate(dc);
                CreatingFigure = null;
            }

            NextState(dc);
        }

        public void closeFigure(DrawContext dc)
        {
            Log.d("PlotterController closeFigure");

            CreatingFigure.Closed = true;

            CreatingFigure.endCreate(dc);

            CadOpe ope = CadOpe.getSetCloseOpe(CurrentLayer.ID, CreatingFigure.ID, true);
            mHistoryManager.foward(ope);

            NextState(dc);
        }
        #endregion

        public void setCurrentLayer(uint id)
        {
            mDB.CurrentLayerID = id;
        }

        #region "undo redo"
        public void undo(DrawContext dc)
        {
            ClearSelection();
            mHistoryManager.undo();

            UpdateRelPoints();

            dc.Drawing.Clear();
            Draw(dc);
        }

        public void redo(DrawContext dc)
        {
            ClearSelection();
            mHistoryManager.redo();

            UpdateRelPoints();

            dc.Drawing.Clear();
            Draw(dc);
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
            Draw(dc);
            DrawSelectedItems(dc);
            DrawSubItems(dc);
        }

        public void Draw(DrawContext dc)
        {
            if (dc == null) return;

            dc.Drawing.DrawAxis();
            dc.Drawing.DrawPageFrame();

            foreach (CadLayer layer in mDB.LayerList)
            {
                if (layer.Visible)
                {
                    dc.Drawing.Draw(layer);
                    DrawRelPoints(dc, layer.RelPointList);
                }
            }

            dc.Drawing.Draw(TempFigureList, DrawTools.PEN_TEST_FIGURE);
        }

        public void DrawSelectedItems(DrawContext dc)
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                dc.Drawing.DrawSelected(layer);
            }
        }

        public void DrawSubItems(DrawContext dc)
        {
            //DrawSelectedItems(dc);
            //Drawer.drawCursorScrn(dc, mSnapScrnPoint);

            dc.Drawing.DrawLastPointMarker(
                DrawTools.PEN_LAST_POINT_MARKER, FreeDownPoint);

            if (mObjDownPoint != null)
            {
                dc.Drawing.DrawLastPointMarker(
                    DrawTools.PEN_LAST_POINT_MARKER2, mObjDownPoint.Value);
            }
        }

        public void DrawRelPoints(DrawContext dc, List<CadRelativePoint> list)
        {
            foreach (CadRelativePoint rp in list)
            {
                rp.draw(dc);
            }
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

        private void NextState(DrawContext dc)
        {
            if (State == States.CREATING)
            {
                if (ContinueCreate)
                {
                    CadFigure.Types type = CreatingFigure.Type;
                    CreatingFigure = null;
                    startCreateFigure(type, dc);
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

        private void SetPointInCreating(DrawContext dc, CadPoint p)
        {
            CreatingFigure.addPointInCreating(dc, p);

            CadFigure.States state = CreatingFigure.State;

            if (state == CadFigure.States.FULL)
            {
                CreatingFigure.endCreate(dc);

                CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, CreatingFigure.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(CreatingFigure);

                NextState(dc);
            }
            else if (state == CadFigure.States.ENOUGH)
            {
                CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, CreatingFigure.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(CreatingFigure);
            }
            else if (state == CadFigure.States.CONTINUE)
            {
                CadOpe ope = CadOpe.getAddPointOpe(
                    CurrentLayer.ID,
                    CreatingFigure.ID,
                    CreatingFigure.PointCount - 1,
                    ref p
                    );

                mHistoryManager.foward(ope);
            }
        }

        private void ClearSelection()
        {
            mSelList.clear();
            mSelectedSegs.Clear();

            foreach (CadLayer layer in mDB.LayerList)
            {
                layer.clearSelectedFlags();
            }
        }

        private HashSet<uint> SelListToIDSet()
        {
            HashSet<uint> idSet = new HashSet<uint>();

            mSelList.List.ForEach(a =>
            {
                if (!idSet.Contains(a.FigureID))
                {
                    idSet.Add(a.FigureID);
                }
            });

            return idSet;
        }

        public List<uint> GetSelectedFigIDList()
        {
            List<uint> idList = new List<uint>();

            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    foreach (CadPoint p in fig.PointList)
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

        private void StartEdit()
        {
            EditIdList = GetSelectedFigIDList();

            foreach (uint id in EditIdList)
            {
                CadFigure fig = mDB.getFigure(id);
                if (fig != null)
                {
                    fig.startEdit();
                }
            }
        }

        private void EndEdit()
        {
            DiffDataList ddl = new DiffDataList();

            List<uint> figIDList = EditIdList;

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.getFigure(id);
                if (fig != null)
                {
                    DiffData dd = fig.endEdit();

                    if (dd != null)
                    {
                        ddl.DiffDatas.Add(dd);
                    }
                }
            }

            if (ddl.DiffDatas.Count > 0)
            {
                CadOpeList root = CadOpe.getListOpe();

                CadOpeList ropeList = RemoveInvalidRelPoints();
                root.OpeList.Add(ropeList);

                CadOpe ope = CadOpe.getDiffOpe(ddl);
                root.OpeList.Add(ope);

                CadOpeList fopeList = RemoveInvalidFigure();
                root.OpeList.Add(fopeList);

                mHistoryManager.foward(root);
            }
            else
            {
                CadOpeList ropeList = RemoveInvalidRelPoints();
                mHistoryManager.foward(ropeList);
            }

            UpdateSelectItemPoints();
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

            mSelList.List.RemoveAll(a => removeSels.Contains(a));


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
                        CadOpe ope = CadOpe.getRemoveFigureOpe(layer, fig.ID);
                        opeList.OpeList.Add(ope);

                        layer.removeFigureByIndex(i);
                    }
                }
            }

            return opeList;
        }

        private void MoveSelectedPoints(DrawContext dc, CadPoint delta)
        {
            List<uint> figIDList = GetSelectedFigIDList();

            //delta.z = 0;

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.getFigure(id);
                if (fig != null)
                {
                    fig.moveSelectedPoints(dc, delta);
                }
            }

            UpdateRelPoints();
        }

        private void RemoveSelectedPoints()
        {
            List<uint> figIDList = GetSelectedFigIDList();
            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.getFigure(id);
                fig.removeSelected();
            }
        }

        private void UpdateRelPoints()
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                UpdateRelPoints(layer.RelPointList);
            }
        }

        private void UpdateRelPoints(List<CadRelativePoint> list)
        {
            foreach (CadRelativePoint rp in list)
            {
                CadFigure figA = mDB.getFigure(rp.FigureIdA);
                CadFigure figB = mDB.getFigure(rp.FigureIdB);

                if (figA == null || figB == null)
                {
                    continue;
                }

                rp.update(figA, figB);
            }
        }

        private void MarkRemoveSelectedRelPoints()
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                MarkRemoveSelectedRelPoints(layer);
            }
        }


        private void MarkRemoveSelectedRelPoints(CadLayer layer)
        {
            List<CadRelativePoint> list = layer.RelPointList;
            int i = list.Count - 1;

            for (; i >= 0; i--)
            {
                CadRelativePoint rp = list[i];

                if (!rp.Selected)
                {
                    continue;
                }

                rp.RemoveMark = true;
            }
        }

        private CadOpeList RemoveInvalidRelPoints()
        {
            CadOpeList opeList = new CadOpeList();

            foreach (CadLayer layer in mDB.LayerList)
            {
                CadOpeList subList = RemoveInvalidRelPointsWithLayer(layer);
                opeList.OpeList.Add(subList);
            }

            return opeList;
        }

        private CadOpeList RemoveInvalidRelPointsWithLayer(CadLayer layer)
        {
            List<CadRelativePoint> list = layer.RelPointList;

            CadOpeList opeList = new CadOpeList();

            CadOpe ope = null;

            int i = list.Count - 1;

            for (; i >= 0; i--)
            {
                CadRelativePoint rp = list[i];

                if (rp.RemoveMark)
                {
                    ope = CadOpe.getRemoveRelPointOpe(layer, rp);
                    opeList.OpeList.Add(ope);

                    list.RemoveAt(i);
                    continue;
                }


                CadFigure figA = layer.getFigureByID(rp.FigureIdA);
                CadFigure figB = layer.getFigureByID(rp.FigureIdB);

                if (
                    (figA != null && figA.PointList.Count > rp.IndexA) &&
                    (figB != null && figB.PointList.Count > rp.IndexB)
                    )
                {
                    continue;
                }

                ope = CadOpe.getRemoveRelPointOpe(layer, rp);
                opeList.OpeList.Add(ope);

                list.RemoveAt(i);
            }

            return opeList;
        }
        #endregion

        #region "Copy and paste"
        public void Copy(DrawContext dc)
        {
            CopyFigures();
        }

        public void Paste(DrawContext dc)
        {
            PasteFigure();
            Draw(dc);
        }

        public void CopyFigures()
        {
            List<uint> figIdList = GetSelectedFigIDList();

            var figList = new List<CadFigure>();

            figIdList.ForEach(id =>
            {
                CadFigure fig = mDB.getFigure(id);
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
                CadPoint pp = default(CadPoint);

                pp = FreeDownPoint;

                Log.d("paste");
                List<CadFigure> list = (List<CadFigure>)Clipboard.GetData("List.CadFiguer");

                CadRect cr = CadUtil.getContainsRect(list);

                CadPoint d = pp - cr.p0;

                d.z = 0;

                CadOpeList opeRoot = CadOpe.getListOpe();

                foreach (CadFigure fig in list)
                {
                    fig.moveAllPoints(d);
                    mDB.addFigure(fig);
                    CurrentLayer.addFigure(fig);

                    CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, fig.ID);
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


        public void AddLayer(string name)
        {
            CadLayer layer = mDB.newLayer();

            layer.Name = name;

            CurrentLayer = layer;

            mDB.LayerList.Add(layer);

            NotifyLayerInfo();

            InteractOut.print("Layer added.  Name:" + layer.Name + " ID:" + layer.ID);
        }

        public void RemoveLayer(uint id)
        {
            if (mDB.LayerList.Count == 1)
            {
                return;
            }

            CadLayer layer = mDB.getLayer(id);

            if (layer == null)
            {
                return;
            }

            int nextCurrentIdx = -1;

            if (CurrentLayer.ID == id)
            {
                nextCurrentIdx = mDB.layerIndex(CurrentLayer.ID);
            }

            mDB.removeLayer(id);

            if (nextCurrentIdx >= 0)
            {
                if (nextCurrentIdx > mDB.LayerList.Count-1)
                {
                    nextCurrentIdx = mDB.LayerList.Count - 1;
                }

                CurrentLayer = mDB.LayerList[nextCurrentIdx];
            }

            NotifyLayerInfo();
            InteractOut.print("Layer removed.  Name:" + layer.Name + " ID:" + layer.ID);
        }
    }
}
