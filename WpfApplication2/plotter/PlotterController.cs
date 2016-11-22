using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Plotter
{

    public delegate void StateChanged(PlotterController sender, PlotterController.StateInfo si);

    public delegate void LayerListChanged(PlotterController sender, PlotterController.LayerListInfo layerListInfo);

    public delegate void RequestContextMenu(PlotterController sender, PlotterController.StateInfo si, int x, int y);

    public partial class PlotterController
    {
        const int SnapRange = 6;


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

        public Interaction Interact { set; get; } = new Interaction();

        //private CadLayer mCurrentLayer = null;

        private CadObjectDB mDB = new CadObjectDB();


        public States State
        {
            private set;
            get;
        } = States.SELECT;



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


        private SelectList mSelList = new SelectList();

        private SelectSegmentList mSelectedSegs = new SelectSegmentList();

        private List<uint> EditIdList = new List<uint>();


        public bool ContinueCreate { set; get; } = false;

        public StateChanged StateChanged;

        public RequestContextMenu RequestContextMenu;

        public LayerListChanged mLayerListChanged = (a, b) => { };

        public LayerListChanged LayerListChanged
        {
            set
            {
                mLayerListChanged = value;
                NotifyLayerInfo();
            }
        }

        public PlotterController()
        {
            CadLayer layer = mDB.newLayer();
            mDB.LayerList.Add(layer);
            CurrentLayer = layer;

            mHistoryManager = new HistoryManager(mDB);

            initScrExecutor();

            initHid();
        }


        #region Notify
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

        public void startCreateFigure(CadFigure.Types type)
        {
            State = States.START_CREATE;
            CreatingFigType = type;

            //NotifyStateChange();

            // Creation start when specify the first coordinate.
            // So, at the moment, not yet a creation start.
        }

        public void endCreateFigure()
        {
            CreatingFigType = CadFigure.Types.NONE;

            State = States.SELECT;

            if (CreatingFigure != null)
            {
                CreatingFigure.endCreate();
                CreatingFigure = null;
            }

            NotifyStateChange();
        }

        public void endCreateFigureState(DrawContext g)
        {
            nextState();
        }

        public void setCurrentLayer(uint id)
        {
            mDB.CurrentLayerID = id;
        }

        #region "undo redo"
        public void undo(DrawContext dc)
        {
            clearSelection();
            mHistoryManager.undo();

            updateRelPoints(CurrentLayer.RelPointList);

            Drawer.clear(dc);
            draw(dc);
        }

        public void redo(DrawContext dc)
        {
            clearSelection();
            mHistoryManager.redo();

            updateRelPoints(CurrentLayer.RelPointList);

            Drawer.clear(dc);
            draw(dc);
        }
        #endregion


        #region "Draw methods"
        public void clear(DrawContext dc)
        {
            if (dc == null) return;
            Drawer.clear(dc);
        }

        public void draw(DrawContext dc)
        {
            if (dc == null) return;

            Drawer.drawAxis(dc);
            Drawer.drawPageFrame(dc);

            foreach (CadLayer layer in mDB.LayerList)
            {
                if (layer.Visible)
                {
                    Drawer.draw(dc, layer);
                    drawRelPoints(dc, layer.RelPointList);
                }
            }
        }

        public void drawSelectedItems(DrawContext dc)
        {
            Drawer.drawSelected(dc, CurrentLayer);
        }

        public void drawSubItems(DrawContext dc)
        {
            drawSelectedItems(dc);

            Drawer.drawCursor(dc, mSnapCursorPos);

            if (mFreeDownPoint != null)
            {
                Drawer.drawLastPointMarker(
                    dc, dc.Tools.LastPointMarkerPen1, mFreeDownPoint.Value);
            }

            if (mObjDownPoint != null)
            {
                Drawer.drawLastPointMarker(
                    dc, dc.Tools.LastPointMarkerPen2, mObjDownPoint.Value);
            }
        }

        public void drawRelPoints(DrawContext dc, List<CadRelativePoint> list)
        {
            foreach (CadRelativePoint rp in list)
            {
                rp.draw(dc);
            }
        }

        public void print(DrawContext dc)
        {
            Drawer.draw(dc, CurrentLayer);
        }
        #endregion


        //-----------------------------------------------------------------------------------------
        // Edit figure methods

        #region Private editing figure methods

        private void nextState()
        {
            if (State == States.CREATING)
            {
                if (ContinueCreate)
                {
                    CadFigure.Types type = CreatingFigure.Type;
                    CreatingFigure = null;
                    startCreateFigure(type);
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

        private void setPoint(CadPoint p)
        {
            CreatingFigure.addPoint(p);

            CadFigure.States state = CreatingFigure.State;

            if (state == CadFigure.States.FULL)
            {
                CreatingFigure.endCreate();

                CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, CreatingFigure.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(CreatingFigure);

                nextState();
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

        private void clearSelection()
        {
            mSelList.clear();
            mSelectedSegs.Clear();
            CurrentLayer.clearSelectedFlags();
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

        private List<uint> getSelectedFigIDList()
        {
            List<uint> idList = new List<uint>();

            foreach (CadFigure fig in CurrentLayer.FigureList)
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

            return idList;
        }

        private void startEdit()
        {
            EditIdList = getSelectedFigIDList();

            foreach (uint id in EditIdList)
            {
                CadFigure fig = mDB.getFigure(id);
                if (fig != null)
                {
                    fig.startEdit();
                }
            }
        }

        private void endEdit()
        {
            DiffDataList ddl = new DiffDataList();

            ddl.LayerID = CurrentLayer.ID;

            CadLayer layer = mDB.getLayer(CurrentLayer.ID);

            List<uint> figIDList = EditIdList;  //getSelectedFigIDList();

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.getFigure(id);
                if (fig != null)
                {
                    DiffData dd = fig.endEdit();

                    if (dd != null)
                    {
                        Log.d("endEditSelected() fig id=" + dd.FigureID);

                        if (fig.PointCount == 0)
                        {
                            DiffItem diffItem = new DiffItem();
                            diffItem.Type = DiffItem.Types.REMOVE_FIGURE;
                            diffItem.index = layer.getFigureIndex(id);

                            dd.Items.Add(diffItem);

                            layer.removeFigureByID(id);
                        }

                        ddl.DiffDatas.Add(dd);
                    }
                }
            }


            if (ddl.DiffDatas.Count > 0)
            {
                CadOpe ope = CadOpe.getDiffOpe(ddl);

                CadOpeList root = CadOpe.getListOpe();

                root.OpeList.Add(ope);

                List<CadOpe> rope = removeInvalidRelPoints(CurrentLayer);

                CadOpeList branch = CadOpe.getListOpe(rope);

                root.OpeList.Add(branch);


                mHistoryManager.foward(root);
            }
            else
            {
                List<CadOpe> rope = removeInvalidRelPoints(CurrentLayer);
                CadOpeList root = CadOpe.getListOpe(rope);

                mHistoryManager.foward(root);
            }
        }

        private void moveSelectedPoints(CadPoint delta)
        {
            List<uint> figIDList = getSelectedFigIDList();

            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.getFigure(id);
                if (fig != null)
                {
                    fig.moveSelectedPoints(delta);
                }
            }

            updateRelPoints(CurrentLayer.RelPointList);
        }

        private void removeSelectedPoints()
        {
            List<uint> figIDList = getSelectedFigIDList();
            foreach (uint id in figIDList)
            {
                CadFigure fig = mDB.getFigure(id);
                fig.removeSelected();
            }
        }

        private void updateRelPoints(List<CadRelativePoint> list)
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

        private void markRemoveSelectedRelPoints(CadLayer layer)
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

        private List<CadOpe> removeInvalidRelPoints(CadLayer layer)
        {
            List<CadRelativePoint> list = layer.RelPointList;

            List<CadOpe> opeList = new List<CadOpe>();

            CadOpe ope = null;

            int i = list.Count - 1;

            for (; i >= 0; i--)
            {
                CadRelativePoint rp = list[i];

                if (rp.RemoveMark)
                {
                    ope = CadOpe.getRemoveRelPoint(CurrentLayer, rp);
                    opeList.Add(ope);

                    list.RemoveAt(i);
                    continue;
                }


                CadFigure figA = CurrentLayer.getFigureByID(rp.FigureIdA);
                CadFigure figB = CurrentLayer.getFigureByID(rp.FigureIdB);

                if (
                    (figA != null && figA.PointList.Count > rp.IndexA) &&
                    (figB != null && figB.PointList.Count > rp.IndexB)
                    )
                {
                    continue;
                }

                ope = CadOpe.getRemoveRelPoint(CurrentLayer, rp);
                opeList.Add(ope);

                list.RemoveAt(i);
            }

            return opeList;
        }
        #endregion

        #region "Copy and paste"
        public void Copy(DrawContext dc)
        {
            copyFigures();
        }

        public void Paste(DrawContext dc)
        {
            pasteFigure();
            draw(dc);
        }

        public void copyFigures()
        {
            List<uint> figIdList = getSelectedFigIDList();

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

        public void pasteFigure()
        {
            if (Clipboard.ContainsData("List.CadFiguer"))
            {
                CadPoint pp = default(CadPoint);

                if (mFreeDownPoint.HasValue)
                {
                    pp = mFreeDownPoint.Value;
                }

                Log.d("paste");
                List<CadFigure> list = (List<CadFigure>)Clipboard.GetData("List.CadFiguer");

                CadRect cr = CadUtil.getContainsRect(list);

                CadPoint d = pp - cr.p0;

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
        }

        #endregion
    }
}
