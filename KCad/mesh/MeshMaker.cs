using CadDataTypes;
using MeshUtilNS;
using Plotter;
using System;

namespace MeshMakerNS
{
    public class MeshMaker
    {
        public enum FaceType
        {
            TRIANGLE,
            QUADRANGLE,
        }

        public static CadMesh CreateBox(CadVertex pos, CadVertex sv, FaceType faceType = FaceType.TRIANGLE)
        {
            CadMesh cm = CreateUnitCube(faceType);

            for (int i=0;i<cm.VertexStore.Count; i++)
            {
                cm.VertexStore.Ref(i) *= sv;
                cm.VertexStore.Ref(i) += pos;
            }

            return cm;
        }

        // 単位立方体作成
        public static CadMesh CreateUnitCube(FaceType faceType)
        {
            CadMesh cm = new CadMesh(8, 12);

            cm.VertexStore.Add(CadVertex.Create(+0.5, +0.5, +0.5));
            cm.VertexStore.Add(CadVertex.Create(-0.5, +0.5, +0.5));
            cm.VertexStore.Add(CadVertex.Create(-0.5, -0.5, +0.5));
            cm.VertexStore.Add(CadVertex.Create(+0.5, -0.5, +0.5));

            cm.VertexStore.Add(CadVertex.Create(+0.5, +0.5, -0.5));
            cm.VertexStore.Add(CadVertex.Create(-0.5, +0.5, -0.5));
            cm.VertexStore.Add(CadVertex.Create(-0.5, -0.5, -0.5));
            cm.VertexStore.Add(CadVertex.Create(+0.5, -0.5, -0.5));

            if (faceType == FaceType.QUADRANGLE)
            {
                cm.FaceStore.Add(new CadFace(0, 1, 2, 3));

                cm.FaceStore.Add(new CadFace(7, 6, 5, 4));

                cm.FaceStore.Add(new CadFace(0, 4, 5, 1));

                cm.FaceStore.Add(new CadFace(1, 5, 6, 2));

                cm.FaceStore.Add(new CadFace(2, 6, 7, 3));

                cm.FaceStore.Add(new CadFace(3, 7, 4, 0));
            }
            else
            {
                cm.FaceStore.Add(new CadFace(0, 1, 2));
                cm.FaceStore.Add(new CadFace(2, 3, 0));

                cm.FaceStore.Add(new CadFace(7, 6, 5));
                cm.FaceStore.Add(new CadFace(5, 4, 7));

                cm.FaceStore.Add(new CadFace(0, 4, 5));
                cm.FaceStore.Add(new CadFace(5, 1, 0));

                cm.FaceStore.Add(new CadFace(1, 5, 6));
                cm.FaceStore.Add(new CadFace(6, 2, 1));

                cm.FaceStore.Add(new CadFace(2, 6, 7));
                cm.FaceStore.Add(new CadFace(7, 3, 2));

                cm.FaceStore.Add(new CadFace(3, 7, 4));
                cm.FaceStore.Add(new CadFace(4, 0, 3));
            }

            return cm;
        }

        public static CadMesh CreateCylinder(CadVertex pos, int slices, double r, double len)
        {
            CadMesh mesh = CreateCylinder(slices, r, len);

            for (int i=0; i<mesh.VertexStore.Count; i++)
            {
                mesh.VertexStore.Ref(i) += pos;
            }

            return mesh;
        }


        public static CadMesh CreateCylinder(int slices, double r, double len)
        {
            CadMesh mesh = new CadMesh(slices * 2 + 2, slices * 3);

            mesh.VertexStore.Add(CadVertex.Create(0, 0, len / 2));
            mesh.VertexStore.Add(CadVertex.Create(0, 0, -len / 2));

            for (int i = 0; i < slices; i++)
            {
                double a1 = i * Math.PI * 2.0 / slices;
                double y = Math.Cos(a1) * r;
                double x = Math.Sin(a1) * r;
                mesh.VertexStore.Add(CadVertex.Create(x, y, len / 2));
                mesh.VertexStore.Add(CadVertex.Create(x, y, -len / 2));
            }

            for (int i = 0; i < slices; i++)
            {
                mesh.FaceStore.Add(new CadFace(0,
                             2 + ((i + 1) % slices) * 2,
                             2 + i * 2));
            }
            for (int i = 0; i < slices; i++)
            {
                mesh.FaceStore.Add(new CadFace(2 + i * 2,
                             2 + ((i + 1) % slices) * 2,
                             3 + ((i + 1) % slices) * 2,
                             3 + i * 2));
            }
            for (int i = 0; i < slices; i++)
            {
                mesh.FaceStore.Add(new CadFace(1,
                             3 + i * 2,
                             3 + ((i + 1) % slices) * 2));
            }

            return mesh;
        }

        public static CadMesh CreateSphere(double r, int slices1, int slices2)
        {
            VertexList vl = new VertexList(slices2);

            double d = Math.PI / slices2;


            for (int i=0; i<slices2; i++)
            {
                double a = i * d;

                double x = Math.Sin(a) * r;
                double y = Math.Cos(a) * r;

                vl.Add(CadVertex.Create(x, y, 0));
            }

            vl.Add(CadVertex.Create(0, -r, 0));


            return CreateRotatingBody(slices1, vl);
        }


