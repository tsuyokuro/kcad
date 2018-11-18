using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    class CadFigureBonder
    {
        public static EditResult Bond(CadObjectDB db, CadFigure fig)
        {
            EditResult result = new EditResult();

            if (fig.LayerID == 0)
            {
                return result;
            }

            if (fig.IsLoop)
            {
                return result;
            }

            CadLayer layer = db.GetLayer(fig.LayerID);

            CadVector ps = fig.PointList[0];
            CadVector pe = fig.PointList[fig.PointCount-1];

            int pi = -1;
            int bpi = -1;

            CadFigure bfig = null;

            foreach (CadFigure tfig in layer.FigureList)
            {
                if (tfig.ID == fig.ID)
                {
                    continue;
                }

                if (tfig.IsLoop)
                {
                    continue;
                }

                CadVector tps = tfig.PointList[0];
                CadVector tpe = tfig.PointList[tfig.PointCount - 1];

                if (tps.Equals(ps))
                {
                    bpi = 0;
                    pi = 0;
                    bfig = tfig;
                    break;
                }

                if (tps.Equals(pe))
                {
                    bpi = 0;
                    pi = fig.PointCount-1;
                    bfig = tfig;
                    break;
                }

                if (tpe.Equals(ps))
                {
                    bpi = tfig.PointCount-1;
                    pi = 0;
                    bfig = tfig;
                    break;
                }

                if (tpe.Equals(pe))
                {
                    bpi = tfig.PointCount - 1;
                    pi = fig.PointCount - 1;
                    bfig = tfig;
                    break;
                }
            }

            if (pi < 0)
            {
                return result;
            }

            CadFigure newFig = db.NewFigure(CadFigure.Types.POLY_LINES);

            VectorList plist = new VectorList(fig.PointList);
            VectorList blist = new VectorList(bfig.PointList);

            if (pi == 0)
            {
                plist.Reverse();
            }

            if (bpi != 0)
            {
                blist.Reverse();
            }

            newFig.PointList.AddRange(plist);
            newFig.PointList.AddRange(blist);

            result.AddList.Add(new EditResult.Item(layer.ID, newFig));

            result.RemoveList.Add(new EditResult.Item(layer.ID, fig));
            result.RemoveList.Add(new EditResult.Item(layer.ID, bfig));

            return result;
        }
    }
}
