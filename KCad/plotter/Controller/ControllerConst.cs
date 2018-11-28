﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller
{
    public enum SelectModes
    {
        POINT,
        OBJECT,
    }

    public enum MeasureModes
    {
        NONE,
        POLY_LINE,
    }

    public enum CursorType
    {
        TRACKING,
        LAST_DOWN,
    }

    class ControllerConst
    {
        public const double MARK_CURSOR_SIZE = 10.0;

        public const double CURSOR_LOCK_MARK_SIZE = 8.0;
    }
}
