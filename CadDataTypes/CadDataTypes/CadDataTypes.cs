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

        public CadFace(params int[] args)
        {
            VList = new FlexArray<int>(args.Length);
            for (int i=0; i< args.Length; i++)
            {
                VList.Add(args[i]);
            }
        }

        public CadFace(CadFace src)
        {
            VList = new FlexArray<int>(src.VList);
        }
    }

    public class CadMesh
    {
        public VectorList VertexStore;
        public FlexArray<CadFace> FaceStore;

        public CadMesh()
        {
        }

        public CadMesh(int vs, int fs)
        {
            VertexStore = new VectorList(vs);
            FaceStore = new FlexArray<CadFace>(fs);
        }
    }
}
