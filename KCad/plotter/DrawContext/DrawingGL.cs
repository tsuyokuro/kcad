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
    class DrawingGL : DrawingX
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
            //Console.WriteLine("DrawingGL Draw (FigureList)");

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
    }
}
