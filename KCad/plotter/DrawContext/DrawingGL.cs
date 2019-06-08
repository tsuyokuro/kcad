//#define DEBUG_DRAW_NORMAL
#define DRAW_HALF_EDGE_OUTLINE

using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using HalfEdgeNS;
using GLFont;
using OpenTK.Graphics;

namespace Plotter
{
    class DrawingGL : DrawingBase
    {
        private DrawContextGL DC;

        private FontFaceW mFontFaceW;
        private FontRenderer mFontRenderer;

        public DrawingGL(DrawContextGL dc)
        {
            DC = dc;

            mFontFaceW = new FontFaceW();
            //mFontFaceW.SetFont(@"C:\Windows\Fonts\msgothic.ttc", 0);
            mFontFaceW.SetResourceFont("/Fonts/mplus-1m-regular.ttf");
            mFontFaceW.SetSize(20);

            mFontRenderer = new FontRenderer();
            mFontRenderer.Init();
        }

        public override void Clear(DrawBrush brush)
        {
            GL.ClearColor(brush.Color4());
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void Draw(List<CadFigure> list, DrawPen pen)
        {
            foreach (CadFigure fig in list)
            {
                fig.ForEachFig((Action<CadFigure>)(a =>
                {
                    if (a.Current)
                    {
                        a.Draw(DC, DrawPen.New(DC, DrawTools.PEN_FIGURE_HIGHLIGHT));
                    }
                    else
                    {
                        a.Draw(DC, pen);
                    }
                }));
            }
        }

        public override void DrawSelected(List<CadFigure> list, DrawPen pen)
        {
            DisableLight();

            foreach (CadFigure fig in list)
            {
                fig.ForEachFig(a =>
                {
                    a.DrawSelected(DC, pen);
                });
            }
        }

        public override void DrawLine(DrawPen pen, Vector3d a, Vector3d b)
        {
            a *= DC.WorldScale;
            b *= DC.WorldScale;

            GL.Color4(pen.Color4());

            GL.Begin(PrimitiveType.LineStrip);

            GL.Vertex3(a);
            GL.Vertex3(b);

            GL.End();
        }

        //public override void DrawFace(DrawPen pen, VertexList pointList, Vector3d normal, bool drawOutline)
        //{
        //    //DebugOut.Std.println("GL DrawFace");

        //    Vector3d p;

        //    if (normal.IsZero())
        //    {
        //        normal = CadMath.Normal(pointList[0], pointList[1], pointList[2]);
        //    }

        //    bool normalValid = !normal.IsZero();


        //    GL.Enable(EnableCap.Lighting);
        //    GL.Enable(EnableCap.Light0);


        //    GL.Begin(PrimitiveType.Polygon);
        //    GL.Color4(0.8f, 0.8f, 0.8f, 1.0f);

        //    if (normalValid)
        //    {
        //        GL.Normal3(normal.vector);
        //    }

        //    foreach (Vector3d pt in pointList)
        //    {
        //        p = pt * DC.WorldScale;

        //        GL.Vertex3(p.vector);
        //    }

        //    GL.End();

        //    GL.Disable(EnableCap.Lighting);
        //    GL.Disable(EnableCap.Light0);

        //    #region 輪郭

        //    if (drawOutline)
        //    {
        //        Color4 color = pen.Color4();

        //        GL.Color4(color);
        //        GL.LineWidth(1.0f);

        //        Vector3d shift = GetShiftForOutLine();

        //        GL.Begin(PrimitiveType.LineStrip);

        //        foreach (Vector3d pt in pointList)
        //        {
        //            p = (pt + shift) * DC.WorldScale;
        //            GL.Vertex3(p.vector);
        //        }

        //        Vector3d pt0 = pointList[0];
        //        p = (pt0 + shift) * DC.WorldScale;

        //        GL.Vertex3(p.vector);

        //        GL.End();
        //    }
        //    #endregion
        //}

        public override void DrawHarfEdgeModel(DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
            if (SettingsHolder.Settings.FillMesh)
            {
                DrawHarfEdgeModel(pen, model);
            }

            if (SettingsHolder.Settings.DrawMeshEdge)
            {
                DrawEdge(pen, edgePen, edgeThreshold, model);
            }
        }

        private void DrawEdge(DrawPen pen, DrawPen edgePen, double edgeThreshold, HeModel model)
        {
            DisableLight();

            GL.LineWidth(1.0f);

            Color4 color = pen.Color4();
            Color4 edgeColor = edgePen.Color4();

            Vector3d shift = GetShiftForOutLine();

            Vector3d p0;
            Vector3d p1;


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

                    p0 = model.VertexStore.Ref(c.Vertex).vector * DC.WorldScale + shift;
                    p1 = model.VertexStore.Ref(next.Vertex).vector * DC.WorldScale + shift;

                    if (draw)
                    {
                        GL.Color4(edgeColor);
                    }
                    else
                    {
                        GL.Color4(color);
                    }

                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(p0);
                    GL.Vertex3(p1);
                    GL.End();

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }
            }
        }

