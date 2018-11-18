using System;
using System.Collections.Generic;
using System.Drawing;
using CadDataTypes;

namespace Plotter
{
    public class SelectList
    {
        private List<MarkPoint> mList = new List<MarkPoint>();

        private HashSet<ulong> mSet = new HashSet<ulong>();

        public List<MarkPoint> List
        {
            get { return mList; }
        }

        public int RemoveAll(Predicate<MarkPoint> match)
        {
            return mList.RemoveAll(match);
        }

        public void ForEach(Action<MarkPoint> action)
        {
            mList.ForEach(action);
        }

        public void add(CadLayer layer, CadFigure fig, int pointIndex)
        {
            MarkPoint item = new MarkPoint();

            item.Layer = layer;
            item.Figure = fig;
            item.PointIndex = pointIndex;

            add(item);
        }

        public void add(CadLayer layer, CadFigure fig)
        {
            for (int idx = 0; idx < fig.PointCount; idx++)
            {
                add(layer, fig, idx);
            }
        }

        public void add(MarkPoint mp)
        {
            if (mSet.Contains(mp.Hash))
            {
                return;
            }

            mSet.Add(mp.Hash);
            mList.Add(mp);
        }

        public void add(MarkSegment ms)
        {
            add(ms.Layer, ms.Figure, ms.PtIndexA);
            add(ms.Layer, ms.Figure, ms.PtIndexB);
        }

        public void add(CadLayer layer, CadFigure fig, int a, int b)
        {
            int si = Math.Min(a, b);
            int ei = Math.Max(a, b);

            for (int i = si; i <= ei; i++)
            {
                add(layer, fig, i);
            }
        }

        public void clear()
        {
            mSet.Clear();
            mList.Clear();
        }
    }

    public class SelectSegmentList
    {
        private List<MarkSegment> mList = new List<MarkSegment>();

        public List<MarkSegment> List { get { return mList; } }

        public MarkSegment LastSel { get; set; }

        public void Add(MarkSegment ms)
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
            LastSel = default(MarkSegment);
        }

        public bool isSelected(MarkSegment ms)
        {
            MarkSegment selItem = mList.Find(a => {
                return a.FigureID == ms.FigureID &&
                a.PtIndexA == ms.PtIndexA &&
                a.PtIndexB == ms.PtIndexB
                ;
            });

            return selItem.FigureID != 0;
        }

        public bool isSelectedFigure(uint figId)
        {
            MarkSegment selItem = mList.Find(a => {
                return a.FigureID == figId;
            });

            return selItem.FigureID != 0;
        }

        public void dump()
        {
            DbgOut.pln("SelectSegmentList {");
            DbgOut.Indent++;
            foreach (MarkSegment ms in mList)
            {
                ms.dump();
            }
            DbgOut.Indent--;
            DbgOut.pln("}");
        }
    }
}