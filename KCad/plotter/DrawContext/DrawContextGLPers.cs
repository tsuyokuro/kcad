using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CadDataTypes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGLPers : DrawContextGL
    {
        public DrawContextGLPers()
        {
            Init(null);
            mUnitPerMilli = 1;
        }

        public DrawContextGLPers(Control control)
        {
            Init(control);
            mUnitPerMilli = 1;
        }

        public override void StartDraw()
        {
            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.Matrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mProjectionMatrix.Matrix);

            SetupLight();
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

            Matrix2D = Matrix4d.CreateOrthographicOffCenter(
                                        0, mViewWidth,
                                        mViewHeight, 0,
                                        0, mProjectionFar);
        }

        public override void SetViewOrg(CadVector org)
        {
            mViewOrg = org;
        }

        public override void CalcProjectionMatrix()
        {
            double aspect = mViewWidth / mViewHeight;
            mProjectionMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            mFovY,
                                            aspect,
                                            mProjectionNear,
                                            mProjectionFar
                                            );

            mProjectionMatrixInv = mProjectionMatrix.Invert();
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

            dv.X = dv.X * DeviceScaleX;
            dv.Y = dv.Y * DeviceScaleY;
            dv.Z = 0;

            return CadVector.Create(dv);
        }

        public override CadVector DevVectorToWorldVector(CadVector pt)
        {
            pt.x = pt.x / DeviceScaleX;
            pt.y = pt.y / DeviceScaleY;

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

        public override DrawContext CreatePrinterContext(CadSize2D pageSize, CadSize2D deviceSize)
        {
            DrawContextGL dc = new DrawContextGLPers();

            dc.CopyMetrics(this);

            dc.CopyCamera(this);
            dc.SetViewSize(deviceSize.Width, deviceSize.Height);

            CadVector org = default;
            org.x = deviceSize.Width / 2.0;
            org.y = deviceSize.Height / 2.0;

            dc.SetViewOrg(org);

            dc.UnitPerMilli = deviceSize.Width / pageSize.Width;

            return dc;
        }

        public void RotateEyePoint(Vector2 prev, Vector2 current)
        {
            Vector2 d = current - prev;

            double ry = (d.X / 10.0) * (Math.PI / 20);
            double rx = (d.Y / 10.0) * (Math.PI / 20);

            CadQuaternion q;
            CadQuaternion r;
            CadQuaternion qp;

            q = CadQuaternion.RotateQuaternion(Vector3d.UnitY, ry);

            r = q.Conjugate();

            qp = CadQuaternion.FromVector(mEye);
            qp = r * qp;
            qp = qp * q;
            mEye = qp.ToVector3d();

            qp = CadQuaternion.FromVector(mUpVector);
            qp = r * qp;
            qp = qp * q;
            mUpVector = qp.ToVector3d();

            Vector3d ev = mLookAt - mEye;

            CadVector a = CadVector.Create(ev);
            CadVector b = CadVector.Create(mUpVector);

            CadVector axis = CadMath.Normal(a, b);

            if (!axis.IsZero())
            {

                q = CadQuaternion.RotateQuaternion(axis.vector, rx);

                r = q.Conjugate();

                qp = CadQuaternion.FromVector(mEye);
                qp = r * qp;
                qp = qp * q;

                mEye = qp.ToVector3d();

                qp = CadQuaternion.FromVector(mUpVector);
                qp = r * qp;
                qp = qp * q;
                mUpVector = qp.ToVector3d();
            }

            CalcViewMatrix();
            CalcViewDir();
            CalcProjectionZW();
        }

        public void MoveForwardEyePoint(double d, bool withLookAt = false)
        {
            Vector3d dv = ViewDir * d;

            if (withLookAt)
            {
                mEye += dv;
                mLookAt += dv;
            }
            else
            {
                Vector3d eye = mEye + dv;

                Vector3d viewDir = mLookAt - eye;

                viewDir.Normalize();

                if ((ViewDir - viewDir).Length > 1.0)
                {
                    return;
                }

                mEye += dv;
            }

            CalcViewMatrix();
            CalcProjectionMatrix();
            CalcProjectionZW();
            CalcViewDir();
        }
    }
}
