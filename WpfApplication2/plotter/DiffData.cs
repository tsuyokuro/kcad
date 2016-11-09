using System;
using System.Collections.Generic;

namespace Plotter
{

    #region "Figure level diff data"
    public class DiffItem
    {
        public enum Types : byte
        {
            NONE = 0,
            ADD = 1,
            REMOVE = 2,
            MODIFY = 3,

            // TODO: In the future, manage figure by FigureDiffData 
            //ADD_FIGURE = 4,
            REMOVE_FIGURE = 5,
        }

        public int index;
        public Types Type;

        public CadPoint P0;
        public CadPoint P1;
    }

    public class DiffData
    {
        public uint FigureID = 0;
        public List<DiffItem> Items = new List<DiffItem>();

        public void undo(CadObjectDB db, CadLayer layer)
        {
            CadFigure fig = db.getFigure(FigureID);

            if (fig == null)
            {
                Log.e("ERROR DiffData." + nameof(undo) + " fig is null");
                return;
            }
            
            foreach (DiffItem dd in Items)
            {
                switch (dd.Type)
                {
                    case DiffItem.Types.ADD:
                        {
                            fig.removePointAt(dd.index);
                        }
                        break;

                    case DiffItem.Types.REMOVE:
                        {
                            fig.insertPointAt(dd.index, dd.P0);
                        }
                        break;

                    case DiffItem.Types.MODIFY:
                        {
                            fig.setPointAt(dd.index, dd.P0);
                        }

                        break;

                    /*
                    case DiffItem.Types.ADD_FIGURE:
                        {
                            layer.removeFigureByID(FigureID);
                        }
                        break;
                    */
                    case DiffItem.Types.REMOVE_FIGURE:
                        {
                            layer.addFigure(fig);
                        }
                        break;
                }
            }
        }

        public void redo(CadObjectDB db, CadLayer layer)
        {
            CadFigure fig = db.getFigure(FigureID);

            if (fig == null)
            {
                Log.e("ERROR DiffData." + nameof(redo) + " fig is null");
                return;
            }

            // TODO: Use revers iterator
            List<DiffItem> rlist = new List<DiffItem>(Items);
            rlist.Reverse();

            foreach (DiffItem dd in rlist)
            {
                switch (dd.Type)
                {
                    case DiffItem.Types.ADD:
                        {
                            fig.insertPointAt(dd.index, dd.P0);
                        }
                        break;

                    case DiffItem.Types.REMOVE:
                        {
                            fig.removePointAt(dd.index);
                        }

                        break;

                    case DiffItem.Types.MODIFY:
                        {
                            fig.setPointAt(dd.index, dd.P1);
                        }

                        break;
                    /*
                    case DiffItem.Types.ADD_FIGURE:
                        {
                            layer.addFigure(fig);
                        }
                        break;
                    */
                    case DiffItem.Types.REMOVE_FIGURE:
                        {
                            layer.removeFigureByID(FigureID);
                        }
                        break;
                }
            }

        }

        public static DiffData create(CadFigure fig)
        {
            uint figID = fig.ID;
            List<CadPoint> oldList = fig.StoreList;
            List<CadPoint> newList = fig.PointList;

            if (oldList == null)
            {
                return null;
            }


            DiffData diff = new DiffData();

            diff.FigureID = figID;

            int ocnt = oldList.Count;
            int ncnt = newList.Count;

            int cnt = Math.Min(ocnt, ncnt);

            int i = 0;
            for (; i < cnt; i++)
            {
                CadPoint op = oldList[i];
                CadPoint np = newList[i];

                if (!op.dataEquals(np))
                {
                    DiffItem item = new DiffItem();

                    item.Type = DiffItem.Types.MODIFY;
                    item.index = i;
                    item.P0 = op;
                    item.P1 = np;

                    diff.Items.Add(item);
                }
            }

            if (ncnt < ocnt)
            {
                for (; i < ocnt; i++)
                {
                    CadPoint op = oldList[i];

                    DiffItem item = new DiffItem();

                    item.Type = DiffItem.Types.REMOVE;
                    item.index = i;
                    item.P0 = op;

                    diff.Items.Add(item);
                }
            }
            else if (ncnt > ocnt)
            {
                for (; i < ncnt; i++)
                {
                    CadPoint np = newList[i];

                    DiffItem item = new DiffItem();

                    item.Type = DiffItem.Types.ADD;
                    item.index = i;
                    item.P0 = np;

                    diff.Items.Add(item);
                }
            }

            if (diff.Items.Count == 0)
            {
                return null;
            }

            return diff;
        }
    }

    public class DiffDataList
    {
        public uint LayerID;
        public List<DiffData> DiffDatas = new List<DiffData>();

        public void undo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);

            foreach (DiffData dd in DiffDatas)
            {
                dd.undo(db, layer);
            }
        }

        public void redo(CadObjectDB db)
        {
            CadLayer layer = db.getLayer(LayerID);

            foreach (DiffData dd in DiffDatas)
            {
                dd.redo(db, layer);
            }
        }
    }
    #endregion

    #region "Layer level diff data"
    public class LayerDiffItem
    {
        public enum Types : byte
        {
            NONE = 0,
            ADD = 1,
            REMOVE = 2,
            REPLACE = 3,
        }

        public int index;
        public Types Type;

        public CadFigure Fig0;
        public CadFigure Fig1;
    }

    public class LayerDiffData
    {
        public uint LayerID = 0;
        public List<LayerDiffItem> Items = new List<LayerDiffItem>();

        public static LayerDiffData create(CadLayer layer)
        {
            uint layerID = layer.ID;
            List<CadFigure> oldList = layer.StoreFigureList;
            List<CadFigure> newList = layer.FigureList;

            if (oldList == null)
            {
                return null;
            }

            LayerDiffData diff = new LayerDiffData();

            diff.LayerID = layerID;

            int ocnt = oldList.Count;
            int ncnt = newList.Count;

            int cnt = Math.Min(ocnt, ncnt);

            int i = 0;
            for (; i < cnt; i++)
            {
                CadFigure ofig = oldList[i];
                CadFigure nfig = newList[i];

                if (ofig.ID != nfig.ID)
                {
                    LayerDiffItem item = new LayerDiffItem();

                    item.Type = LayerDiffItem.Types.REPLACE;
                    item.index = i;
                    item.Fig0 = ofig;
                    item.Fig0 = nfig;

                    diff.Items.Add(item);
                }
            }

            if (ncnt < ocnt)
            {
                for (; i < ocnt; i++)
                {
                    CadFigure ofig = oldList[i];

                    LayerDiffItem item = new LayerDiffItem();

                    item.Type = LayerDiffItem.Types.REMOVE;
                    item.index = i;
                    item.Fig0 = ofig;

                    diff.Items.Add(item);
                }
            }
            else if (ncnt > ocnt)
            {
                for (; i < ncnt; i++)
                {
                    CadFigure nfig = newList[i];

                    LayerDiffItem item = new LayerDiffItem();

                    item.Type = LayerDiffItem.Types.ADD;
                    item.index = i;
                    item.Fig0 = nfig;

                    diff.Items.Add(item);
                }
            }

            if (diff.Items.Count == 0)
            {
                return null;
            }

            return diff;
        }
    }
    #endregion
}
