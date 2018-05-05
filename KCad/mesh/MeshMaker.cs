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
    }
}