        // 回転体の作成
        public static CadMesh CreateRotatingBody(int slices, VertexList vl, FaceType facetype = FaceType.TRIANGLE)
        {
            if (vl.Count < 2)
            {
                return null;
            }

            CadMesh mesh = new CadMesh(vl.Count * slices, vl.Count * slices);

            // 上下端が中心軸にあるなら共有
            int s = 0;
            int e = vl.Count;

            int vc = vl.Count;

            bool topCap = false;
            bool bottomCap = false;

            int ps = 0;

            if (vl[0].x == 0)
            {
                mesh.VertexStore.Add(vl[0]);
                s += 1;
                topCap = true;
                vc--;
                ps++;
            }

            if (vl[vl.Count-1].x == 0)
            {
                mesh.VertexStore.Add(vl[vl.Count - 1]);
                e -= 1;
                bottomCap = true;
                vc--;
                ps++;
            }

            double d = Math.PI * 2.0 / slices;

            for (int i = 0; i < slices; i++)
            {
                double a = i * d;

                for (int vi=s; vi<e; vi++)
                {
                    CadVertex v = vl[vi];
                    CadVertex vv = default(CadVertex);

                    vv.x = v.x * Math.Cos(a);
                    vv.y = v.y;
                    vv.z = v.x * Math.Sin(a);

                    mesh.VertexStore.Add(vv);
                }
            }

            CadFace f;

            if (topCap)
            {
                for (int i = 0; i < slices; i++)
                {
                    f = new CadFace(0, ((i + 1) % slices) * vc + ps, i * vc + ps);
                    mesh.FaceStore.Add(f);
                }
            }

            if (bottomCap)
            {
                for (int i = 0; i < slices; i++)
                {
                    int bi = (vc - 1);

                    f = new CadFace(1, (i * vc) + bi + ps, ((i + 1) % slices) * vc + bi + ps);
                    mesh.FaceStore.Add(f);
                }
            }

            // 四角形で作成
            if (facetype == FaceType.QUADRANGLE)
            {
                for (int i = 0; i < slices; i++)
                {
                    int nextSlice = ((i + 1) % slices) * vc + ps;

                    for (int vi = 0; vi < vc - 1; vi++)
                    {
                        f = new CadFace(
                            (i * vc) + ps + vi,
                            nextSlice + vi,
                            nextSlice + vi + 1,
                            (i * vc) + ps + vi + 1
                            );

                        mesh.FaceStore.Add(f);
                    }
                }
            }
            else
            {
                // 三角形で作成
                for (int i = 0; i < slices; i++)
                {
                    int nextSlice = ((i + 1) % slices) * vc + ps;

                    for (int vi = 0; vi < vc - 1; vi++)
                    {
                        f = new CadFace(
                            (i * vc) + ps + vi,
                            nextSlice + vi,
                            (i * vc) + ps + vi + 1
                            );

                        mesh.FaceStore.Add(f);

                        f = new CadFace(
                           nextSlice + vi,
                           nextSlice + vi + 1,
                           (i * vc) + ps + vi + 1
                           );

                        mesh.FaceStore.Add(f);
                    }
                }

            }

            return mesh;
        }

        public static CadMesh CreateExtruded(VertexList src, CadVertex dv, int div = 0)
        {
            if (src.Count < 3)
            {
                return null;
            }

            div += 1;

            VertexList vl;

            CadVertex n = CadUtil.RepresentativeNormal(src);


            if (CadMath.InnerProduct(n, dv) <= 0)
            {
                vl = new VertexList(src);
                vl.Reverse();
            }
            else
            {
                vl = src;
            }

            int vlCnt = vl.Count;

            CadMesh mesh = new CadMesh(vl.Count * 2, vl.Count);

            CadFace f;

            CadVertex dt = dv / div;

            CadVertex sv = CadVertex.Zero;

            // 頂点リスト作成
            for (int i = 0; i < div + 1; i++)
            {
                for (int j = 0; j < vlCnt; j++)
                {
                    mesh.VertexStore.Add(vl[j] + sv);
                }

                sv += dt;
            }


            // 表面
            f = new CadFace();

            for (int i = 0; i < vlCnt; i++)
            {
                f.VList.Add(i);
            }

            mesh.FaceStore.Add(f);


            // 裏面
            f = new CadFace();

            int si = (div + 1) * vlCnt -1;
            int ei = si - (vlCnt-1);

            for (int i = si; i >= ei; i--)
            {
                f.VList.Add(i);                
            }

            mesh.FaceStore.Add(f);

            // 側面
            for (int k = 0; k < div; k++)
            {
                int ti = vlCnt * k;

                for (int i = 0; i < vlCnt; i++)
                {
                    int j = (i + 1) % vlCnt;

                    f = new CadFace();

                    f.VList.Add(i + ti);
                    f.VList.Add(i + ti + vlCnt);
                    f.VList.Add(j + ti + vlCnt);
                    f.VList.Add(j + ti);

                    mesh.FaceStore.Add(f);
                }
            }

            MeshUtil.SplitAllFace(mesh);

            return mesh;
        }
    }
}
