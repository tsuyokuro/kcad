//#define OPEN_TK_NEXT

using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


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

        public DrawingGL(DrawContextGL dc)
        {
            DC = dc;
        }

        public override void Clear()
        {
            GL.ClearColor(DC.Color(DrawTools.COLOR_BACKGROUND));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void Draw(CadLayer layer, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            Draw(layer.FigureList, pen);
        }

        public override void Draw(IReadOnlyList<CadFigure> list, int pen = DrawTools.PEN_DEFAULT_FIGURE)
        {
            foreach (CadFigure fig in list)
            {
                if (fig.Current)
                {
                    fig.Draw(DC, DrawTools.PEN_FIGURE_HIGHLIGHT);
                }
                else
                {
                    fig.Draw(DC, pen);
                }
            }
        }

        public override void DrawLine(int pen, CadPoint a, CadPoint b)
        {
            GLPen glpen = DC.Pen(pen);

            GL.Begin(LINES);
            GL.Color4(glpen.Color);

            a *= DC.WoldScale;
            b *= DC.WoldScale;

            GL.Vertex3(a.x, a.y, a.z);
            GL.Vertex3(b.x, b.y, b.z);

            GL.End();
        }

        public override void DrawFace(int pen, IReadOnlyList<CadPoint> pointList, CadPoint normal)
        {
            CadPoint p;
            GLPen glpen;

            if (normal.IsZero())
            {
                normal = CadMath.Normal(pointList);
            }

            bool normalValid = !normal.IsZero();


            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);


            GL.Begin(POLYGON);
            GL.Color4(0.8f, 0.8f, 0.8f, 1.0f);

            if (normalValid)
            {
                GL.Normal3(normal.vector);
            }

            foreach (CadPoint pt in pointList)
            {
                p = pt * DC.WoldScale;

                GL.Vertex3(p.vector);
            }

            GL.End();


            #region 輪郭
            // 輪郭は、光源設定を無効化
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            glpen = DC.Pen(pen);

            GL.Color4(glpen.Color);
            GL.LineWidth(1.0f);

            Vector3d t = DC.ViewDir * -1.0f;

            CadPoint shift = (CadPoint)t;

            GL.Begin(LINE_STRIP);

            foreach (CadPoint pt in pointList)
            {
                p = (pt + shift) * DC.WoldScale;
                GL.Vertex3(p.vector);
            }

            CadPoint pt0 = pointList[0];
            p = (pt0 + shift) * DC.WoldScale;

            GL.Vertex3(p.vector);

            GL.End();
            #endregion
        }

        public override void DrawAxis()
        {
            CadPoint p0 = default(CadPoint);
            CadPoint p1 = default(CadPoint);

            double len = 120.0;
            double arrowLen = 12.0;
            double arrowW2 = 6.0;

            // X軸
            p0.x = -len;
            p0.y = 0;
            p0.z = 0;

            p1.x = len;
            p1.y = 0;
            p1.z = 0;

            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            // Y軸
            p0.x = 0;
            p0.y = -len;
            p0.z = 0;

            p1.x = 0;
            p1.y = len;
            p1.z = 0;

            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -len;

            p1.x = 0;
            p1.y = 0;
            p1.z = len;

            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
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

            Matrix4d view = Matrix4d.CreateOrthographicOffCenter(
                                        0, DC.ViewWidth,
                                        DC.ViewHeight, 0,
                                        0, 1000);
            GL.MultMatrix(ref view);
        }

        private void End2D()
        {
            PopMatrixes();
        }

        public override void DrawSelected(CadLayer layer)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            Start2D();

            DrawSelectedFigurePoint(layer.FigureList);

            End2D();
        }

        public override void DrawSelectedPoint(CadPoint pt, int pen = DrawTools.PEN_SLECT_POINT)
        {
            CadPoint p0 = DC.CadPointToUnitPoint(pt) - 2;
            CadPoint p1 = p0 + 4;

            DrawRect2D(p0.vector, p1.vector, pen);
        }

        private void DrawSelectedFigurePoint(IReadOnlyList<CadFigure> list)
        {
            foreach (CadFigure fig in list)
            {
                fig.DrawSelected(DC, DrawTools.PEN_DEFAULT_FIGURE);
            }
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

            GL.Begin(LINE_STRIP);

            GL.Color4(glpen.Color);
            GL.Vertex3(v0);
            GL.Vertex3(v1);
            GL.Vertex3(v2);
            GL.Vertex3(v3);
            GL.Vertex3(v0);

            GL.End();
        }

        public override void DrawDownPointCursor(int pen, CadPoint p)
        {
            DrawCross(pen, p, 10.0);
        }

        public override void DrawCross(int pen, CadPoint p, double size)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            double hs = size;

            CadPoint px0 = p;
            px0.x -= hs;
            CadPoint px1 = p;
            px1.x += hs;

            CadPoint py0 = p;
            py0.y -= hs;
            CadPoint py1 = p;
            py1.y += hs;

            CadPoint pz0 = p;
            pz0.z -= hs;
            CadPoint pz1 = p;
            pz1.z += hs;

            DrawLine(pen, px0, px1);
            DrawLine(pen, py0, py1);
            DrawLine(pen, pz0, pz1);
        }
    }
}
