//#define OPEN_TK_NEXT

using System.Collections.Generic;

using OpenTK;
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

        public override void Draw(CadLayer layer)
        {
            Draw(layer.FigureList);
        }

        public override void Draw(IReadOnlyList<CadFigure> list, int pen = -1)
        {
            if (pen == -1)
            {
                pen = DrawTools.PEN_DEFAULT_FIGURE;
            }

            foreach (CadFigure fig in list)
            {
                fig.draw(DC, pen);
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

        public override void DrawFace(int pen, IReadOnlyList<CadPoint> pointList)
        {
            CadPoint p;
            GLPen glpen;

            CadPoint normal = CadMath.Normal(pointList);
            bool normalValid = !normal.IsZero();


            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            #region 表面
            GL.Begin(POLYGON);
            GL.Color4(0.6f,0.6f,0.6f,1.0f);

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
            #endregion

            /*
            #region 裏面
            if (DC.LightingEnable)
            {
                // 裏面
                GL.Begin(POLYGON);
                GL.Color4(0.6f, 0.6f, 0.6f, 1.0f);

                if (normalValid)
                {
                    GL.Normal3(normal.vector);
                }

                int i = pointList.Count - 1;

                for (; i >= 0; i--)
                {
                    CadPoint pt = pointList[i];
                    p = pt * DC.WoldScale;

                    GL.Vertex3(p.vector);
                }

                GL.End();
            }
            #endregion
            */

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

            //DrawLine(DrawTools.PEN_AXIS, p0, p1);
            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            // Y軸
            p0.x = 0;
            p0.y = -len;
            p0.z = 0;

            p1.x = 0;
            p1.y = len;
            p1.z = 0;

            //DrawLine(DrawTools.PEN_AXIS, p0, p1);
            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);

            // Z軸
            p0.x = 0;
            p0.y = 0;
            p0.z = -len;

            p1.x = 0;
            p1.y = 0;
            p1.z = len;

            //DrawLine(DrawTools.PEN_AXIS, p0, p1);
            DrawArrow(DrawTools.PEN_AXIS, p0, p1, ArrowTypes.CROSS, ArrowPos.END, arrowLen, arrowW2);
        }
    }
}
