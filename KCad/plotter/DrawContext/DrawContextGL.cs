using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Plotter
{
    class DrawContextGL : DrawContext
    {
        Vector4 LightPosition;
        Color4 LightAmbient;    // 環境光
        Color4 LightDiffuse;    // 拡散光
        Color4 LightSpecular;   // 鏡面反射光

        Color4 MaterialAmbient;
        Color4 MaterialDiffuse;
        Color4 MaterialSpecular;
        Color4 MaterialShininess;

        public bool LightingEnable = true;


        public enum ProjectionType
        {
            TELESCOPE,
            STANDERD,
            WIDE_ANGLE,
        }

        public DrawContextGL()
        {
            //WoldScale = 0.2f;

            WoldScale = 1.0f;
            Tools.Setup(DrawTools.ToolsType.DARK_GL);

            mDrawing = new DrawingGL(this);

            InitCamera(ProjectionType.STANDERD);

            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            /*
            LightPosition = new Vector4(150f, 150f, 150f, 0.0f);
            LightAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            LightDiffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            LightSpecular = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            */

            LightPosition = new Vector4(150.0f, 150f, 500.0f, 0.0f);
            LightAmbient = new Color4(0.8f, 0.8f, 0.8f, 1.0f);
            LightDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            LightSpecular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            MaterialAmbient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            MaterialDiffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            MaterialSpecular = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            MaterialShininess = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

            RecalcViewDirFromCameraDirection();
        }

        public void InitCamera(ProjectionType type)
        {
            double a = 1.0;

            double ez = 1000.0;
            double near = 500.0;
            double far = 1500.0;


            // FovY 視野角を指定
            // 視野角に合わせて視点、視錐台の近遠を調整

            switch (type)
            {
                case ProjectionType.TELESCOPE:
                    // 望遠
                    a = 2.0;

                    Eye = Vector3d.Zero;
                    Eye.X = 0.0;
                    Eye.Y = 0.0;
                    Eye.Z = ez / a;

                    FovY = Math.PI / 4;

                    ProjectionNear = near / a;
                    ProjectionFar = far / a;

                    LookAt = Vector3d.Zero;
                    UpVector = Vector3d.UnitY;

                    break;
                case ProjectionType.STANDERD:
                    a = 4.0;

                    Eye = Vector3d.Zero;
                    Eye.X = 0.0;
                    Eye.Y = 0.0;
                    Eye.Z = ez / a;

                    FovY = Math.PI / 3;

                    ProjectionNear = near / a;
                    ProjectionFar = far / a;

                    LookAt = Vector3d.Zero;
                    UpVector = Vector3d.UnitY;

                    break;

                case ProjectionType.WIDE_ANGLE:
                    a = 4;

                    Eye = Vector3d.Zero;
                    Eye.X = 0.0;
                    Eye.Y = 0.0;
                    Eye.Z = ez / a;

                    FovY = Math.PI / 2;

                    ProjectionNear = near / a;
                    ProjectionFar = far / a;

                    LookAt = Vector3d.Zero;
                    UpVector = Vector3d.UnitY;

                    break;
            }
        }

        public GLPen Pen(int id)
        {
            return Tools.glpen(id);
        }

        public Color4 Color(int id)
        {
            return Tools.glcolor(id);
        }

        public override void SetupTools(DrawTools.ToolsType type)
        {
            if (DrawTools.IsTypeForGL(type))
            {
                Tools.Setup(type);
            }
        }

        public override void StartDraw()
        {
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mViewMatrix.GLMatrix);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref mProjectionMatrix.GLMatrix);

            SetupLight();
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
            pt *= WoldScale;

            // 透視変換用にWが必要なので、Vector4に変換
            Vector4d wv = (Vector4d)pt;

            wv.W = 1.0f;

            Vector4d sv = wv * mViewMatrix;
            Vector4d pv = sv * mProjectionMatrix;

            Vector4d dv;

            dv.X = pv.X / pv.W;
            dv.Y = pv.Y / pv.W;
            dv.Z = pv.Z / pv.W;
            dv.W = pv.W;

            dv.X = dv.X * (ViewWidth / 2.0);
            dv.Y = -dv.Y * (ViewHeight / 2.0);
            dv.Z = 0;

            return CadVector.Create(dv);
        }

        public override CadVector UnitVectorToCadVector(CadVector pt)
        {
            pt.x = pt.x / (ViewWidth / 2.0);
            pt.y = -pt.y / (ViewHeight / 2.0);

            Vector3d epv = pt.vector - Eye;

            Vector4d wv;

            wv.W = epv.Length;

            // mProjectionMatrixInvに掛けて wv.W=1.0 となる z を求める
            //wv.Z = (ProjectionMatrix.M33 * (-wv.W)) + (ProjectionMatrix.M43);
            wv.Z = (1.0 - (wv.W * mProjectionMatrixInv.M44)) / mProjectionMatrixInv.M34;

            wv.X = pt.x * wv.W;
            wv.Y = pt.y * wv.W;

            wv = wv * mProjectionMatrixInv;
            wv = wv * mViewMatrixInv;

            wv /= WoldScale;

            return CadVector.Create(wv);
        }

        private void SetupLight()
        {
            if (!LightingEnable)
            {
                return;
            }

            // 裏面を描かない
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);
            //GL.FrontFace(FrontFaceDirection.Ccw);

            //法線の正規化
            //GL.Enable(EnableCap.Normalize);

            GL.Light(LightName.Light0, LightParameter.Position, LightPosition);
            GL.Light(LightName.Light0, LightParameter.Ambient, LightAmbient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, LightDiffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, LightSpecular);

            /*
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, MaterialAmbient);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, MaterialDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, MaterialSpecular);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, MaterialShininess);
            */
        }

        public override void SetViewSize(double w, double h)
        {
            mViewWidth = w;
            mViewHeight = h;

            mViewOrg.x = w / 2.0;
            mViewOrg.y = h / 2.0;

            GL.Viewport(0, 0, (int)mViewWidth, (int)mViewHeight);

            //double aspect = mViewWidth / mViewHeight;

            //mProjectionMatrix.GLMatrix = Matrix4d.CreatePerspectiveFieldOfView(
            //                                FovY,
            //                                aspect,
            //                                ProjectionNear,
            //                                ProjectionFar
            //                                );
            //mProjectionMatrixInv.GLMatrix = Matrix4d.Invert(mProjectionMatrix.GLMatrix);

            RecalcMatrix();
        }

        private void RecalcMatrix()
        {
            double aspect = mViewWidth / mViewHeight;

            mProjectionMatrix.GLMatrix = Matrix4d.CreatePerspectiveFieldOfView(
                                            FovY,
                                            aspect,
                                            ProjectionNear,
                                            ProjectionFar
                                            );

            mProjectionMatrixInv.GLMatrix = Matrix4d.Invert(mProjectionMatrix.GLMatrix);
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

            qp = CadQuaternion.FromVector(Eye);
            qp = r * qp;
            qp = qp * q;
            Eye = qp.ToVector3d();

            qp = CadQuaternion.FromVector(UpVector);
            qp = r * qp;
            qp = qp * q;
            UpVector = qp.ToVector3d();

            Vector3d ev = LookAt - Eye;

            CadVector a = CadVector.Create(ev);
            CadVector b = CadVector.Create(UpVector);

            CadVector axis = CadMath.Normal(a, b);

            if (!axis.IsZero())
            {

                q = CadQuaternion.RotateQuaternion(axis.vector, rx);

                r = q.Conjugate();

                qp = CadQuaternion.FromVector(Eye);
                qp = r * qp;
                qp = qp * q;

                Eye = qp.ToVector3d();

                qp = CadQuaternion.FromVector(UpVector);
                qp = r * qp;
                qp = qp * q;
                UpVector = qp.ToVector3d();
            }

            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            RecalcViewDirFromCameraDirection();
        }

        public void MoveForwardEyePoint(double d)
        {
            Eye += ViewDir * d;
            ProjectionNear -= d;

            mViewMatrix.GLMatrix = Matrix4d.LookAt(Eye, LookAt, UpVector);
            mViewMatrixInv.GLMatrix = Matrix4d.Invert(mViewMatrix.GLMatrix);

            RecalcMatrix();
        }

        public override void Dispose()
        {
            Tools.Dispose();
        }
    }
}