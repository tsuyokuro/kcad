using OpenTK;
using System.Drawing;
using CadDataTypes;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Plotter
{
    public class DrawContextGDI : DrawContext
    {
        protected Control ViewCtrl;

        BufferedGraphics Buffer;

        protected Graphics mGdiGraphics = null;
        public Graphics GdiGraphics
        {
            protected set => mGdiGraphics = value;
            get => mGdiGraphics;
        }

        public override double UnitPerMilli
        {
            set
            {
                mUnitPerMilli = value;
                CalcProjectionMatrix();
            }

            get => mUnitPerMilli;
        }

        private SmoothingMode mSmoothingMode = SmoothingMode.HighSpeed;

        public SmoothingMode SmoothingMode
        {
            set
            {
                mSmoothingMode = value;
                if (mGdiGraphics != null)
                {
                    mGdiGraphics.SmoothingMode = mSmoothingMode;
                }
            }

            get => mSmoothingMode;
        }

        private PixelOffsetMode mPixelOffsetMode = PixelOffsetMode.HighSpeed;

        public PixelOffsetMode PixelOffsetMode
        {
            set
            {
                mPixelOffsetMode = value;
                if (mGdiGraphics != null)
                {
                    mGdiGraphics.PixelOffsetMode = mPixelOffsetMode;
                }
            }

            get => mPixelOffsetMode;
        }

        public DrawContextGDI()
        {
        }

        public DrawContextGDI(Control control)
        {
            Init(control);
        }

        private void Init(Control control)
        {
            ViewCtrl = control;

            SetViewSize(1, 1);  // Create dummy Graphics

            mUnitPerMilli = 4; // 4 pix = 1mm
            mViewOrg.X = 0;
            mViewOrg.Y = 0;

            CalcProjectionMatrix();
            CalcProjectionZW();

            mDrawing = new DrawingGDI(this);
        }

        public override void SetViewOrg(Vector3d org)
        {
            mViewOrg = org;
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            if (w == 0 || h == 0)
            {
                return;
            }

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            CalcProjectionMatrix();
            CalcProjectionZW();

            DisposeGraphics();
            CreateGraphics();
        }

        protected virtual void DisposeGraphics()
        {
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }

        protected virtual void CreateGraphics()
        {
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;

            Buffer = currentContext.Allocate(ViewCtrl.CreateGraphics(),
                               ViewCtrl.DisplayRectangle);

            mGdiGraphics = Buffer.Graphics;

            mGdiGraphics.SmoothingMode = mSmoothingMode;
            mGdiGraphics.PixelOffsetMode = mPixelOffsetMode;
        }

        public override void Dispose()
        {
            DisposeGraphics();
            Tools.Dispose();
            mDrawing.Dispose();
        }

        public override void CalcProjectionMatrix()
        {
            mProjectionMatrix = Matrix4d.CreateOrthographic(
                                            ViewWidth / mUnitPerMilli, ViewHeight / mUnitPerMilli,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }

        public Pen Pen(int id)
        {
            DrawPen pen = Tools.Pen(id);
            return pen.GdiPen;
        }

        public Color PenColor(int id)
        {
            return Tools.PenColorTbl[id];
        }

        public Font Font(int id)
        {
            return Tools.font(id);
        }

        public Brush Brush(int id)
        {
            DrawBrush brush = Tools.Brush(id);
            return brush.GdiBrush;
        }

        public Color BrushColor(int id)
        {
            return Tools.BrushColorTbl[id];
        }

        public void Refresh()
        {
            if (Buffer != null)
                Buffer.Render();
        }

        public override DrawPen GetPen(int idx)
        {
            return Tools.Pen(idx);
        }

        public override DrawBrush GetBrush(int idx)
        {
            return Tools.Brush(idx);
        }

        public override DrawContext Clone()
        {
            DrawContextGDI dc = new DrawContextGDI();

            dc.CopyProjectionMetrics(this);
            dc.WorldScale = WorldScale;

            dc.CopyCamera(this);
            dc.SetViewSize(ViewWidth, ViewHeight);

            dc.SetViewOrg(ViewOrg);

            return dc;
        }
    }
}
