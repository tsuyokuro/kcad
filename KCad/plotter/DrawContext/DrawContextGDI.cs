﻿using OpenTK;
using System.Drawing;
using System.Drawing.Imaging;
using CadDataTypes;
using System.Windows.Forms;

namespace Plotter
{
    public class DrawContextGDI : DrawContext
    {
        protected Control Wnd;

        private Graphics mGdiGraphics = null;

        private Bitmap mImage = null;

        private BitmapData LockedBitmapData = null;

        private Rectangle Rect = default(Rectangle);

        public Graphics GdiGraphics
        {
            protected set { mGdiGraphics = value; }
            get { return mGdiGraphics; }
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
            Init(null);
        }

        public DrawContextGDI(Control control)
        {
            Init(control);
        }

        private void Init(Control control)
        {
            Wnd = control;

            SetViewSize(8, 1);  // Create dummy Image and Graphics

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
            mGdiGraphics = Graphics.FromImage(mImage);

            Rect.X = 0;
            Rect.Y = 0;
            Rect.Width = (int)mViewWidth;
            Rect.Height = (int)mViewHeight;
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

        public override CadVector DevVectorToWorldVector(CadVector pt)
        {
            Vector4d wv = default(Vector4d);
            wv.X = pt.x / (UnitPerMilli * DeviceScaleX);
            wv.Y = pt.y / (UnitPerMilli * DeviceScaleY);
            //wv.Z = pt.z / UnitPerMilli;
            wv.Z = pt.z;

            wv = wv * mProjectionMatrixInv;

            wv = wv * mViewMatrixInv;

            wv /= WorldScale;

            return CadVector.Create(wv);
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

            LockedBitmapData = mImage.LockBits(
                    Rect,
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
