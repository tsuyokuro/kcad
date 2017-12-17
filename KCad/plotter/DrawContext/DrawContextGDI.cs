using OpenTK;
using OpenTK.Graphics;
using System.Drawing;

namespace Plotter
{
    public class DrawContextGDI : DrawContext
    {
        private int GraphicsRef = 0;

        protected Graphics mGraphics = null;

        private Bitmap mImage = null;

        public Graphics graphics
        {
            set { mGraphics = value; }
            get { return mGraphics; }
        }

        public Bitmap Image
        {
            get
            {
                return mImage;
            }
        }

        public DrawContextGDI()
        {
            SetUnitPerMilli(4); // 4 pix = 1mm
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            mProjectionMatrix = UMatrix4.Unit;
            mProjectionMatrixInv = UMatrix4.Unit;

            mDrawing = new DrawingGDI(this);
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


            mImage = new Bitmap((int)mViewWidth, (int)mViewHeight);
            mGraphics = Graphics.FromImage(mImage);
        }

        private void DisposeGraphics()
        {
            if (mGraphics != null)
            {
                mGraphics.Dispose();
                mGraphics = null;
            }

            if (mImage != null)
            {
                mImage.Dispose();
                mImage = null;
            }
        }

        public override CadVector CadPointToUnitPoint(CadVector pt)
        {
            CadVector p = CadVectorToUnitVector(pt);
            p = p + mViewOrg;
            return p;
        }

        public override CadVector UnitPointToCadPoint(CadVector pt)
        {
            pt = pt - mViewOrg;
            return UnitVectorToCadVector(pt);
        }

        public override CadVector CadVectorToUnitVector(CadVector pt)
        {
            Vector4d ptv = (Vector4d)pt;

            ptv.W = 1.0f;

            ptv = ptv * mViewMatrix;
            ptv = ptv * mProjectionMatrix;

            ptv.X /= ptv.W;
            ptv.Y /= ptv.W;
            ptv.Z /= ptv.W;

            CadVector p = default(CadVector);

            p.x = ptv.X * (UnitPerMilli * DeviceScaleX);
            p.y = ptv.Y * (UnitPerMilli * DeviceScaleY);
            //p.z = ptv.Z * UnitPerMilli;
            p.z = 0;

            return p;
        }

        public override CadVector UnitVectorToCadVector(CadVector pt)
        {
            Vector4d wv = default(Vector4d);
            wv.X = pt.x / (UnitPerMilli * DeviceScaleX);
            wv.Y = pt.y / (UnitPerMilli * DeviceScaleY);
            //wv.Z = pt.z / UnitPerMilli;
            wv.Z = pt.z;

            wv = wv * mProjectionMatrixInv;

            wv = wv * mViewMatrixInv;

            wv /= WoldScale;

            return CadVector.Create(wv);
        }

        public override void Dispose()
        {
            DisposeGraphics();
            Tools.Dispose();
        }

        #region Depend GDI Graphics
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
        #endregion
    }
}
