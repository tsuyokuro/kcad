using CadDataTypes;
using System;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        private CadVector GetSelectionCenter()
        {
            CadVector min = CadVector.Create(CadConst.MaxValue);
            CadVector max = CadVector.Create(CadConst.MinValue);

            int selPointCnt = 0;

            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    foreach (CadVector p in fig.PointList)
                    {
                        if (p.Selected)
                        {
                            selPointCnt++;

                            min.x = Math.Min(p.x, min.x);
                            min.y = Math.Min(p.y, min.y);
                            min.z = Math.Min(p.z, min.z);

                            max.x = Math.Max(p.x, max.x);
                            max.y = Math.Max(p.y, max.y);
                            max.z = Math.Max(p.z, max.z);
                        }
                    }
                }
            }

            CadVector cp = (max - min) / 2f + min;

            DebugOut.println("GetSelectionCenter() sel pt cnt=" + selPointCnt.ToString());

            return cp;
        }


        public Centroid Centroid()
        {
            List<uint> idList = DB.GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = mDB.GetFigure(id);

                Centroid t = fig.GetCentroid();

                if (cent.IsInvalid)
                {
                    cent = t;
                    continue;
                }

                if (t.IsInvalid)
                {
                    continue;
                }

                cent = CadUtil.MergeCentroid(cent, t);
            }

            return cent;
        }

        public double Area()
        {
            List<uint> idList = DB.GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = mDB.GetFigure(id);

                Centroid t = fig.GetCentroid();

                if (cent.IsInvalid)
                {
                    cent = t;
                    continue;
                }

                if (t.IsInvalid)
                {
                    continue;
                }

                cent = CadUtil.MergeCentroid(cent, t);
            }

            if (cent.IsInvalid)
            {
                return 0;
            }

            return cent.Area;
        }
    }
}