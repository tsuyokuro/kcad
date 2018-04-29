using MyCollections;

namespace CadDataTypes
{
    public class VectorList : FlexArray<CadVector>
    {
        public VectorList() : base(8) { }
        public VectorList(int capa) : base(capa) { }
        public VectorList(VectorList src) : base(src) { }
    }

    public class CadFace
    {
        public FlexArray<int> VList;

        public CadFace()
        {
            VList = new FlexArray<int>(3);
        }
    }

    public class CadMesh
    {
        public VectorList VertexStore;
        public FlexArray<CadFace> FaceStore;
    }
}
