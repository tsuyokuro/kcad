//#define OPEN_TK_NEXT
//#define DEBUG_DRAW_NORMAL
#define DRAW_HALF_EDGE_OUTLINE

using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using HalfEdgeNS;
using CadDataTypes;
using System.Drawing;
using GLFont;

namespace Plotter
{
    class DrawingGL : DrawingBase
    {
        private DrawContextGL DC;

#if OPEN_TK_NEXT
        private const PrimitiveType LINES = PrimitiveType.Lines;
        private const PrimitiveType POLYGON = PrimitiveType.Polygon;
        private const PrimitiveType LINE_STRIP = PrimitiveType.LineStrip;
#else
        private const BeginMode LINES = BeginMode.Lines;
        private const BeginMode POLYGON = BeginMode.Polygon;
        private const BeginMode LINE_STRIP = BeginMode.LineStrip;
#endif

        private FontFaceW mFontFaceW;
        private FontRenderer mFontRenderer;

        public DrawingGL(DrawContextGL dc)
        {
            DC = dc;

            mFontFaceW = new FontFaceW();
            mFontFaceW.SetFont(@"C:\Windows\Fonts\msgothic.ttc", 0);
            mFontFaceW.SetSize(20);

            mFontRenderer = new FontRenderer();
            mFontRenderer.Init();
        }

        public override void Clear(int brush)
        {
            GL.ClearColor(DC.Color(brush));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void Draw(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            foreach (CadFigure fig in list)
            {
                fig.ForEachFig(a =>
                {
                    if (a.Current)
                    {
                        a.Draw(DC, DrawTools.PEN_FIGURE_HIGHLIGHT);
                    }
                    else
                    {
                        a.Draw(DC, pen);
                    }
                });
            }
        }

        public override void DrawSelected(List<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            Start2D();

            foreach (CadFigure fig in list)
            {
                fig.ForEachFig(a =>
                {
                    a.DrawSelected(DC, pen);
                });
            }

            End2D();
        }

        public override void DrawLine(int pen, CadVector a, CadVector b)
        {
            GLPen glpen = DC.Pen(pen);

            GL.Begin(PrimitiveType.LineStrip);
            GL.Color4(glpen.Color);

            a *= DC.WorldScale;
            b *= DC.WorldScale;

            GL.Vertex3(a.x, a.y, a.z);
            GL.Vertex3(b.x, b.y, b.z);

            GL.End();
        }

        public override void DrawFace(int pen, VectorList pointList, CadVector normal, bool drawOutline)
        {
            //DebugOut.Std.println("GL DrawFace");

            CadVector p;
            GLPen glpen;

            if (normal.IsZero())
            {
                normal = CadMath.Normal(pointList[0], pointList[1], pointList[2]);
            }

            bool normalValid = !normal.IsZero();


            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);


            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(0.8f, 0.8f, 0.8f, 1.0f);

            if (normalValid)
            {
                GL.Normal3(normal.vector);
            }

            foreach (CadVector pt in pointList)
            {
                p = pt * DC.WorldScale;

                GL.Vertex3(p.vector);
            }

            GL.End();

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            #region 輪郭

            if (drawOutline)
            {
                glpen = DC.Pen(pen);

                GL.Color4(glpen.Color);
                GL.LineWidth(1.0f);

                CadVector shift = GetShiftForOutLine();

                GL.Begin(PrimitiveType.LineStrip);

                foreach (CadVector pt in pointList)
                {
                    p = (pt + shift) * DC.WorldScale;
                    GL.Vertex3(p.vector);
                }

                CadVector pt0 = pointList[0];
                p = (pt0 + shift) * DC.WorldScale;

                GL.Vertex3(p.vector);

                GL.End();
            }
            #endregion
        }

        public override void DrawHarfEdgeModel(int pen, int edgePen, double edgeThreshold, HeModel model)
        {
            DrawHarfEdgeModel(pen, model);

#if DRAW_HALF_EDGE_OUTLINE
            DrawEdge(pen, edgePen, edgeThreshold, model);
#endif
        }

        private void DrawEdge(int pen, int edgePen, double edgeThreshold, HeModel model)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);
            GL.LineWidth(1.0f);

            GLPen glpen = DC.Pen(pen);
            GLPen glEdgepen = DC.Pen(edgePen);

            //Vector3d t = DC.ViewDir * (-0.1f / DC.WorldScale);

            CadVector shift = GetShiftForOutLine();

            CadVector p0;
            CadVector p1;


            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                HalfEdge pair;

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
                        double s = CadMath.InnerProduct(model.NormalStore[c.Normal], model.NormalStore[pair.Normal]);

                        if (Math.Abs(s) < edgeThreshold)
                        {
                            draw = true;
                        }
                    }

                    HalfEdge next = c.Next;

                    p0 = model.VertexStore.Ref(c.Vertex) * DC.WorldScale + shift;
                    p1 = model.VertexStore.Ref(next.Vertex) * DC.WorldScale + shift;

                    if (draw)
                    {
                        GL.Color4(glEdgepen.Color);
                    }
                    else
                    {
                        GL.Color4(glpen.Color);
                    }

                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(p0.vector);
                    GL.Vertex3(p1.vector);
                    GL.End();

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public override void DrawHarfEdgeModel(int pen, HeModel model)
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            
            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                GL.Begin(PrimitiveType.Polygon);
                GL.Color4(0.8f, 0.8f, 0.8f, 1.0f);

                if (f.Normal != HeModel.INVALID_INDEX)
                {
                    CadVector nv = model.NormalStore[f.Normal];
                    GL.Normal3(nv.vector);
                }

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    CadVector p = model.VertexStore.Ref(c.Vertex);

                    GL.Vertex3((p * DC.WorldScale).vector);

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }

                GL.End();

