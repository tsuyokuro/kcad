using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawingGL : DrawingBase
    {
        private DrawContextGL DC;

        public DrawingGL(DrawContextGL dc)
        {
            DC = dc;
        }

        public override void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(DC.Color(DrawTools.COLOR_BACKGROUND));
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

            GL.Begin(PrimitiveType.Lines);
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
            GL.Begin(PrimitiveType.Polygon);
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

            #region 裏面
            if (DC.LightingEnable)
            {
                // 裏面
                GL.Begin(PrimitiveType.Polygon);
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

            #region 輪郭
            // 輪郭は、光源設定を無効化
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Light0);

            glpen = DC.Pen(pen);

            GL.Color4(glpen.Color);
            GL.LineWidth(1.0f);

            Vector3d t = DC.ViewDir * -1.0f;

            CadPoint shift = (CadPoint)t;

            GL.Begin(PrimitiveType.LineStrip);
 
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
    }
}
