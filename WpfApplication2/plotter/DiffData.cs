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

        public void undo(CadObjectDB db)
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
                }
            }
        }

        public void redo(CadObjectDB db)
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
        public List<DiffData> DiffDatas = new List<DiffData>();

        public void undo(CadObjectDB db)
        {
            foreach (DiffData dd in DiffDatas)
            {
                dd.undo(db);
            }
        }

        public void redo(CadObjectDB db)
        {
            foreach (DiffData dd in DiffDatas)
            {
                dd.redo(db);
            }
        }
    }
    #endregion
}
