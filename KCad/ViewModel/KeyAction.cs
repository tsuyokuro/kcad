//#define USE_GDI_VIEW
using System;

namespace KCad.ViewModel
{

    public partial class PlotterViewModel
    {
        public class KeyAction
        {
            public Action Down;
            public Action Up;
            public string Description;

            public KeyAction(Action down, Action up, string description=null)
            {
                Down = down;
                Up = up;
                Description = description;
            }
        }
    }
}
