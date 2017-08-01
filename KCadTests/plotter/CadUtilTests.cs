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
            CadVector p1 = default(CadVector);
            CadVector p2 = default(CadVector);
            CadVector p3 = default(CadVector);

            p1.x = 10;
            p1.y = 10;
            p1.z = 0;

            p2.x = 10;
            p2.y = 20;
            p2.z = 0;

            p3.x = 20;
            p3.y = 10;
            p3.z = 0;

            List<CadVector> pl = new List<CadVector>();

            pl.Add(p1);
            pl.Add(p2);
            pl.Add(p3);

            double area = CadUtil.TriangleArea(pl);

            Assert.IsTrue(area == 50.0);

            /*
            Matrix44 m = default(Matrix44);

            m.setXRote(Math.PI / 8.0);

            p1 = Matrix44.product(m, p1);
            p2 = Matrix44.product(m, p2);
            p3 = Matrix44.product(m, p3);

            pl.Clear();
            pl.Add(p1);
            pl.Add(p2);
            pl.Add(p3);

            area = CadUtil.getTriangleArea(pl);

            Assert.IsTrue(area == 50.0);

            DebugOut dout = new DebugOutVS();

            p1.dump(dout);
            p2.dump(dout);
            p3.dump(dout);
            */
        }

        [TestMethod()]
        public void unitVectorTest()
        {
            CadVector p = default(CadVector);

            p.x = 10;
            p.y = 10;
            p.z = 10;

            p = p.UnitVector();

            double n = p.Norm();

            Assert.IsTrue(n > -1 && n <= 1 );
        }
    }
}