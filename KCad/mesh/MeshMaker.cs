using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeshMakerNS
{
    public class MeshMaker
    {
        public static CadMesh CreateBox(CadVector pos, CadVector sv)
        {
            CadMesh cm = CreateCube();

            sv /= 2;

            for (int i=0;i<cm.VertexStore.Count; i++)
            {
                cm.VertexStore.Ref(i) *= sv;
                cm.VertexStore.Ref(i) += pos;
            }

            return cm;
        }


        public static CadMesh CreateCube()
        {
            CadMesh cm = new CadMesh(8, 12);

            cm.VertexStore.Add(CadVector.Create(+1.0, +1.0, +1.0));
            cm.VertexStore.Add(CadVector.Create(-1.0, +1.0, +1.0));
            cm.VertexStore.Add(CadVector.Create(-1.0, -1.0, +1.0));
            cm.VertexStore.Add(CadVector.Create(+1.0, -1.0, +1.0));

            cm.VertexStore.Add(CadVector.Create(+1.0, +1.0, -1.0));
            cm.VertexStore.Add(CadVector.Create(-1.0, +1.0, -1.0));
            cm.VertexStore.Add(CadVector.Create(-1.0, -1.0, -1.0));
            cm.VertexStore.Add(CadVector.Create(+1.0, -1.0, -1.0));

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

            return cm;
        }

        public static CadMesh CreateCylinder(CadVector pos, int slices, double r, double len)
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

            mesh.VertexStore.Add(CadVector.Create(0, 0, len / 2));
            mesh.VertexStore.Add(CadVector.Create(0, 0, -len / 2));

            for (int i = 0; i < slices; i++)
            {
                double a1 = i * Math.PI * 2.0 / slices;
                double y = Math.Cos(a1) * r;
                double x = Math.Sin(a1) * r;
                mesh.VertexStore.Add(CadVector.Create(x, y, len / 2));
                mesh.VertexStore.Add(CadVector.Create(x, y, -len / 2));
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
    }
}
