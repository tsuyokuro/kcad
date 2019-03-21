using MyCollections;

namespace CadDataTypes
{
    public class CadMesh
    {
        public VectorList VertexStore;
        public FlexArray<CadFace> FaceStore;

        public CadMesh()
        {
        }

        public CadMesh(int vertexCount, int faceCount)
        {
            VertexStore = new VectorList(vertexCount);
            FaceStore = new FlexArray<CadFace>(faceCount);
        }
    }
}
