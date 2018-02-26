﻿using MyCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class VectorList : AutoArray<CadVector>
    {
        public VectorList() : base(8){ }
        public VectorList(int capa) : base(capa) { }
        public VectorList(VectorList src) : base(src) { }
    }
}
