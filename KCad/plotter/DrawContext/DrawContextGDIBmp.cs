using OpenTK;
using System.Drawing;
using System.Drawing.Imaging;
using CadDataTypes;
using System.Windows.Forms;

namespace Plotter
{
    public class DrawContextGDIBmp : DrawContextGDI
    {
        private BitmapData LockedBitmapData = null;

        private Bitmap mImage = null;
        public Bitmap Image
        {
            get => mImage;
        }

        public DrawContextGDIBmp()
        {
            Init();
        }

        private void Init()
        {
            SetViewSize(8, 1);  // Create dummy Image and Graphics

            mUnitPerMilli = 4; // 4 pix = 1mm
            mViewOrg.x = 0;
            mViewOrg.y = 0;

            mProjectionMatrix = UMatrix4.Unit;
            mProjectionMatrixInv = UMatrix4.Unit;

            mDrawing = new DrawingGDIBmp(this);

            CalcProjectionMatrix();
            CalcProjectionZW();
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            if (w == 0 || h == 0)
            {
                return;
            }

            DisposeGraphics();

            mImage = new Bitmap((int)mViewWidth, (int)mViewHeight);
            mGdiGraphics = Graphics.FromImage(mImage);

            CalcProjectionMatrix();
            CalcProjectionZW();
        }

        private void DisposeGraphics()
        {
            if (mGdiGraphics != null)
            {
                mGdiGraphics.Dispose();
                mGdiGraphics = null;
            }

            if (mImage != null)
            {
                mImage.Dispose();
                mImage = null;
            }
        }

        public BitmapData GetLockedBits()
        {
            return LockedBitmapData;
        }

        public BitmapData LockBits()
        {
            if (mImage == null)
            {
                return null;
            }

            if (LockedBitmapData != null)
            {
                return LockedBitmapData;
            }

            Rectangle r = new Rectangle(0, 0, mImage.Width, mImage.Height);

            LockedBitmapData = mImage.LockBits(
                    r,
                    ImageLockMode.ReadWrite, mImage.PixelFormat);

            return LockedBitmapData;
        }

        public void UnlockBits()
        {
            if (mImage == null)
            {
                return;
            }

            if (LockedBitmapData == null)
            {
                return;
            }

            mImage.UnlockBits(LockedBitmapData);
            LockedBitmapData = null;
        }

        public override void Dispose()
        {
            DisposeGraphics();
            Tools.Dispose();
        }
    }
}
