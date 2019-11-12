using OpenTK;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public void ClearAll()
        {
            PageSize = new PaperPageSize();

            mDB.ClearAll();
            HistoryMan.Clear();

            UpdateLayerList();
            UpdateObjectTree(true);
        }

        #region Layer
        public void SelectAllInCurrentLayer()
        {
            foreach (CadFigure fig in CurrentLayer.FigureList)
            {
                fig.Select();
            }
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
                if (nextCurrentIdx > mDB.LayerList.Count - 1)
                {
                    nextCurrentIdx = mDB.LayerList.Count - 1;
                }

                CurrentLayer = mDB.LayerList[nextCurrentIdx];
            }

            UpdateLayerList();
            ItConsole.println("Layer removed.  Name:" + layer.Name + " ID:" + layer.ID);
        }
        #endregion

        public void SelectFigure(uint figID)
        {
            CadFigure fig = DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            fig.Select();
        }

        public void SelectFigureById(uint id, int idx, bool clearSelect = true)
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

            if (idx >= 0)
            {
                fig.SelectPointAt(idx, true);
            }
            else
            {
                fig.Select();
            }

            CurrentFigure = fig;
        }

        public void MovePointsFromStored(List<CadFigure> figList, Vector3d d)
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

        public void Remove()
        {
            StartEdit();

            RemoveSelectedPoints();

            EndEdit();
        }

        public void InsPoint()
        {
            StartEdit();
            if (InsPointToLastSelectedSeg())
            {
                EndEdit();
            }
            else
            {
                AbendEdit();
            }
        }

        public void Copy()
        {
            PlotterClipboard.CopyFiguresAsBin(this);
        }

        public void Paste()
        {
            PlotterClipboard.PasteFiguresAsBin(this);
            UpdateObjectTree(true);
        }
    }
}