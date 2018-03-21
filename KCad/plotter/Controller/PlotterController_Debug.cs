#define LOG_INFO
#define LOG_DEBUG

using Newtonsoft.Json.Linq;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace Plotter
{
    public partial class PlotterController
    {
        private CadFigure getSelFig()
        {
            if (mSelList.List.Count == 0)
            {
                return null;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = si.Figure;

            return fig;
        }

        private void test_isPointInTriangle3D()
        {
            TempFigureList.Clear();

            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = si.Figure;

            CadVector pt = LastDownPoint;

            DebugOut dout = new DebugOut();

            bool ret = CadUtil.IsPointInTriangle(pt, fig);

            DebugOut.println("ret=" + ret);
        }

        private void test_crossPoint()
        {
            TempFigureList.Clear();

            MarkSeg seg = mSelectedSegs.LastSel;

            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = mDB.GetFigure(seg.FigureID);

            CadVector a = fig.GetPointAt(seg.PtIndexA);
            CadVector b = fig.GetPointAt(seg.PtIndexB);

            CadVector pt = LastDownPoint;

            CrossInfo ret = CadUtil.PerpendicularCrossSeg2D(a, b, pt);

            if (ret.IsCross)
            {
                CadFigure figx = CadFigure.Create(CadFigure.Types.POLY_LINES);

                figx.AddPoint(pt);
                figx.AddPoint(ret.CrossPoint);

                TempFigureList.Add(figx);
            }

            NotifyDataChanged(true);
        }

        private void test_crossPlane()
        {
            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = mDB.GetFigure(si.FigureID);
            
            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            if (fig.PointCount < 3)
            {
                return;
            }

            CadVector a = LastDownPoint;

            CadVector normal = CadMath.Normal(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

            if (normal.IsZero())
            {
                return;
            }

            CadVector cp = CadUtil.CrossPlane(a, fig.PointList[0], normal);

            CadFigure line = DB.NewFigure(CadFigure.Types.POLY_LINES);

            line.AddPoint(a);
            line.AddPoint(cp);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, line.ID);
            HistoryManager.foward(ope);
            CurrentLayer.AddFigure(line);
        }

        private void test_crossPlane2(DrawContext dc)
        {
            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = mDB.GetFigure(si.FigureID);

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            if (fig.PointCount < 3)
            {
                return;
            }

            CadVector a = LastDownPoint;
            CadVector b = a + CadVector.Create(dc.ViewDir);

            CadVector normal = CadMath.Normal(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

            if (normal.IsZero())
            {
                return;
            }

            CadVector cp = CadUtil.CrossPlane(a, b, fig.PointList[0], normal);

            if (!cp.Valid)
            {
                return;
            }

            CadFigure line = DB.NewFigure(CadFigure.Types.POLY_LINES);

            line.AddPoint(a);
            line.AddPoint(cp);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, line.ID);
            HistoryManager.foward(ope);
            CurrentLayer.AddFigure(line);
        }

        private void test_crossPlane3(DrawContext dc)
        {
            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = mDB.GetFigure(si.FigureID);

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            if (fig.PointCount < 3)
            {
                return;
            }

            CadVector a = dc.CadPointToUnitPoint(LastDownPoint);
            CadVector b = a;
            b.z -= 100;

            a = dc.UnitPointToCadPoint(a);
            b = dc.UnitPointToCadPoint(b);

            CadVector normal = CadMath.Normal(fig.PointList[0], fig.PointList[1], fig.PointList[2]);

            if (normal.IsZero())
            {
                return;
            }

            CadVector cp = CadUtil.CrossPlane(a, b, fig.PointList[0], normal);

            if (!cp.Valid)
            {
                return;
            }

            CadFigure line = DB.NewFigure(CadFigure.Types.POLY_LINES);

            line.AddPoint(LastDownPoint);
            line.AddPoint(cp);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, line.ID);
            HistoryManager.foward(ope);
            CurrentLayer.AddFigure(line);
        }

        private void test_depthLine(DrawContext dc)
        {
            CadVector a = dc.CadPointToUnitPoint(LastDownPoint);
            CadVector b = a;
            b.z -= 100;

            a = dc.UnitPointToCadPoint(a);
            b = dc.UnitPointToCadPoint(b);

            CadFigure line = DB.NewFigure(CadFigure.Types.POLY_LINES);

            line.AddPoint(a);
            line.AddPoint(b);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, line.ID);
            HistoryManager.foward(ope);
            CurrentLayer.AddFigure(line);

            NotifyDataChanged(true);
        }

        private void test_areaCollector()
        {
            TempFigureList.Clear();

            AreaCollecter ac = new AreaCollecter(mDB);
            List<CadFigure> figs = ac.Collect(mSelList.List);

            figs.ForEach(a => { TempFigureList.Add(a); });

            NotifyDataChanged(true);
        }

        private void test_quaternion()
        {
            TempFigureList.Clear();

            CadFigure fig = getSelFig();
            if (fig == null) return;

            CadVector axis = default(CadVector);
            //axis.y = -1.0;
            axis.x = -1.0;
            axis.z = -1.0;

            axis = axis.UnitVector();

            double rad = CadMath.Deg2Rad(10);

            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, rad);
            CadQuaternion r = q.Conjugate();

            CadFigure tfig = CadFigure.Create(CadFigure.Types.POLY_LINES);

            CadVector tp = default(CadVector);

            int i = 0;

            for (;i<fig.PointList.Count;i++)
            {
                CadVector p = fig.PointList[i];
                CadQuaternion qp = CadQuaternion.FromPoint(p);

                qp = r * qp;
                qp = qp * q;

                tp.x = qp.x;
                tp.y = qp.y;
                tp.z = qp.z;

                tfig.AddPoint(tp);
            }

            fig.ClearPoints();
            fig.AddPoints(tfig.PointList);

            //TempFigureList.Add(tfig);

            NotifyDataChanged(true);
        }

        private void test_matrix()
        {
        }

        private void test_matrix2()
        {
            MatrixMN m1 = new MatrixMN(new double[,]
            {
                { 11, 12, 13 },
                { 21, 22, 23 },
                { 31, 32, 33 }
            });

            MatrixMN m2 = new MatrixMN(new double[,]
            {
                { 11, 12, 13 },
                { 21, 22, 23 },
                { 31, 32, 33 }
            });

            MatrixMN m3 = m1.Product(m2);

            m1.dump();
            m2.dump();
            m3.dump();
        }

        private void test()
        {
            UMatrix4 m = new UMatrix4(
                1, 0, 0, 0,
                10, 0, 0, 0,
                100, 0, 0, 0,
                1000, 0, 0, 0
                );

            Vector4d v = new Vector4d(1, 2, 3, 4);

            Vector4d vd = Vector4d.Transform(v, m.GLMatrix);
        }

        private void testGLInvert(DrawContext dc)
        {
            CadVector p0 = CadVector.Create(10, 10, 0);

            p0 += dc.ViewOrg;

            CadVector pc = dc.UnitPointToCadPoint(p0);

            CadVector ps = dc.CadPointToUnitPoint(pc);

            ps -= dc.ViewOrg;

            //DebugOut.Std.println("View");
            //ps.dump(DebugOut.Std);
        }

        private void test_ClearLayer()
        {
            ClearLayer(CurrentLayer.ID);
            NotifyDataChanged(true);
        }

        private void mtest()
        {
            DrawContextGDI tdc = new DrawContextGDI();

            tdc.SetViewSize(120, 60);
            tdc.ViewOrg = CadVector.Create(60, 30, 0);

            tdc.SetCamera(Vector3d.UnitZ, Vector3d.Zero, Vector3d.UnitY);

            CadVector p = CadVector.Create(0, 0, 0);

            CadVector tp = default(CadVector);

            tp = tdc.CadPointToUnitPoint(p);

            CadUtil.Dump((Vector4d)p, "p");
            CadUtil.Dump((Vector4d)p, "tp");
        }

        private void LoadDxfTest()
        {
            CadDxfLoader loader = new CadDxfLoader();

            loader.AsyncLoad(@"f:\work2\cblock.dxf", 1000, (state, percent, db)=>
            {
                mDB = db;

                mHistoryManager = new HistoryManager(mDB);

                NotifyDataChanged(true);

                NotifyLayerInfo();

                InteractOut.println("Complete! point=" + loader.TotalPointCount +
                    " Face=" + loader.TotalFaceCount);
            });

            InteractOut.println("Loading ...");
        }

        public void debugCommand(DrawContext dc, string s)
        {
            if (s == "test")
            {
                test();
            }

            else if (s == "dxf")
            {
                LoadDxfTest();
            }

            else if (s == "mtest")
            {
                mtest();
            }

            else if (s == "clear layer")
            {
                test_ClearLayer();
            }

            else if (s == "test gi")
            {
                testGLInvert(dc);
            }

            else if (s == "clean temp")
            {
                TempFigureList.Clear();
            }

            else if (s == "depthLine")
            {
                test_depthLine(dc);
            }

            else if (s == "test q")
            {
                test_quaternion();
            }

            else if (s == "test matrix")
            {
                test_matrix();
            }

            else if (s == "test matrix2")
            {
                test_matrix2();
            }

            else if (s == "test cross")
            {
                test_crossPoint();
            }

            else if (s == "test areaCollector")
            {
                test_areaCollector();
            }

            else if (s == "test isPointInTriangle")
            {
                test_isPointInTriangle3D();
            }

            else if (s == "crossPlane")
            {
                test_crossPlane();
            }

            else if (s == "crossPlane2")
            {
                test_crossPlane2(dc);
            }

            else if (s == "crossPlane3")
            {
                test_crossPlane3(dc);
            }

            else if (s == "dump sels")
            {
                mSelList.dump();
            }

            else if (s == "dump selsegs")
            {
                mSelList.dump();
            }

            else if (s == "dump layer")
            {
                CurrentLayer.dump();
            }

            else if (s == "sdump layer")
            {
                CurrentLayer.sdump();
            }

            else if (s == "dump")
            {
                //DebugOut dst = new DebugOut();
                dump();
            }

            else if (s == "dump figs")
            {
                mDB.dumpFigureMap();
            }

            else if (s == "dump temp")
            {
                DebugOut.println("dump temp {");
                DebugOut.Indent++;
                foreach (CadFigure fig in TempFigureList)
                {
                    fig.Dump("temp fig");
                }
                DebugOut.Indent--;
                DebugOut.println("}");
            }

            else if (s == "dump snap")
            {
                DebugOut dout = new DebugOut();
                DebugOut.println("Snap range {");
                DebugOut.Indent++;
                DebugOut.println("Point:" + PointSnapRange.ToString());
                DebugOut.println("Segment:" + LineSnapRange.ToString());
                DebugOut.println("Line:" + LineSnapRange.ToString());
                DebugOut.Indent--;
                DebugOut.println("}");
            }
        }
        public void dump()
        {
            mDB.dump();
        }
    }
}
