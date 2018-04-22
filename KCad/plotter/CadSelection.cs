using System;
using System.Collections.Generic;
using System.Drawing;
using CadDataTypes;

namespace Plotter
{
    public class SelectItem
    {
        public uint LayerID
        {
            get; set;
        }

        public uint FigureID
        {
            get
            {
                return Figure.ID;
            }
        }

        public CadFigure Figure
        {
            get; set;
        }

        public int PointIndex
        {
            get; set;
        }

        public CadVector Point
        {
            get
            {
                return Figure.PointList[PointIndex];
            }
        }

        public SelectItem()
        {
        }

        public SelectItem(MarkPoint mp)
        {
            LayerID = mp.LayerID;
            Figure = mp.Figure;
            PointIndex = mp.PointIndex;
        }

        public SelectItem(SelectItem src)
        {
            LayerID = src.LayerID;
            Figure = src.Figure;
            PointIndex = src.PointIndex;
        }

        public void dump()
        {
            DebugOut.println("SelectItem {");
            DebugOut.Indent++;
            DebugOut.println("LayerID:" + LayerID.ToString());
            DebugOut.println("FigureID:" + FigureID.ToString());
            DebugOut.println("PointIndex:" + PointIndex.ToString());
            DebugOut.Indent--;
            DebugOut.println("}");
        }

        public bool update()
        {
            if (Figure == null)
            {
                return true;
            }

            if (PointIndex >= Figure.PointList.Count)
            {
                return false;
            }

            return true;
        }
    }

    public class SelectList
    {
        private List<SelectItem> mList = new List<SelectItem>();

        public List<SelectItem> List
        {
            get { return mList; }
        }

        public int RemoveAll(Predicate<SelectItem> match)
        {
            return mList.RemoveAll(match);
        }

        public void ForEach(Action<SelectItem> action)
        {
            mList.ForEach(action);
        }

        public void add(uint layerID, CadFigure fig, int pointIndex)
        {
            SelectItem f = null;

            if (fig != null)
            {
                f = find(fig.ID, pointIndex);
            }

            if (f != null)
            {
                return;
            }

            SelectItem item = new SelectItem();

            item.LayerID = layerID;
            item.Figure = fig;
            item.PointIndex = pointIndex;

            mList.Add(item);
        }

        public void add(uint layerID, CadFigure fig)
        {
            for (int idx = 0; idx < fig.PointCount; idx++)
            {
                add(layerID, fig, idx);
            }
        }

        public void add(MarkPoint mp)
        {
            SelectItem f = null;

            f = find(mp.FigureID, mp.PointIndex);

            if (f == null)
            {
                mList.Add(new SelectItem(mp));
            }
        }

        public void add(MarkSeg ms)
        {
            add(ms.LayerID, ms.Figure, ms.PtIndexA);
            add(ms.LayerID, ms.Figure, ms.PtIndexB);
        }

        public void add(uint layerID, CadFigure fig, int a, int b)
        {
            int si = Math.Min(a, b);
            int ei = Math.Max(a, b);

            for (int i = si; i <= ei; i++)
            {
                add(layerID, fig, i);
            }
        }

        public SelectItem find(uint figureId, int pointIndex)
        {
            return mList.Find((a) => (a.FigureID == figureId && a.PointIndex == pointIndex));
        }

        public void clear()
        {
            mList.Clear();
        }

        public bool isSelected(MarkPoint mp)
        {
            SelectItem selItem = mList.Find(a => {
                return a.FigureID == mp.FigureID &&
                a.PointIndex == mp.PointIndex;
            });

            return selItem != null;
        }

        public bool isSelectedFigure(uint figId)
        {
            SelectItem selItem = mList.Find(a => {
                return a.FigureID == figId;
            });

            return selItem != null;
        }

        public void dump()
        {
            DebugOut.println("SelectList {");
            DebugOut.Indent++;
            foreach (SelectItem item in mList)
            {
                item.dump();
            }
            DebugOut.Indent--;
            DebugOut.println("}");
        }
    }

    public class SelectSegmentList
    {
        private List<MarkSeg> mList = new List<MarkSeg>();

        public List<MarkSeg> List { get { return mList; } }

        public MarkSeg LastSel { get; set; }

        public void Add(MarkSeg ms)
        {
            LastSel = ms;

            int fnd;

            fnd = mList.FindIndex(a =>
            {
                bool ret =
                    a.FigureID == ms.FigureID &&
                    a.PtIndexA == ms.PtIndexA &&
                    a.PtIndexB == ms.PtIndexB;
                return ret;
            });

            if (fnd == -1)
            {
                mList.Add(ms);
            }
        }

        public void Clear()
        {
            mList.Clear();
            LastSel = default(MarkSeg);
        }

        public bool isSelected(MarkSeg ms)
        {
            MarkSeg selItem = mList.Find(a => {
                return a.FigureID == ms.FigureID &&
                a.PtIndexA == ms.PtIndexA &&
                a.PtIndexB == ms.PtIndexB
                ;
            });

            return selItem.FigureID != 0;
        }

        public bool isSelectedFigure(uint figId)
        {
            MarkSeg selItem = mList.Find(a => {
                return a.FigureID == figId;
            });

            return selItem.FigureID != 0;
        }

        public void dump()
        {
            DebugOut.println("SelectSegmentList {");
            DebugOut.Indent++;
            foreach (MarkSeg ms in mList)
            {
                ms.dump();
            }
            DebugOut.Indent--;
            DebugOut.println("}");
        }
    }
}