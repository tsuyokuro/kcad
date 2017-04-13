using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class CadFigureBonder : CadFigureAssembler
    {
        private CadLayer Layer = null;

        private List<CadFigure> Work;
        private List<SelectItem> SelectList;
        private List<Item> ItemList;

        private uint LayerID
        {
            get
            {
                if (Layer == null)
                {
                    return 0;
                }

                return Layer.ID;
            }
        }

        public CadFigureBonder(CadObjectDB db, CadLayer layer) : base(db)
        {
            Layer = layer;

            // Copy figure list to Work
            if (layer != null)
            {
                Work = new List<CadFigure>(layer.FigureList);
            }
            else
            {
                Work = new List<CadFigure>();

                foreach (CadLayer la in db.LayerList)
                {
                    foreach (CadFigure fig in la.FigureList)
                    {
                        Work.Add(fig);
                    }
                }
            }
        }

        private class Item
        {
            public uint LayerID;
            public uint FigureID;
            public int PointIndex;
            public CadPoint Point;

            public void set(CadObjectDB db, SelectItem si)
            {
                LayerID = si.LayerID;
                FigureID = si.FigureID;
                PointIndex = si.PointIndex;

                CadFigure fig = db.getFigure(FigureID);
                Point = fig.PointList[PointIndex];
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
            SelectList = new List<SelectItem>();

            foreach (SelectItem item in selList)
            {
                if (Layer != null)
                {
                    if (item.LayerID != LayerID)
                    {
                        continue;
                    }
                }

                SelectList.Add(item);
            }

            // Collect endpoint (first and last point) items
            // to ItemList 
            collectEndPoint(SelectList);

            // joint end points
            bondMain();

            foreach (ResultItem ri in ProcResult.AddList)
            {
                if (ri.Figure.PointCount > 2)
                {
                    CadPoint sp = ri.Figure.PointList[0];
                    CadPoint ep = ri.Figure.PointList[ri.Figure.PointList.Count-1];

                    if (ep.coordEquals(sp))
                    {
                        ri.Figure.RemovePointAt(ri.Figure.PointList.Count - 1);
                        ri.Figure.Closed = true;
                    }
                }
            }

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
                item.set(DB, si);
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

                    ProcResult.AddList.Add(new ResultItem(LayerID, bondInfo.BondedFigure));
                    ProcResult.RemoveList.Add(new ResultItem(bondInfo.figA.LayerID, bondInfo.figA));
                    ProcResult.RemoveList.Add(new ResultItem(bondInfo.figB.LayerID, bondInfo.figB));

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
            CadPoint head = bi.BondedFigure.GetPointAt(0);
            CadPoint tail = bi.BondedFigure.GetPointAt(pcnt - 1);

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

            CadPoint p0 = fig0.GetPointAt(idx0);
            CadPoint p1 = fig1.GetPointAt(0);


            int idx1 = -1;

            if (p1.coordEquals(p0))
            {
                idx1 = 0;
            }
            else
            {
                p1 = fig1.GetPointAt(fig1.PointCount - 1);

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
            rfig.LayerID = LayerID;

            BondInfo ret = new BondInfo();

            if (idx0 == fig0.PointCount - 1)
            {
                // bond fig1 to fig0's tail

                ret.figA = fig0;
                ret.indexA = idx0;

                ret.figB = fig1;
                ret.indexB = idx1;

                rfig.CopyPoints(fig0);

                if (idx1 == 0)
                {
                    rfig.AddPoints(fig1.PointList, 1);
                }
                else
                {
                    rfig.AddPointsReverse(fig1.PointList, 1);
                }
            }
            else if (idx1 == fig1.PointCount - 1)
            {
                // bond fig0 to fig1's tail

                ret.figA = fig1;
                ret.indexA = idx1;

                ret.figB = fig0;
                ret.indexB = idx0;

                rfig.CopyPoints(fig1);

                if (idx0 == 0)
                {
                    rfig.AddPoints(fig0.PointList, 1);
                }
                else
                {
                    rfig.AddPointsReverse(fig0.PointList, 1);
                }
            }
            else
            {
                // Both points are head. Bond fig1 to reversed fig0 

                ret.figA = fig0;
                ret.indexA = idx0;

                ret.figB = fig1;
                ret.indexB = idx1;

                rfig.AddPointsReverse(fig0.PointList);
                rfig.AddPoints(fig1.PointList, 1);
            }

            ret.BondedFigure = rfig;
            ret.item = item;

            return ret;
        }
    }
}
