using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plotter
{
    public struct PointSpec
    {
        public uint FigureID;
        public int PointIndex;
        public CadPoint Point;
    }


    public class SelectItem
    {
        //public uint LayerID = 0;

        PointSpec mPointSpec;

        public uint FigureID
        {
            get { return mPointSpec.FigureID; }
            set { mPointSpec.FigureID = value; }
        }

        public int PointIndex
        {
            get { return mPointSpec.PointIndex; }
            set { mPointSpec.PointIndex = value; }
        }

        public CadPoint Point
        {
            get { return mPointSpec.Point; }
            set { mPointSpec.Point = value; }
        }


        public SelectItem()
        {
            mPointSpec = default(PointSpec);
        }

        public SelectItem(SelectItem src)
        {
            //LayerID = src.LayerID;
            FigureID = src.FigureID;

            PointIndex = src.PointIndex;
            Point = src.Point;
        }

        public void dump(DebugOut dout)
        {
            dout.println("SelectItem {");
            dout.Indent++;
            dout.println("FigureID:" + FigureID.ToString());
            dout.println("PointIndex:" + PointIndex.ToString());
            mPointSpec.Point.dump(dout);
            dout.Indent--;
            dout.println("}");
        }
    }

    public class SelectList
    {
        private List<SelectItem> mList = new List<SelectItem>();

        public List<SelectItem> List
        {
            get { return mList; }
        }

        public void add(uint figureId, int pointIndex, CadPoint point)
        {
            SelectItem f = find(figureId, pointIndex);

            if (f != null)
            {
                f.Point = point;
                return;
            }

            SelectItem item = new SelectItem();

            item.FigureID = figureId;
            item.PointIndex = pointIndex;

            item.Point = point;
            mList.Add(item);
        }

        public void add(CadFigure fig)
        {
            for (int idx = 0; idx < fig.PointCount; idx++)
            {
                add(fig.ID, idx, fig.getPointAt(idx));
            }
        }

        public void add(MarkPoint mp)
        {
            add(mp.FigureID, mp.PointIndex, mp.Point);
        }

        public void add(MarkSeg ms)
        {
            add(ms.FigureID, ms.PtIndexA, ms.pA);
            add(ms.FigureID, ms.PtIndexB, ms.pB);
        }

        public void add(CadFigure fig, int a, int b)
        {
            int si = Math.Min(a, b);
            int ei = Math.Max(a, b);

            for (int i = si; i <= ei; i++)
            {
                CadPoint p = fig.getPointAt(i);
                add(fig.ID, i, p);
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