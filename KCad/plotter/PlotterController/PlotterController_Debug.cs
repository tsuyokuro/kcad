﻿#define LOG_INFO
#define LOG_DEBUG

using Newtonsoft.Json.Linq;
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

        private void test_isPointInTriangle3D(DrawContext dc)
        {
            TempFigureList.Clear();

            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = si.Figure;

            CadPoint pt = FreeDownPoint;

            DebugOut dout = new DebugOut();

            bool ret = CadUtil.isPointInTriangle3D(pt, fig.PointList);

            dout.println("ret=" + ret);
        }

        private void test_getPoints(DrawContext dc)
        {
            TempFigureList.Clear();

            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = si.Figure;

            IReadOnlyList<CadPoint> list = fig.GetPoints(64);

            CadFigure tfig = new CadFigure(CadFigure.Types.POLY_LINES);

            tfig.AddPoints(list);

            TempFigureList.Add(tfig);

            List<CadFigure> tl = TriangleSplitter.split(tfig);

            tl.ForEach(a => TempFigureList.Add(a));

            Clear(dc);
            Draw(dc);
        }

        private void test_crossPoint(DrawContext dc)
        {
            TempFigureList.Clear();

            MarkSeg seg = mSelectedSegs.LastSel;

            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = mDB.getFigure(seg.FigureID);

            CadPoint a = fig.GetPointAt(seg.PtIndexA);
            CadPoint b = fig.GetPointAt(seg.PtIndexB);

            CadPoint pt = FreeDownPoint;

            CrossInfo ret = CadUtil.getPerpCrossSeg2D(a, b, pt);

            if (ret.IsCross)
            {
                CadFigure figx = new CadFigure(CadFigure.Types.POLY_LINES);

                figx.AddPoint(pt);
                figx.AddPoint(ret.CrossPoint);

                TempFigureList.Add(figx);
            }

            Clear(dc);
            Draw(dc);
        }

        private void test_areaCollector()
        {
            TempFigureList.Clear();

            AreaCollecter ac = new AreaCollecter(mDB);
            List<CadFigure> figs = ac.collect(mSelList.List);

            figs.ForEach(a => { TempFigureList.Add(a); });

            RequestRedraw();
        }

        private void test_getCentroid(DrawContext dc)
        {
            TempFigureList.Clear();

            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = si.Figure;

            Centroid cent = fig.GetCentroid();

            if (cent.SplitList != null)
            {
                cent.SplitList.ForEach(a => TempFigureList.Add(a));
            }

            CadFigure centfig = new CadFigure(CadFigure.Types.POLY_LINES);

            centfig.AddPoint(cent.Point);

            TempFigureList.Add(centfig);

            Clear(dc);
            Draw(dc);
        }

        private void test_quaternion(DrawContext dc)
        {
            TempFigureList.Clear();

            CadFigure fig = getSelFig();
            if (fig == null) return;

            CadPoint axis = default(CadPoint);
            //axis.y = -1.0;
            axis.x = -1.0;
            axis.z = -1.0;

            axis = axis.UnitVector();

            double rad = CadMath.deg2rad(10);

            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, rad);
            CadQuaternion r = q.Conjugate();

            CadFigure tfig = new CadFigure(CadFigure.Types.POLY_LINES);

            CadPoint tp = default(CadPoint);

            int i = 0;

            for (;i<fig.PointList.Count;i++)
            {
                CadPoint p = fig.PointList[i];
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

            Clear(dc);
            Draw(dc);
        }

        private void test_matrix()
        {
        }

        private void test_matrix2()
        {
            DebugOut o = new DebugOut();

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

            MatrixMN m3 = m1.product(m2);

            m1.dump(o);
            m2.dump(o);
            m3.dump(o);
        }

        private void test_dc(DrawContext dc)
        {
            DrawContext tdc = new DrawContext();

            tdc.SetViewMatrix(UMatrixs.ViewXY);

            CadPoint p = CadPoint.Create(0, 0, -100);

            CadPoint vp = tdc.CadPointToUnitPoint(p);

            vp.dump(DebugOut.Std);
        }

        private void dump_figv(DrawContext dc)
        {
            CadFigure fig = getSelFig();
            if (fig == null) return;

            fig.DumpV(DebugOut.Std, dc);
        }

        private void test(DrawContext dc)
        {
            CadPoint p = CadPoint.Create(12, 34, 0);

            CadPoint pp = dc.UnitPointToCadPoint(p);

            CadPoint p0 = dc.CadPointToUnitPoint(pp);

            dc.ViewOrg.dump(DebugOut.Std);
            p.dump(DebugOut.Std);
            pp.dump(DebugOut.Std);
            p0.dump(DebugOut.Std);
        }

        private void testGLInvert()
        {
            CadPoint p0 = CadPoint.Create(10, 10, 0);

            p0 += CurrentDC.ViewOrg;

            CadPoint pc = CurrentDC.UnitPointToCadPoint(p0);

            CadPoint ps = CurrentDC.CadPointToUnitPoint(pc);

            ps -= CurrentDC.ViewOrg;

            //DebugOut.Std.println("View");
            //ps.dump(DebugOut.Std);
        }

        private void test_ClearLayer(DrawContext dc)
        {
            ClearLayer(dc, CurrentLayer.ID);
        }

        public void debugCommand(DrawContext dc, string s)
        {
            if (s == "test")
            {
                test(dc);
            }

            else if (s == "clear layer")
            {
                test_ClearLayer(dc);
            }

            else if (s == "test gi")
            {
                testGLInvert();
            }

            else if (s == "clean temp")
            {
                TempFigureList.Clear();
            }

            else if (s == "test q")
            {
                test_quaternion(dc);
            }

            else if (s == "test dc")
            {
                test_dc(dc);
            }

            else if (s == "test matrix")
            {
                test_matrix();
            }

            else if (s == "test matrix2")
            {
                test_matrix2();
            }

            else if (s == "test centroid")
            {
                test_getCentroid(dc);
            }

            else if (s == "test getPoints")
            {
                test_getPoints(dc);
            }

            else if (s == "test cross")
            {
                test_crossPoint(dc);
            }

            else if (s == "test areaCollector")
            {
                test_areaCollector();
            }

            else if (s == "test isPointInTriangle")
            {
                test_isPointInTriangle3D(dc);
            }

            else if (s == "dump figv")
            {
                dump_figv(dc);
            }

            else if (s == "dump sels")
            {
                DebugOut dout = new DebugOut();
                mSelList.dump(dout);
            }

            else if (s == "dump selsegs")
            {
                DebugOut dout = new DebugOut();
                mSelList.dump(dout);
            }

            else if (s == "dump layer")
            {
                DebugOut dout = new DebugOut();
                CurrentLayer.dump(dout);
            }

            else if (s == "sdump layer")
            {
                DebugOut dout = new DebugOut();
                CurrentLayer.sdump(dout);
            }

            else if (s == "dump")
            {
                DebugOut dst = new DebugOut();
                dump(dst);
            }

            else if (s == "dump figs")
            {
                DebugOut dout = new DebugOut();
                mDB.dumpFigureMap(dout);
            }

            else if (s == "dump temp")
            {
                DebugOut dout = new DebugOut();

                dout.println("dump temp {");
                dout.Indent++;
                foreach (CadFigure fig in TempFigureList)
                {
                    fig.Dump(dout);
                }
                dout.Indent--;
                dout.println("}");
            }

            else if (s == "Save")
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("f:\\work\\test.kcd", FileMode.Create, FileAccess.Write, FileShare.None);

                formatter.Serialize(stream, mDB);

                stream.Close();
            }
            else if (s == "load")
            {
                IFormatter formatter = new BinaryFormatter();

                Stream stream = new FileStream("f:\\work\\test.kcd", FileMode.Open, FileAccess.Read, FileShare.Read);

                CadObjectDB db = (CadObjectDB)formatter.Deserialize(stream);

                stream.Close();

                DebugOut dout = new DebugOut();
                db.dump(dout);

                mDB = db;
            }
            else if (s == "savej")
            {
                StreamWriter writer = new StreamWriter("f:\\work\\test.txt");

                Stopwatch sw = new Stopwatch();

                sw.Start();
                JObject jo = mDB.ToJson();
                sw.Stop();

                DebugOut dout = new DebugOut();
                dout.println("ToJson time:" + sw.Elapsed);

                writer.Write(jo.ToString());

                writer.Close();
            }
            else if (s == "loadj")
            {
                StreamReader reader = new StreamReader("f:\\work\\test.txt");

                String js = reader.ReadToEnd();

                reader.Close();

                JObject jo = JObject.Parse(js);


                CadObjectDB db = new CadObjectDB();


                Stopwatch sw = new Stopwatch();
                sw.Start();
                db.FromJson(jo);
                sw.Stop();

                DebugOut dout = new DebugOut();
                dout.println("FromJson time:" + sw.Elapsed);


                db.dump(dout);

                mDB = db;

                Clear(dc);
                Draw(dc);
            }
            else if (s == "copy")
            {
                CopyFigures();
            }
            else if (s == "paste")
            {
                PasteFigure();
            }
        }
        public void dump(DebugOut dst)
        {
            DebugOut dout = new DebugOut();
            mDB.dump(dout);
        }
    }
}
