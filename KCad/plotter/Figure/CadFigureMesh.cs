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

        private FlexArray<Index2> SegList = new FlexArray<Index2>();


        static CadFigureMesh()
        {
            EDGE_THRESHOLD = Math.Cos(CadMath.Deg2Rad(15));
        }

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

                CadVector v;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    SegList.Add(new Index2(c.Vertex, next.Vertex));

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public override void Draw(DrawContext dc, int pen)
        {
            dc.Drawing.DrawHarfEdgeModel(DrawTools.PEN_MESH_LINE, mHeModel);
            DrawEdge(dc, pen);
        }

        private void DrawEdge(DrawContext dc, int pen)
        {
            Vector3d t = dc.ViewDir * (-0.2f / dc.WorldScale);

            CadVector shift = (CadVector)t;


            CadVector p0;
            CadVector p1;


            for (int i = 0; i < mHeModel.FaceStore.Count; i++)
            {
                HeFace f = mHeModel.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                HalfEdge pair;

                CadVector v;

                for (; ; )
                {
                    bool draw = false;

                    pair = c.Pair;

                    if (pair == null)
                    {
                        draw = true;
                    }
                    else
                    {
                        double s = CadMath.InnerProduct(mHeModel.NormalStore[c.Normal], mHeModel.NormalStore[pair.Normal]);

                        if ( Math.Abs(s) < EDGE_THRESHOLD)
                        {
                            draw = true;
                        } 
                    }

                    HalfEdge next = c.Next;

                    if (draw)
                    {
                        dc.Drawing.DrawLine(pen,
                            mHeModel.VertexStore.Ref(c.Vertex) + shift,
                            mHeModel.VertexStore.Ref(next.Vertex) + shift
                            );
                    }

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

        public override JObject GeometricDataToJson()
        {
            JObject jvdata = new JObject();

            JObject jmodel = HeJson.HeModelToJson(mHeModel);

            jvdata.Add("model", jmodel);

            return jvdata;
        }

        public override void GeometricDataFromJson(JObject jvdata)
        {
            JObject jmodel = (JObject)jvdata["model"];

            HeModel model = HeJson.HeModelFromJson(jmodel);

            if (model == null)
            {
                return;
            }

            mHeModel = model;

            mPointList = mHeModel.VertexStore;
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