        public override void DrawHarfEdgeModel(DrawPen pen, HeModel model)
        {
            EnableLight();
            
            for (int i = 0; i < model.FaceStore.Count; i++)
            {
                HeFace f = model.FaceStore[i];

                HalfEdge head = f.Head;

                HalfEdge c = head;

                GL.Begin(PrimitiveType.Polygon);
                GL.Color4(0.8f, 0.8f, 0.8f, 1.0f);

                if (f.Normal != HeModel.INVALID_INDEX)
                {
                    Vector3d nv = model.NormalStore[f.Normal];
                    GL.Normal3(nv);
                }

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    Vector3d p = model.VertexStore.Ref(c.Vertex).vector;

                    GL.Vertex3((p * DC.WorldScale));

                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }

                GL.End();

#if DEBUG_DRAW_NORMAL
                DisableLight();

                c = head;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    Vector3d p = model.VertexStore.Ref(c.Vertex);

                    if (c.Normal != HeModel.INVALID_INDEX)
                    {
                        Vector3d nv = model.NormalStore[c.Normal];
                        Vector3d np0 = p;
                        Vector3d np1 = p + (nv * 15);

                        DrawArrow(pen, np0, np1, ArrowTypes.CROSS, ArrowPos.END, 3, 3);
                    }


                    c = next;

                    if (c == head)
                    {
                        break;
                    }
                }

