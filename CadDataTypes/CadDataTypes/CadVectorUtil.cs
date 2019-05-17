namespace CadDataTypes
{
    // 短い配列をstackに確保するためのstruct
    public struct CadVectorArray4
    {
        public CadVertex v0;
        public CadVertex v1;
        public CadVertex v2;
        public CadVertex v3;

        public int Length;

        public CadVertex this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return v0;
                    case 1: return v1;
                    case 2: return v2;
                    case 3: return v3;
                }

                return CadVertex.Zero;
            }

            set
            {
                switch (i)
                {
                    case 0: v0 = value; break;
                    case 1: v1 = value; break;
                    case 2: v2 = value; break;
                    case 3: v3 = value; break;
                }
            }
        }
    }
}