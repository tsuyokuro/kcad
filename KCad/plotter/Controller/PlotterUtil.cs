using CadDataTypes;
using System;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public class PlotterUtil
    {
        public static CadVector GetSelectionCenter(PlotterController c)
        {
            CadVector min = CadVector.Create(CadConst.MaxValue);
            CadVector max = CadVector.Create(CadConst.MinValue);

            int selPointCnt = 0;

            foreach (CadLayer layer in c.DB.LayerList)
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

            return cp;
        }


        public static Centroid Centroid(PlotterController c)
        {
            List<uint> idList = c.DB.GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = c.DB.GetFigure(id);

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

        public static double Area(PlotterController c)
        {
            List<uint> idList = c.DB.GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = c.DB.GetFigure(id);

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

        public static CadVector CrossOnScreen(DrawContext dc, CadVector wp00, CadVector wp01, CadVector wp10, CadVector wp11)
        {
            CadVector sp00 = dc.WorldPointToDevPoint(wp00);
            CadVector sp01 = dc.WorldPointToDevPoint(wp01);
            CadVector sp10 = dc.WorldPointToDevPoint(wp10);
            CadVector sp11 = dc.WorldPointToDevPoint(wp11);

            CadVector cp = CadUtil.CrossLine2D(sp00, sp01, sp10, sp11);

            return cp;
        }
    }
}