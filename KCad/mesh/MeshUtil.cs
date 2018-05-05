﻿using CadDataTypes;
using MyCollections;
using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshUtilNS
{
    public class MeshUtil
    {
        // 全てのFaceを3角形に分割する
        public static void SplitAllFace(CadMesh mesh)
        {
            FlexArray<CadFace> faceStore = new FlexArray<CadFace>();

            for (int i=0; i<mesh.FaceStore.Count; i++)
            {
                CadFace face = mesh.FaceStore[i];
                if (face.VList.Count < 3)
                {
                    continue;
                }

                if (face.VList.Count == 3)
                {
                    faceStore.Add(face);
                    continue;
                }

                List<CadFace> flist = Split(face, mesh);

                faceStore.AddRange(flist);
            }

            mesh.FaceStore = faceStore;
        }

        public static List<CadFace> Split(CadFace face, CadMesh mesh)
        {
            CadVector p0 = default(CadVector);

            // Deep copy
            CadFace src = new CadFace(face);

            var triangles = new List<CadFace>();

            int i1 = -1;

            int state = 0;

            CadFace triangle;

            i1 = FindMaxDistantPointIndex(p0, src, mesh);

            if (i1 == -1)
            {
                return triangles;
            }

            triangle = GetTriangleWithCenterPoint(src, i1);

            CadVector tp0 = mesh.VertexStore[ triangle.VList[0] ];
            CadVector tp1 = mesh.VertexStore[ triangle.VList[1] ];
            CadVector tp2 = mesh.VertexStore[ triangle.VList[2] ];

            CadVector dir = CadMath.Normal(tp1, tp0, tp2);
            CadVector currentDir = CadVector.Zero;

            while (src.VList.Count > 3)
            {
                if (state == 0)
                {
                    i1 = FindMaxDistantPointIndex(p0, src, mesh);
                    if (i1 == -1)
                    {
                        return triangles;
                    }
                }

                triangle = GetTriangleWithCenterPoint(src, i1);

                tp0 = mesh.VertexStore[ triangle.VList[0] ];
                tp1 = mesh.VertexStore[ triangle.VList[1] ];
                tp2 = mesh.VertexStore[ triangle.VList[2] ];

                currentDir = CadMath.Normal(tp1, tp0, tp2);

                bool hasIn = ListContainsPointInTriangle(triangle, src, mesh);

                double scala = CadMath.InnerProduct(dir, currentDir);

                if (!hasIn && (scala > 0))
                {
                    triangles.Add(triangle);
                    src.VList.RemoveAt(i1);
                    state = 0;
                    continue;
                }

                if (state == 0)
                {
                    state = 1;
                    i1 = 0;
                }
                else if (state == 1)
                {
                    i1++;
                    if (i1 >= src.VList.Count)
                    {
                        break;
                    }
                }
            }

            if (src.VList.Count == 3)
            {
                triangle = new CadFace();
                triangle.VList.Add(src.VList[0]);
                triangle.VList.Add(src.VList[1]);
                triangle.VList.Add(src.VList[2]);

                triangles.Add(triangle);
            }

            return triangles;
        }

        private static bool ListContainsPointInTriangle(CadFace triangle, CadFace face, CadMesh mesh)
        {
            FlexArray<int> tps = triangle.VList;

            for (int i=0; i< face.VList.Count; i++)
            {
                CadVector fv = mesh.VertexStore[ face.VList[i] ];

                if (
                    fv.Equals(mesh.VertexStore[ tps[0] ]) ||
                    fv.Equals(mesh.VertexStore[ tps[1] ]) ||
                    fv.Equals(mesh.VertexStore[ tps[2] ])
                    )
                {
                    continue;
                }

                bool ret = IsPointInTriangle(fv, triangle, mesh);
                if (ret)
                {
                    return true;
                }
            }

            return false;
        }

        public static int FindMaxDistantPointIndex(CadVector p0, CadFace f, CadMesh mesh)
        {
            int ret = -1;
            int i;

            CadVector t;

            double maxd = 0;

            for (i = 0; i < f.VList.Count; i++)
            {
                CadVector fp = mesh.VertexStore[ f.VList[i] ];

                t = fp - p0;
                double d = t.Norm();

                if (d > maxd)
                {
                    maxd = d;
                    ret = i;
                }
            }

            return ret;
        }

        private static CadFace GetTriangleWithCenterPoint(CadFace face, int cpIndex)
        {
            int i1 = cpIndex;
            int endi = face.VList.Count - 1;

            int i0 = i1 - 1;
            int i2 = i1 + 1;

            if (i0 < 0) { i0 = endi; }
            if (i2 > endi) { i2 = 0; }

            var triangle = new CadFace();

            triangle.VList.Add(face.VList[i0]);
            triangle.VList.Add(face.VList[i1]);
            triangle.VList.Add(face.VList[i2]);

            return triangle;
        }

        public static bool IsPointInTriangle(CadVector p, CadFace triangle, CadMesh mesh)
        {
            if (triangle.VList.Count < 3)
            {
                return false;
            }

            CadVector p0 = mesh.VertexStore[ triangle.VList[0] ];
            CadVector p1 = mesh.VertexStore[ triangle.VList[1] ];
            CadVector p2 = mesh.VertexStore[ triangle.VList[2] ];

            CadVector c1 = CadMath.CrossProduct(p, p0, p1);
            CadVector c2 = CadMath.CrossProduct(p, p1, p2);
            CadVector c3 = CadMath.CrossProduct(p, p2, p0);

            double ip12 = CadMath.InnerProduct(c1, c2);
            double ip13 = CadMath.InnerProduct(c1, c3);


            // When all corossProduct result's sign are same, Point is in triangle
            if (ip12 > 0 && ip13 > 0)
            {
                return true;
            }

            return false;
        }
    }
}
