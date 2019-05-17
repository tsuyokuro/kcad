using HalfEdgeNS;
using MyCollections;
using Newtonsoft.Json.Linq;
using OpenTK;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadDataTypes;

namespace Plotter
{
    public class CadFigureMesh : CadFigure
    {
        public HeModel mHeModel;

        public static double EDGE_THRESHOLD;

        private FlexArray<IndexPair> SegList = new FlexArray<IndexPair>();


        static CadFigureMesh()
        {
            EDGE_THRESHOLD = Math.Cos(CadMath.Deg2Rad(30));
        }

        public override VertexList PointList => mPointList;

        public override int PointCount => PointList.Count;

        public CadFigureMesh()
        {
            Type = Types.MESH;

            mHeModel = new HeModel();

            mPointList = mHeModel.VertexStore;
        }

        public void SetMesh(HeModel mesh)
        {
            mHeModel = mesh;
            mPointList = mHeModel.VertexStore;

            UpdateSegList();
        }

        public void CreateModel(CadFigure fig)
        {
            if (!(fig is CadFigurePolyLines))
            {
                return;
            }

            mHeModel.Clear();

            for (int i = 0; i < fig.PointCount; i++)
            {
                int idx = mHeModel.AddVertex(fig.PointList[i]);
            }

            List<CadFigure> figList = TriangleSplitter.Split(fig, 16);

            HeModelBuilder mb = new HeModelBuilder();

            mb.Start(mHeModel);

            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure t = figList[i];
                mb.AddTriangle(t.PointList[0], t.PointList[1], t.PointList[2]);
            }
        }

        public override void EndEdit()
        {
            base.EndEdit();
            mHeModel.RecreateNormals();
        }

        public override CadSegment GetSegmentAt(int n)
        {
            CadSegment seg = default(CadSegment);
            seg.P0 = mPointList[SegList[n].Idx0];
            seg.P1 = mPointList[SegList[n].Idx1];

            return seg;
        }

        public override FigureSegment GetFigSegmentAt(int n)
        {
            FigureSegment seg = new FigureSegment(this, n, SegList[n].Idx0, SegList[n].Idx1);
            return seg;
        }

        public override int SegmentCount
        {
            get
            {
                return SegList.Count;
            }
        }

        private void UpdateSegList()
        {
            SegList.Clear();

            for (int i = 0; i < mHeModel.FaceStore.Count; i++)
            {
                HeFace f = mHeModel.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;


                for (; ; )
                {
                    HalfEdge next = c.Next;

                    SegList.Add(new IndexPair(c.Vertex, next.Vertex));

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public override void Draw(DrawContext dc, DrawPen pen)
        {
            dc.Drawing.DrawHarfEdgeModel(
                dc.GetPen(DrawTools.PEN_MESH_LINE), pen, EDGE_THRESHOLD, mHeModel);
        }

        public override void DrawSelected(DrawContext dc, DrawPen pen)
        {
            int i;
            int num = PointList.Count;

            for (i = 0; i < num; i++)
            {
                CadVertex p = PointList[i];

                if (!p.Selected) continue;

                dc.Drawing.DrawSelectedPoint(p, dc.GetPen(DrawTools.PEN_SELECT_POINT));
            }
        }

        public override void SelectPointAt(int index, bool sel)
        {
            CadVertex p = mPointList[index];
            p.Selected = sel;
            mPointList[index] = p;
        }

        public override Centroid GetCentroid()
        {
            Centroid cent = default(Centroid);
            Centroid ct = default(Centroid);

            for (int i = 0; i < mHeModel.FaceStore.Count; i++)
            {
                HeFace f = mHeModel.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge he = head;

                int i0 = he.Vertex;
                int i1 = he.Next.Vertex;
                int i2 = he.Next.Next.Vertex;

                ct.set(
                    mHeModel.VertexStore[i0],
                    mHeModel.VertexStore[i1],
                    mHeModel.VertexStore[i2]
                    );

                cent = CadUtil.MergeCentroid(cent, ct);
            }

            return cent;
        }

        public override void InvertDir()
        {
            mHeModel.InvertAllFace();
        }

        public override MpGeometricData GeometricDataToMp()
        {
            MpMeshGeometricData mpGeo = new MpMeshGeometricData();
            mpGeo.HeModel = MpHeModel.Create(mHeModel);

            return mpGeo;
        }

        public override void GeometricDataFromMp(MpGeometricData mpGeo)
        {
            if (!(mpGeo is MpMeshGeometricData))
            {
                return;
            }

            MpMeshGeometricData meshGeo = (MpMeshGeometricData)mpGeo;

            mHeModel = meshGeo.HeModel.Restore();

            mPointList = mHeModel.VertexStore;
        }

        public override void RemoveSelected()
        {
            List<int> removeList = new List<int>();

            for (int i = 0; i < mPointList.Count; i++)
            {
                if (mPointList[i].Selected)
                {
                    mHeModel.RemoveVertexRelationFace(i);
                    removeList.Add(i);
                }
            }

            if (mHeModel.FaceStore.Count == 0)
            {
                mHeModel.Clear();
                return;
            }

            mHeModel.RemoveVertexs(removeList);
        }
    }
}
