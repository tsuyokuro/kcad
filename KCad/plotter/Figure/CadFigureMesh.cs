using HalfEdgeNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class CadFigureMesh : CadFigure
    {
        private HeModel mHeModel;

        public override VectorList PointList
        {
            get
            {
                return mPointList;
            }
        }

        public override int PointCount
        {
            get
            {
                return PointList.Count;
            }
        }

        public CadFigureMesh()
        {
            Type = Types.MESH;

            mHeModel = new HeModel();
        }

        public void CreateModel(CadFigure fig)
        {
            if (!(fig is CadFigurePolyLines))
            {
                return;
            }


        }
    }
}
