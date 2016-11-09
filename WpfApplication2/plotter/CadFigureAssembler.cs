using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class CadFigureAssembler
    {
        protected CadObjectDB DB;
        protected CadLayer Layer;
        protected Result ProcResult = new Result();

        public class Result
        {
            public List<CadFigure> AddList = new List<CadFigure>();
            public List<CadFigure> RemoveList = new List<CadFigure>();

            public bool isValid()
            {
                return AddList.Count > 0 || RemoveList.Count > 0;
            }

            public void clear()
            {
                AddList.Clear();
                RemoveList.Clear();
            }
        }

        public CadFigureAssembler(CadObjectDB db, CadLayer layer)
        {
            DB = db;
            Layer = layer;
        }
    }

    class CadFigureCutter : CadFigureAssembler
    {
        public CadFigureCutter(CadObjectDB db, CadLayer layer) : base(db, layer)
        {
        }

        public Result cut(List<SelectItem> selList)
        {
            var sels = (from a in selList orderby a.FigureID, a.PointIndex ascending select a);

            uint figId = 0;
            CadFigure fig = null;
            int pcnt = 0;
            int sp = -1;
            int cp = -1;
            int num = 0;

            List<SelectItem> figSet = new List<SelectItem>();

            VoidFunc endFig = () =>
            {
                num = pcnt - sp;

                CadFigure nfig = null;

                if (fig.Closed)
                {
                    if (num >= 1)
                    {
                        nfig = DB.newFigure(CadFigure.Types.POLY_LINES);
                        nfig.addPoints(fig.PointList, sp, num);

                        CadPoint t = fig.getPointAt(0);
                        nfig.addPoint(t);
                    }
                }
                else
                {
                    if (num >= 2)
                    {
                        nfig = DB.newFigure(CadFigure.Types.POLY_LINES);
                        nfig.addPoints(fig.PointList, sp, num);
                    }
                }

                if (nfig != null)
                {
                    ProcResult.AddList.Add(nfig);
                }
            };

            foreach (SelectItem si in sels)
            {
                if (si.FigureID != figId)
                {
                    if (sp != -1)
                    {
                        endFig();
                    }

                    figId = si.FigureID;
                    fig = DB.getFigure(figId);
                    pcnt = fig.PointCount;
                    sp = -1;
                }

                cp = si.PointIndex;

                if (cp == 0)
                {
                    continue;
                }

                if (sp == -1)
                {
                    figSet.Add(si);
                    sp = 0;
                }

                num = cp - sp + 1;

                if (sp + num <= pcnt)
                {
                    CadFigure nfig = DB.newFigure(CadFigure.Types.POLY_LINES);
                    nfig.addPoints(fig.PointList, sp, num);

                    ProcResult.AddList.Add(nfig);
                }

                sp = cp;
            }

            if (sp != -1)
            {
                endFig();
            }

            foreach (SelectItem si in figSet)
            {
                CadFigure rmfig = DB.getFigure(si.FigureID);

                if (rmfig != null)
                {
                    ProcResult.RemoveList.Add(rmfig);
                }
            }

            return ProcResult;
        }

    }

    class CadFigureBonder : CadFigureAssembler
    {
        private List<CadFigure> Work;
        private List<SelectItem> SelectList;
        private List<Item> ItemList;

        public CadFigureBonder(CadObjectDB db, CadLayer layer) : base(db, layer)
        {
            // Copy figure list to Work
            Work = new List<CadFigure>(layer.FigureList);
        }

        private class Item
        {
            public uint FigureID;
            public int PointIndex;
            public CadPoint Point;

            public void set(SelectItem si)
            {
                FigureID = si.FigureID;
                PointIndex = si.PointIndex;
                Point = si.Point;
            }
        }

        private class BondInfo
        {
            public CadFigure BondedFigure;

            public Item item;

            public CadFigure figA;
            public int indexA;

            public CadFigure figB;
            public int indexB;
        }

        public Result bond(List<SelectItem> selList)
        {
            SelectList = selList;

            // Collect endpoint (first and last point) items
            // to ItemList 
            collectEndPoint(selList);

            // joint end points
            bondMain();

            return ProcResult;
        }

        private void collectEndPoint(List<SelectItem> selList)
        {
            ItemList = new List<Item>();

            foreach (SelectItem si in selList)
            {
                if (si.FigureID == 0) continue;


                // falldown if index is 0 or last
                if (si.PointIndex != 0)
                {
                    CadFigure fig = DB.getFigure(si.FigureID);

                    if (si.PointIndex != fig.PointCount - 1)
                    {
                        continue;
                    }
                }


                Item item = new Item();
                item.set(si);
                ItemList.Add(item);
            }
        }

        private void bondMain()
        {
            while (ItemList.Count > 0)
            {
                BondInfo bondInfo = null;

                Item item = ItemList[0];

                foreach (CadFigure fig1 in Work)
                {
                    if (fig1.ID == item.FigureID)
                    {
                        continue;
                    }

                    bondInfo = bondFigure(item, fig1);

                    if (bondInfo != null)
                    {
                        break;
                    }
                }

                if (bondInfo != null)
                {
                    updateItemList(bondInfo);

                    ProcResult.AddList.Add(bondInfo.BondedFigure);
                    ProcResult.RemoveList.Add(bondInfo.figA);
                    ProcResult.RemoveList.Add(bondInfo.figB);

                    Work.RemoveAll(a =>
                        a.ID == bondInfo.figA.ID ||
                        a.ID == bondInfo.figB.ID
                        );


                    Work.Add(bondInfo.BondedFigure);
                }
                else
                {
                    ItemList.RemoveAll((a) =>
                        a.FigureID == item.FigureID &&
                        a.PointIndex == item.PointIndex
                        );
                }
            }
        }

        private Item findItem(CadPoint p)
        {
            foreach (Item item in ItemList)
            {
                if (p.coordEquals(item.Point))
                {
                    return item;
                }
            }

            return null;
        }


        /**
         * replace Item in ItemList to bondedFigure
         *
         */
        private void updateItemList(BondInfo bi)
        {
            int pcnt = bi.BondedFigure.PointCount;
            CadPoint head = bi.BondedFigure.getPointAt(0);
            CadPoint tail = bi.BondedFigure.getPointAt(pcnt - 1);

            Item item = null;

            Item litem = null;
            item = findItem(head);

            if (item != null)
            {
                litem = new Item();
                litem.FigureID = bi.BondedFigure.ID;
                litem.PointIndex = 0;
                litem.Point = head;

                ItemList.RemoveAll((a) =>
                    a.Point.coordEquals(head)
                    );

                ItemList.Add(litem);
            }

            Item ritem = null;
            item = findItem(tail);

            if (item != null)
            {
                ritem = new Item();
                ritem.FigureID = bi.BondedFigure.ID;
                ritem.PointIndex = bi.BondedFigure.PointCount - 1;
                ritem.Point = tail;

                ItemList.RemoveAll((a) =>
                    a.Point.coordEquals(tail)
                    );

                ItemList.Add(ritem);
            }

            // Remove item that is used for bond
            ItemList.RemoveAll((a) =>
                a.FigureID == bi.item.FigureID &&
                a.PointIndex == bi.item.PointIndex
                );
        }

        private BondInfo bondFigure(Item item, CadFigure fig1)
        {
            CadFigure fig0 = DB.getFigure(item.FigureID);
            int idx0 = item.PointIndex;

            if (fig0.ID == fig1.ID)
            {
                return null;
            }

            // Is point index first or last ?
            if ((idx0 != 0) && (idx0 != fig0.PointCount - 1))
            {
                return null;
            }

            CadPoint p0 = fig0.getPointAt(idx0);
            CadPoint p1 = fig1.getPointAt(0);


            int idx1 = -1;

            if (p1.coordEquals(p0))
            {
                idx1 = 0;
            }
            else
            {
                p1 = fig1.getPointAt(fig1.PointCount - 1);

                if (p1.coordEquals(p0))
                {
                    idx1 = fig1.PointCount - 1;
                }
            }

            if (idx1 == -1)
            {
                return null;
            }

            // Create figure that will be marged fig0 and fig1.
            CadFigure rfig = DB.newFigure(CadFigure.Types.POLY_LINES);

            BondInfo ret = new BondInfo();

            if (idx0 == fig0.PointCount - 1)
            {
                // bond fig1 to fig0's tail

                ret.figA = fig0;
                ret.indexA = idx0;

                ret.figB = fig1;
                ret.indexB = idx1;

                rfig.copyPoints(fig0);

                if (idx1 == 0)
                {
                    rfig.addPoints(fig1.PointList, 1);
                }
                else
                {
                    rfig.addPointsReverse(fig1.PointList, 1);
                }
            }
            else if (idx1 == fig1.PointCount - 1)
            {
                // bond fig0 to fig1's tail

                ret.figA = fig1;
                ret.indexA = idx1;

                ret.figB = fig0;
                ret.indexB = idx0;

                rfig.copyPoints(fig1);

                if (idx0 == 0)
                {
                    rfig.addPoints(fig0.PointList, 1);
                }
                else
                {
                    rfig.addPointsReverse(fig0.PointList, 1);
                }
            }
            else
            {
                // Both points are head. Bond fig1 to reversed fig0 

                ret.figA = fig0;
                ret.indexA = idx0;

                ret.figB = fig1;
                ret.indexB = idx1;

                rfig.addPointsReverse(fig0.PointList);
                rfig.addPoints(fig1.PointList, 1);
            }

            ret.BondedFigure = rfig;
            ret.item = item;

            return ret;
        }
    }

    class CadSegmentCutter : CadFigureAssembler
    {
        public CadSegmentCutter(CadObjectDB db, CadLayer layer) : base(db, layer)
        {
        }

        public Result cutSegment(MarkSeg seg, CadPoint p)
        {
            ProcResult.clear();

            var ci = CadUtil.getNormCross2D(seg.pA, seg.pB, p);

            if (!ci.isCross)
            {
                return ProcResult;
            }

            CadFigure org = DB.getFigure(seg.FigureID);

            int a = Math.Min(seg.PtIndexA, seg.PtIndexB);
            int b = Math.Max(seg.PtIndexA, seg.PtIndexB);


            CadFigure fa = DB.newFigure(CadFigure.Types.POLY_LINES);
            CadFigure fb = DB.newFigure(CadFigure.Types.POLY_LINES);

            fa.addPoints(org.PointList, 0, a + 1);
            fa.addPoint(ci.CrossPoint);

            fb.addPoint(ci.CrossPoint);
            fb.addPoints(org.PointList, b);

            if (org.Closed)
            {
                fb.addPoint(fa.getPointAt(0));
            }

            ProcResult.AddList.Add(fa);
            ProcResult.AddList.Add(fb);

            ProcResult.RemoveList.Add(org);

            return ProcResult;
        }
    }
}