                EnableLight();
#endif
            }

            DisableLight();
        }

        public override void DrawAxis()
        {
            Vector3d p0 = default(Vector3d);
            Vector3d p1 = default(Vector3d);

            double len = 100.0;
            double arrowLen = 4.0 / DC.WorldScale;
            double arrowW2 = 2.0 / DC.WorldScale;

            // X軸
            p0.X = -len;
            p0.Y = 0;
            p0.Z = 0;

            p1.X = len;
            p1.Y = 0;
            p1.Z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            if (!CadMath.IsParallel(p1 - p0, (Vector3d)DC.ViewDir))
            {
                DrawArrow(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
            }

            // Y軸
            p0.X = 0;
            p0.Y = -len;
            p0.Z = 0;

            p1.X = 0;
            p1.Y = len;
            p1.Z = 0;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            if (!CadMath.IsParallel(p1 - p0, (Vector3d)DC.ViewDir))
            {
                DrawArrow(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
            }

            // Z軸
            p0.X = 0;
            p0.Y = 0;
            p0.Z = -len;

            p1.X = 0;
            p1.Y = 0;
            p1.Z = len;

            p0 /= DC.WorldScale;
            p1 /= DC.WorldScale;

            if (!CadMath.IsParallel(p1 - p0, (Vector3d)DC.ViewDir))
            {
                DrawArrow(DrawPen.New(DC, DrawTools.PEN_AXIS), p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
            }
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

            GL.MultMatrix(ref DC.Matrix2D);
        }

        private void End2D()
        {
            PopMatrixes();
        }

        public override void DrawSelectedPoint(Vector3d pt, DrawPen pen)
        {
            Vector3d p0 = DC.WorldPointToDevPoint(pt).Add(-2);
            Vector3d p1 = p0.Add(4);

            DrawRect2D(p0, p1, pen);
        }

        private void DrawRect2D(Vector3d p0, Vector3d p1, DrawPen pen)
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

            Start2D();

            GL.Begin(PrimitiveType.LineStrip);

            GL.Color4(pen.Color4());
            GL.Vertex3(v0);
            GL.Vertex3(v1);
            GL.Vertex3(v2);
            GL.Vertex3(v3);
            GL.Vertex3(v0);

            GL.End();

            End2D();
        }

        public override void DrawCross(DrawPen pen, Vector3d p, double size)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            double hs = size;

            Vector3d px0 = p;
            px0.X -= hs;
            Vector3d px1 = p;
            px1.X += hs;

            Vector3d py0 = p;
            py0.Y -= hs;
            Vector3d py1 = p;
            py1.Y += hs;

            Vector3d pz0 = p;
            pz0.Z -= hs;
            Vector3d pz1 = p;
            pz1.Z += hs;

            DrawLine(pen, px0, px1);
            DrawLine(pen, py0, py1);
            DrawLine(pen, pz0, pz1);
        }

        private Vector3d GetShiftForOutLine()
        {
            double shift = DC.DevSizeToWoldSize(0.9);
            Vector3d vv = -DC.ViewDir * shift;

            return vv;
        }

        //private void DumpGLMatrix()
        //{
        //    GL.MatrixMode(MatrixMode.Modelview);

        //    double[] model = new double[16];
        //    double[] projection = new double[16];

        //    GL.GetDouble(GetPName.ProjectionMatrix, projection);

        //    UMatrix4 m4 = new UMatrix4(projection);


        //    m4.dump("Get");

        //    DC.ProjectionMatrix.dump("Set");
        //}

        public override void DrawText(int font, DrawBrush brush, Vector3d a, Vector3d xdir, Vector3d ydir, DrawTextOption opt, string s)
        {
            a *= DC.WorldScale;

            FontTex tex = mFontFaceW.CreateTexture(s);

            Vector3d xv = xdir.UnitVector() * tex.ImgW * 0.15;
            Vector3d yv = ydir.UnitVector() * tex.ImgH * 0.15;

            if (xv.IsZero() || yv.IsZero())
            {
                return;
            }

            if ((opt.Option & DrawTextOption.H_CENTER)!=0)
            {
                a -= (xv / 2);
            }

            GL.Color4(brush.Color4());
            
            mFontRenderer.Render(tex, a, xv, yv);
        }

        public override void DrawCrossCursorScrn(CadCursor pp, DrawPen pen)
        {
            double size = Math.Max(DC.ViewWidth, DC.ViewHeight);

            Vector3d p0 = pp.Pos.vector - (pp.DirX.vector * size);
            Vector3d p1 = pp.Pos.vector + (pp.DirX.vector * size);

            p0 = DC.DevPointToWorldPoint(p0);
            p1 = DC.DevPointToWorldPoint(p1);

            GL.Disable(EnableCap.DepthTest);

            DrawLine(pen, p0, p1);

            p0 = pp.Pos.vector - (pp.DirY.vector * size);
            p1 = pp.Pos.vector + (pp.DirY.vector * size);

            p0 = DC.DevPointToWorldPoint(p0);
            p1 = DC.DevPointToWorldPoint(p1);

            DrawLine(pen, p0, p1);

            GL.Enable(EnableCap.DepthTest);
        }

        public override void DrawMarkCursor(DrawPen pen, Vector3d p, double pix_size)
        {
            GL.Disable(EnableCap.DepthTest);

            Vector3d size = DC.DevVectorToWorldVector(Vector3d.UnitX * pix_size);
            DrawCross(pen, p, size.Norm());

            GL.Enable(EnableCap.DepthTest);
        }

        public override void DrawRect(DrawPen pen, Vector3d p0, Vector3d p1)
        {
            GL.Disable(EnableCap.DepthTest);

            Vector3d pp0 = DC.WorldPointToDevPoint(p0);
            Vector3d pp2 = DC.WorldPointToDevPoint(p1);

            Vector3d pp1 = pp0;
            pp1.Y = pp2.Y;

            Vector3d pp3 = pp0;
            pp3.X = pp2.X;

            pp0 = DC.DevPointToWorldPoint(pp0);
            pp1 = DC.DevPointToWorldPoint(pp1);
            pp2 = DC.DevPointToWorldPoint(pp2);
            pp3 = DC.DevPointToWorldPoint(pp3);

            DrawLine(pen, pp0, pp1);
            DrawLine(pen, pp1, pp2);
            DrawLine(pen, pp2, pp3);
            DrawLine(pen, pp3, pp0);

            GL.Enable(EnableCap.DepthTest);
        }

        public override void DrawHighlightPoint(Vector3d pt, DrawPen pen)
        {
            Vector3d size = DC.DevVectorToWorldVector(Vector3d.UnitX * 4);
            DrawCross(pen, pt, size.Norm());
        }

        public override void DrawDot(DrawPen pen, Vector3d p)
        {
            GL.Color4(pen.Color4());

            GL.Begin(PrimitiveType.Points);

            GL.Vertex3(p);

            GL.End();
        }

        public override void DrawGrid(Gridding grid)
        {
            if (DC is DrawContextGLOrtho)
            {
                DrawGridOrtho(grid);
            }
            else if (DC is DrawContextGL)
            {
                DrawGridPerse(grid);
            }
        }

        public void DrawGridOrtho(Gridding grid)
        {
            Vector3d lt = Vector3d.Zero;
            Vector3d rb = new Vector3d(DC.ViewWidth, DC.ViewHeight, 0);

            Vector3d ltw = DC.DevPointToWorldPoint(lt);
            Vector3d rbw = DC.DevPointToWorldPoint(rb);

            double minx = Math.Min(ltw.X, rbw.X);
            double maxx = Math.Max(ltw.X, rbw.X);

            double miny = Math.Min(ltw.Y, rbw.Y);
            double maxy = Math.Max(ltw.Y, rbw.Y);

            double minz = Math.Min(ltw.Z, rbw.Z);
            double maxz = Math.Max(ltw.Z, rbw.Z);

            DrawPen pen = DrawPen.New(DC, DrawTools.PEN_GRID);

            Vector3d p = default;

            double n = grid.Decimate(DC, grid, 8);

            double x, y, z;
            double sx, sy, sz;
            double szx = grid.GridSize.X * n;
            double szy = grid.GridSize.Y * n;
            double szz = grid.GridSize.Z * n;

            sx = Math.Round(minx / szx) * szx;
            sy = Math.Round(miny / szy) * szy;
            sz = Math.Round(minz / szz) * szz;

            x = sx;
            while (x < maxx)
            {
                p.X = x;
                p.Z = 0;

                y = sy;

                while (y < maxy)
                {
                    p.Y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                x += szx;
            }

            z = sz;
            y = sy;

            while (z < maxz)
            {
                p.Z = z;
                p.X = 0;

                y = sy;

                while (y < maxy)
                {
                    p.Y = y;
                    DrawDot(pen, p);
                    y += szy;
                }

                z += szz;
            }

            z = sz;
            x = sx;

            while (x < maxx)
            {
                p.X = x;
                p.Y = 0;

                z = sz;

                while (z < maxz)
                {
                    p.Z = z;
                    DrawDot(pen, p);
                    z += szz;
                }

                x += szx;
            }
        }

        public void DrawGridPerse(Gridding grid)
        {
        }

        public override void DrawRectScrn(DrawPen pen, Vector3d pp0, Vector3d pp1)
        {
            Vector3d p0 = DC.DevPointToWorldPoint(pp0);
            Vector3d p1 = DC.DevPointToWorldPoint(pp1);

            DrawRect(pen, p0, p1);
        }

        public override void DrawPageFrame(double w, double h, Vector3d center)
        {
            if (!(DC is DrawContextGLOrtho))
            {
                return;
            }

            Vector3d pt = default(Vector3d);

            // p0
            pt.X = -w / 2 + center.X;
            pt.Y = h / 2 + center.Y;
            pt.Z = 0;

            Vector3d p0 = default(Vector3d);
            p0.X = pt.X * DC.UnitPerMilli;
            p0.Y = pt.Y * DC.UnitPerMilli;

            p0 += DC.ViewOrg;

            // p1
            pt.X = w / 2 + center.X;
            pt.Y = -h / 2 + center.Y;
            pt.Z = 0;

            Vector3d p1 = default(Vector3d);
            p1.X = pt.X * DC.UnitPerMilli;
            p1.Y = pt.Y * DC.UnitPerMilli;

            p1 += DC.ViewOrg;

            DrawRectScrn(DrawPen.New(DC, DrawTools.PEN_PAGE_FRAME), p0, p1);
        }

        public void EnableLight()
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
        }

        public void DisableLight()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);
        }
    }
}
