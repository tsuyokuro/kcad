using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plotter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Tests
{
    [TestClass()]
    public class CadUtilTests
    {
        [TestMethod()]
        public void getTriangleAreaTest()
        {
            CadPoint p1 = default(CadPoint);
            CadPoint p2 = default(CadPoint);
            CadPoint p3 = default(CadPoint);

            p1.x = 10;
            p1.y = 10;
            p1.z = 0;

            p2.x = 10;
            p2.y = 20;
            p2.z = 0;

            p3.x = 20;
            p3.y = 10;
            p3.z = 0;

            List<CadPoint> pl = new List<CadPoint>();

            pl.Add(p1);
            pl.Add(p2);
            pl.Add(p3);

            double area = CadUtil.getTriangleArea(pl);

            Assert.IsTrue(area == 50.0);
        }
    }
}