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

            mHeModel = new HeModel(mPointList);
        }

        public void CreateModel(CadFigure fig)
        {
            if (!(fig is CadFigurePolyLines))
            {
                return;
            }

            List<CadFigure> figList = TriangleSplitter.Split(fig);

            mHeModel.Clear();

            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure t = figList[i];
                mHeModel.AddTriangle(t.PointList[0], t.PointList[1], t.PointList[2]);
            }
        }

        public override void Draw(DrawContext dc, int pen)
        {
            for (int i = 0; i < mHeModel.FaceStore.Count; i++)
            {
                HeFace f = mHeModel.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                CadVector v;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    dc.Drawing.DrawLine(pen,
                        mHeModel.VertexStore.Ref(c.Vertex),
                        mHeModel.VertexStore.Ref(next.Vertex));

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }

        }

        public override void DrawSelected(DrawContext dc, int pen)
        {
            int i;
            int num = PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadVector p = PointList[i];

                if (!p.Selected) continue;

                dc.Drawing.DrawSelectedPoint(p);
            }
        }

        public override void SelectPointAt(int index, bool sel)
        {
            CadVector p = mPointList[index];
            p.Selected = sel;
            mPointList[index] = p;
        }
    }
}
