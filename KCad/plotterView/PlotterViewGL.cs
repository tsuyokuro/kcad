﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;

namespace Plotter
{
    class PlotterViewGL : GLControl, IPlotterView
    {
        private DrawContext mDrawContext = new DrawContextGL();

        private PlotterController mController = null;

        public DrawContext DrawContext
        {
            get
            {
                return mDrawContext;
            }
        }

        public PaperPageSize PageSize
        {
            get
            {
                return mDrawContext.PageSize;
            }
        }

        public static PlotterViewGL Create()
        {
            GraphicsMode mode = GraphicsMode.Default;
            return Create(mode);
        }

        public static PlotterViewGL Create(GraphicsMode mode)
        {
            PlotterViewGL v = new PlotterViewGL(mode);
            return v;
        }

        private PlotterViewGL(GraphicsMode mode) : base(mode)
        {
            Load += onLoad;
            Resize += onResize;
            Paint += onPaint;
            MouseMove += onMouseMove;
            SwapBuffers();
        }

        private void onLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            SwapBuffers();
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            //SwapBuffers();
        }

        private void onResize(object sender, EventArgs e)
        {
            mDrawContext.setViewSize(Size.Width, Size.Height);

            if (mController != null)
            {
                DrawContext dc = startDraw();
                mController.clear(dc);
                mController.draw(dc);
                endDraw();
            }
        }

        public DrawContext startDraw()
        {
            MakeCurrent();
            mDrawContext.startDraw();
            return mDrawContext;
        }

        public void endDraw()
        {
            Console.Write("PlotterViewGL ensDraw()");

            mDrawContext.endDraw();
            SwapBuffers();
        }

        public void SetController(PlotterController controller)
        {
            mController = controller;
        }
    }
}
