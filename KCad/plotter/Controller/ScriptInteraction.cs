using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller
{
    public class ScriptInteraction
    {
        public bool WaitDown = false;
        public event Action<CadVector> Down;

        public bool WaitLine = false;
        public event Action<CadVector, CadVector> Line;
    }
}
