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

        public override void Draw(CadLayer layer)
        {
            //Console.WriteLine("DrawingGL Draw (layer)");
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

            a *= DC.Scale;
            b *= DC.Scale;

            GL.Vertex3(a.x, a.y, a.z);
            GL.Vertex3(b.x, b.y, b.z);

            GL.End();
        }

        /*
        public override void Draw(IReadOnlyList<CadFigure> list, int pen = -1)
        {
            float w2 = 1.0f;
            float z = 0.0f;

            GL.Begin(PrimitiveType.Lines);

            GL.Color4(Color4.White);
            GL.Vertex3(-w2, w2, z);
            GL.Vertex3(-w2, -w2, z);

            GL.Color4(Color4.Red);
            GL.Vertex3(-w2, -w2, z);
            GL.Vertex3(w2, -w2, z);

            GL.Color4(Color4.Lime);
            GL.Vertex3(w2, -w2, z);
            GL.Vertex3(w2, w2, z);

            GL.Color4(Color4.Blue);
            GL.Vertex3(w2, w2, z);
            GL.Vertex3(-w2, w2, z);


            GL.Color4(Color4.White);
            GL.Vertex3(-w2, -w2, 0);
            GL.Vertex3(-w2, -w2, -w2 * 2);

            GL.Color4(Color4.White);
            GL.Vertex3(-w2, -w2, -w2 * 2);
            GL.Vertex3(w2, -w2, -w2 * 2);

            GL.Color4(Color4.White);
            GL.Vertex3(w2, -w2, -w2 * 2);
            GL.Vertex3(w2, -w2, 0);

            GL.End();
        }
        */
    }
}
