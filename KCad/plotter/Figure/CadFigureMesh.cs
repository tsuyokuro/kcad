using HalfEdgeNS;
using MyCollections;
using Newtonsoft.Json.Linq;
using OpenTK;
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

        private FlexArray<int> mEdge;

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

            mEdge = new FlexArray<int>();
        }

        public void CreateModel(CadFigure fig)
        {
            if (!(fig is CadFigurePolyLines))
            {
                return;
            }

            mEdge.Clear();
            mHeModel.Clear();

            for (int i = 0; i<fig.PointCount; i++)
            {
                int idx = mHeModel.AddVertex(fig.PointList[i]);

                mEdge.Add(idx);
            }

            List<CadFigure> figList = TriangleSplitter.Split(fig, 16);


            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure t = figList[i];
                mHeModel.AddTriangle(t.PointList[0], t.PointList[1], t.PointList[2]);
            }
        }

        public override void Draw(DrawContext dc, int pen)
        {
            //DrawFaces(dc, DrawTools.PEN_MESH_LINE);
            dc.Drawing.DrawHarfEdgeModel(DrawTools.PEN_MESH_LINE, mHeModel);
            DrawEdge(dc, pen);
        }

        private void DrawFaces(DrawContext dc, int pen)
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

        private void DrawEdge(DrawContext dc, int pen)
        {
            Vector3d t = dc.ViewDir * (-0.2f / dc.WoldScale);

            CadVector shift = (CadVector)t;


            CadVector p0;
            CadVector p1;

            for (int i = 0; i < mEdge.Count - 1; i++)
            {
                p0 = mHeModel.VertexStore.Ref(mEdge[i]);
                p1 = mHeModel.VertexStore.Ref(mEdge[i + 1]);

                dc.Drawing.DrawLine(pen, p0 + shift, p1 + shift);
            }

            p0 = mHeModel.VertexStore.Ref(mEdge[mEdge.Count - 1]);
            p1 = mHeModel.VertexStore.Ref(mEdge[0]);
            dc.Drawing.DrawLine(pen, p0 + shift, p1 + shift);
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

        public override JObject GeometricDataToJson()
        {
            JObject jvdata = new JObject();

            JArray jedge = CadJson.ToJson.IntArrayToJson(mEdge);
            
            JObject jmodel = HeUtil.HeModelToJson(mHeModel);

            jvdata.Add("edge", jedge);

            jvdata.Add("model", jmodel);

            return jvdata;
        }

        public override void GeometricDataFromJson(JObject jvdata, CadJson.VersionCode version)
        {
            JArray jedge = (JArray)jvdata["edge"];
            JObject jmodel = (JObject)jvdata["model"];

            HeModel model = HeUtil.HeModelFromJson(jmodel, version);

            if (model == null)
            {
                return;
            }

            FlexArray<int> edge = CadJson.FromJson.IntArrayFromJson(jedge);

            if (edge == null)
            {
                return;
            }

            mHeModel = model;

            mPointList = mHeModel.VertexStore;

            mEdge = edge;
        }
    }
}
