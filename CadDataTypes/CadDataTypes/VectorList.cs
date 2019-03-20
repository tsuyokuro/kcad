using MyCollections;

namespace CadDataTypes
{
    public class VectorList : FlexArray<CadVector>
    {
        public VectorList() : base(8) { }
        public VectorList(int capa) : base(capa) { }
        public VectorList(VectorList src) : base(src) { }
    }
}
