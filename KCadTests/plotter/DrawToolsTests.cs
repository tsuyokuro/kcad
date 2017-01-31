﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Tests
{
    [TestClass()]
    public class DrawToolsTests
    {
        [TestMethod()]
        public void InitDarkSetTest()
        {
            DrawTools dt = new DrawTools();
            dt.Setup(DrawTools.ToolsType.DARK);
            dt.Dispose();
        }

        [TestMethod()]
        public void InitPrinterSetTest()
        {
            DrawTools dt = new DrawTools();
            dt.Setup(DrawTools.ToolsType.PRINTER);
            dt.Dispose();
        }
    }
}