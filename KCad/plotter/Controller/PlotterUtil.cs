using CadDataTypes;
using System;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public class PlotterUtil
    {
        public static CadVertex GetSelectionCenter(PlotterController c)
        {
            CadVertex min = CadVertex.Create(CadConst.MaxValue);
            CadVertex max = CadVertex.Create(CadConst.MinValue);

            int selPointCnt = 0;

            foreach (CadLayer layer in c.DB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    foreach (CadVertex p in fig.PointList)
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

            CadVertex cp = (max - min) / 2f + min;

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

        // Calculate the sum of the areas of selected shapes
        // 選択された図形の面積の総和を求める
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

        //
        // Calculate the intersection point in the screen coordinate system
        // スクリーン座標系での交点を求める
        //
        public static CadVertex CrossOnScreen(DrawContext dc, CadVertex wp00, CadVertex wp01, CadVertex wp10, CadVertex wp11)
        {
            CadVertex sp00 = dc.WorldPointToDevPoint(wp00);
            CadVertex sp01 = dc.WorldPointToDevPoint(wp01);
            CadVertex sp10 = dc.WorldPointToDevPoint(wp10);
            CadVertex sp11 = dc.WorldPointToDevPoint(wp11);

            CadVertex cp = CadUtil.CrossLine2D(sp00, sp01, sp10, sp11);

            return cp;
        }
    }
}