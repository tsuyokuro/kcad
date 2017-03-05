using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public class SelectItem
    {
        public MarkPoint mMarkPoint;

        public uint LayerID
        {
            get { return mMarkPoint.LayerID; }
            set { mMarkPoint.LayerID = value; }
        }

        public uint FigureID
        {
            get { return mMarkPoint.FigureID; }
        }

        public CadFigure Figure
        {
            get { return mMarkPoint.Figure; }
            set { mMarkPoint.Figure = value; }
        }

        public int PointIndex
        {
            get { return mMarkPoint.PointIndex; }
            set { mMarkPoint.PointIndex = value; }
        }

        public CadPoint Point
        {
            get { return mMarkPoint.Point; }
            set { mMarkPoint.Point = value; }
        }

        public SelectItem()
        {
            mMarkPoint = default(MarkPoint);
        }

        public SelectItem(MarkPoint mp)
        {
            mMarkPoint = mp;
        }

        public SelectItem(SelectItem src)
        {
            mMarkPoint = src.mMarkPoint;
        }

        public void dump(DebugOut dout)
        {
            dout.println("SelectItem {");
            dout.Indent++;
            dout.println("LayerID:" + LayerID.ToString());
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PointIndex:" + PointIndex.ToString());
            dout.Indent--;
            dout.println("}");
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


            Point = Figure.PointList[PointIndex];

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

        public void add(uint layerID, CadFigure fig, int pointIndex, CadPoint point)
        {
            SelectItem f = null;

            if (fig != null)
            {
                f = find(fig.ID, pointIndex);
            }

            if (f != null)
            {
                //f.Point = point;
                return;
            }

            SelectItem item = new SelectItem();

            item.LayerID = layerID;
            item.Figure = fig;
            item.PointIndex = pointIndex;

            item.Point = point;
            mList.Add(item);
        }

        public void add(uint layerID, CadFigure fig)
        {
            for (int idx = 0; idx < fig.PointCount; idx++)
            {
                add(layerID, fig, idx, fig.getPointAt(idx));
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
            add(ms.LayerID, ms.Figure, ms.PtIndexA, ms.pA);
            add(ms.LayerID, ms.Figure, ms.PtIndexB, ms.pB);
        }

        public void add(uint layerID, CadFigure fig, int a, int b)
        {
            int si = Math.Min(a, b);
            int ei = Math.Max(a, b);

            for (int i = si; i <= ei; i++)
            {
                CadPoint p = fig.getPointAt(i);
                add(layerID, fig, i, p);
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

        public void dump(DebugOut dout)
        {
            dout.println("SelectList {");
            dout.Indent++;
            foreach (SelectItem item in mList)
            {
                item.dump(dout);
            }
            dout.Indent--;
            dout.println("}");
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

        public void dump(DebugOut dout)
        {
            dout.println("SelectSegmentList {");
            dout.Indent++;
            foreach (MarkSeg ms in mList)
            {
                ms.dump(dout);
            }
            dout.Indent--;
            dout.println("}");
        }
    }
}