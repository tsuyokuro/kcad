using OpenTK;
using System.Drawing;
using System.Drawing.Imaging;
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

            get => PixelOffsetMode;
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
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            RecalcProjectionMatrix();

            mDrawing = new DrawingGDI(this);
        }

        private void RecalcProjectionMatrix()
        {
            mProjectionMatrix.GLMatrix = Matrix4d.CreateOrthographic(mViewWidth, mViewHeight, mProjectionNear, mProjectionFar);
            mProjectionMatrixInv.GLMatrix = Matrix4d.Invert(mProjectionMatrix.GLMatrix);

            Vector4d wv = Vector4d.Zero;
            wv.W = 1.0f;
            wv.Z = -mEye.Length;

            Vector4d pv = wv * mProjectionMatrix;

            mProjectionW = pv.W;
            mProjectionZ = pv.Z;
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            if (w == 0 || h==0)
            {
                return;
            }

            DisposeGraphics();

            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;

            Buffer = currentContext.Allocate(ViewCtrl.CreateGraphics(),
                               ViewCtrl.DisplayRectangle);

            mGdiGraphics = Buffer.Graphics;

            mGdiGraphics.SmoothingMode = mSmoothingMode;
            mGdiGraphics.PixelOffsetMode = mPixelOffsetMode;
        }

        private void DisposeGraphics()
        {
            if (Buffer != null)
            {
                Buffer.Dispose();
                Buffer = null;
            }
        }

        public override CadVector WorldPointToDevPoint(CadVector pt)
        {
            CadVector p = WorldVectorToDevVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public override CadVector DevPointToWorldPoint(CadVector pt)
        {
            pt = pt - mViewOrg;
            return DevVectorToWorldVector(pt);
        }

        public override CadVector WorldVectorToDevVector(CadVector pt)
        {
            pt *= WorldScale;

            Vector4d wv = (Vector4d)pt;

            wv.W = 1.0f;

            Vector4d sv = wv * mViewMatrix;
            Vector4d pv = sv * mProjectionMatrix;

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            dv.X = dv.X * (mUnitPerMilli * DeviceScaleX);
            dv.Y = dv.Y * (mUnitPerMilli * DeviceScaleY);
            dv.Z = 0;

            return CadVector.Create(dv);
        }

        public override CadVector DevVectorToWorldVector(CadVector pt)
        {
            pt.x = pt.x / (mUnitPerMilli * DeviceScaleX);
            pt.y = pt.y / (mUnitPerMilli * DeviceScaleY);

            Vector4d wv;

            wv.W = mProjectionW;
            wv.Z = mProjectionZ;

            wv.X = pt.x * wv.W;
            wv.Y = pt.y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WorldScale;

            return CadVector.Create(wv);
        }


        public override void Dispose()
        {
            DisposeGraphics();
            Tools.Dispose();
        }

        public Pen Pen(int id)
        {
            return Tools.pen(id);
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
            return Tools.brush(id);
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
    }
}
