using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CadDataTypes;
using System.Windows.Forms;

namespace Plotter
{
    class DrawContextGLOrtho : DrawContextGL
    {
        CadVector Center = default;

        public DrawContextGLOrtho()
        {
            Init(null);
        }

        public DrawContextGLOrtho(Control control)
        {
            Init(control);
        }

        public override void Active()
        {
            CalcProjectionMatrix();
        }

        public override CadVector WorldPointToDevPoint(CadVector pt)
        {
            CadVector p = WorldVectorToDevVector(pt);
            p = p + Center;
            return p;
        }

        public override CadVector DevPointToWorldPoint(CadVector pt)
        {
            pt = pt - Center;
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

            dv.X = dv.X * (DeviceScaleX);
            dv.Y = dv.Y * (DeviceScaleY);
            dv.Z = 0;

            return CadVector.Create(dv);
        }

        public override CadVector DevVectorToWorldVector(CadVector pt)
        {
            pt.x = pt.x / (DeviceScaleX);
            pt.y = pt.y / (DeviceScaleY);

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

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            mViewOrg.x = w / 2.0;
            mViewOrg.y = h / 2.0;

            DeviceScaleX = w / 2.0;
            DeviceScaleY = -h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            CalcProjectionMatrix();
            CalcProjectionZW();

            Center.x = w / 2;
            Center.y = h / 2;

            Matrix2D = Matrix4d.CreateOrthographicOffCenter(
                                        0, mViewWidth,
                                        mViewHeight, 0,
                                        0, mProjectionFar);
        }

        public override void CalcProjectionMatrix()
        {
            mProjectionMatrix = Matrix4d.CreateOrthographic(
                                            ViewWidth / mUnitPerMilli, ViewHeight / mUnitPerMilli,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            double dx = ViewOrg.x - (ViewWidth / 2.0);
            double dy = ViewOrg.y - (ViewHeight / 2.0);

            mProjectionMatrix.M41 = dx / (ViewWidth / 2.0);
            mProjectionMatrix.M42 = -dy / (ViewHeight / 2.0);

            mProjectionMatrixInv = mProjectionMatrix.Invert();
        }
    }
}