#if DEBUG_DRAW_NORMAL

                c = head;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    CadVector p = model.VertexStore.Ref(c.Vertex);

                    if (c.Normal != HeModel.INVALID_INDEX)
                    {
                        CadVector nv = model.NormalStore[c.Normal];
                        CadVector np0 = p;
                        CadVector np1 = p + (nv * 15);

                        GL.Disable(EnableCap.Lighting);
                        GL.Disable(EnableCap.Light0);

                        DrawArrow(pen, np0, np1, ArrowTypes.CROSS, ArrowPos.END, 3, 3);

                        GL.Enable(EnableCap.Lighting);
                        GL.Enable(EnableCap.Light0);
                    }


                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
#endif
            }
        }

        public override void DrawAxis()
        {
            CadVector p0 = default(CadVector);
            CadVector p1 = default(CadVector);

            double len = 120.0;
            double arrowLen = 12.0 / DC.WorldScale;
            double arrowW2 = 6.0 / DC.WorldScale;

            // X軸
            p0.x = -len;
            p0.y = 0;
            p0.z = 0;

            p1.x = len;
            p1.y = 0;
            p1.z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            // Y軸
            p0.x = 0;
            p0.y = -len;
            p0.z = 0;

            p1.x = 0;
            p1.y = len;
            p1.z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -len;

            p1.x = 0;
            p1.y = 0;
            p1.z = len;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);


            //GL.Translate(20, 0, 0);
            //GL.Color4(Color.White);

            //FontW.RenderW("123", RenderMode.All);
        }

        private void PushMatrixes()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
        }

        private void PopMatrixes()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

        private void Start2D()
        {
            PushMatrixes();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MultMatrix(ref DC.OrthographicMatrix);
        }

        private void End2D()
        {
            PopMatrixes();
        }

        public override void DrawSelectedPoint(CadVector pt, int pen = DrawTools.PEN_SELECT_POINT)
        {
            CadVector p0 = DC.WorldPointToDevPoint(pt) - 2;
            CadVector p1 = p0 + 4;

            DrawRect2D(p0.vector, p1.vector, pen);
        }

        private void DrawRect2D(Vector3d p0, Vector3d p1, int pen)
        {
            Vector3d v0 = Vector3d.Zero;
            Vector3d v1 = Vector3d.Zero;
            Vector3d v2 = Vector3d.Zero;
            Vector3d v3 = Vector3d.Zero;

            v0.X = System.Math.Max(p0.X, p1.X);
            v0.Y = System.Math.Min(p0.Y, p1.Y);

            v1.X = v0.X;
            v1.Y = System.Math.Max(p0.Y, p1.Y);

            v2.X = System.Math.Min(p0.X, p1.X);
            v2.Y = v1.Y;

            v3.X = v2.X;
            v3.Y = v0.Y;

            GLPen glpen = DC.Pen(pen);

            GL.Begin(PrimitiveType.LineStrip);

            GL.Color4(glpen.Color);
            GL.Vertex3(v0);
            GL.Vertex3(v1);
            GL.Vertex3(v2);
            GL.Vertex3(v3);
            GL.Vertex3(v0);

            GL.End();
        }

        public override void DrawMarkCursor(int pen, CadVector p, double size)
        {
            DrawCross(pen, p, size / DC.WorldScale);
        }

        public override void DrawCross(int pen, CadVector p, double size)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            double hs = size;

            CadVector px0 = p;
            px0.x -= hs;
            CadVector px1 = p;
            px1.x += hs;

            CadVector py0 = p;
            py0.y -= hs;
            CadVector py1 = p;
            py1.y += hs;

            CadVector pz0 = p;
            pz0.z -= hs;
            CadVector pz1 = p;
            pz1.z += hs;

            DrawLine(pen, px0, px1);
            DrawLine(pen, py0, py1);
            DrawLine(pen, pz0, pz1);
        }

        private CadVector GetShiftForOutLine()
        {
            CadVector v = DC.DevVectorToWorldVector(CadVector.UnitX);
            Vector3d vv = -DC.ViewDir * v.Norm();

            return (CadVector)vv;
        }

        private void DumpGLMatrix()
        {
            GL.MatrixMode(MatrixMode.Modelview);

            double[] model = new double[16];
            double[] projection = new double[16];

            GL.GetDouble(GetPName.ProjectionMatrix, projection);

            UMatrix4 m4 = new UMatrix4(projection);


            m4.dump("Get");

            DC.ProjectionMatrix.dump("Set");
        }

        public override void DrawText(int font, int brush, CadVector a, CadVector xdir, CadVector ydir, DrawTextOption opt, string s)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();

            FontTex tex = mFontFaceW.CreateTexture(s);

            CadVector xv = xdir.UnitVector() * tex.ImgW * 0.3;
            CadVector yv = ydir.UnitVector() * tex.ImgH * 0.3;

            if (xv.IsZero() || yv.IsZero())
            {
                return;
            }

            if ((opt.Option & DrawTextOption.H_CENTER)!=0)
            {
                a -= (xv / 2);
            }

            GL.Color4(DC.Color(brush));
            
            mFontRenderer.Render(tex, a.vector, xv.vector, yv.vector);
        }
    }
}
