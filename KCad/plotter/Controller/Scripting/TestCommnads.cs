﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CadDataTypes;
using LibiglWrapper;
using HalfEdgeNS;
using CarveWapper;
using MeshMakerNS;
using SplineCurve;
using KCad;
using Plotter.Serializer;
using MessagePack;
using Newtonsoft.Json.Linq;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK;
using Plotter.svg;
using System.Xml.Linq;

namespace Plotter.Controller
{
    public class TestCommands
    {
        PlotterController Controller;

        public TestCommands(PlotterController controller)
        {
            Controller = controller;
        }

        private void test001()
        {
            VertexList vl = new VertexList();

            vl.Add(CadVertex.Create(0, 20, 0));
            vl.Add(CadVertex.Create(15, 15, 0));
            vl.Add(CadVertex.Create(18, 0, 0));
            vl.Add(CadVertex.Create(15, -15, 0));
            vl.Add(CadVertex.Create(10, -20, 0));

            CadMesh cm = MeshMaker.CreateRotatingBody(16, vl);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryMan.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
            Controller.UpdateTreeView(true);
        }

        private void test002()
        {
            CadMesh cm = MeshMaker.CreateSphere(new Vector3d(0,0,0), 20, 16, 16);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryMan.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
            Controller.UpdateTreeView(true);
        }

        private void test003()
        {
            CadFigure tfig = GetTargetFigure();

            if (tfig == null || tfig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }


            CadMesh cm = MeshMaker.CreateExtruded(tfig.GetPoints(16), Vector3d.UnitZ * -20);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryMan.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
            Controller.UpdateTreeView(true);
        }

        private void test004()
        {
            ItConsole.println("\x1b[33mTest1\x1b[00mテスト\x1b[36mTest3");
        }

        private void test005()
        {
            CadFigure fig = GetTargetFigure();

            if (fig.PointCount < 3)
            {
                return;
            }

            Vector3d v1 = fig.PointList[0].vector - fig.PointList[1].vector;
            Vector3d v2 = fig.PointList[2].vector - fig.PointList[1].vector;

            double t = CadMath.AngleOfVector(v1, v2);

            double a = CadMath.Rad2Deg(t);

            ItConsole.println(string.Format("angle:{0}(deg)", a));
        }

        private void test006()
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            List<uint> idlist = Controller.DB.GetSelectedFigIDList();

            int i;
            for (i = 0; i < idlist.Count; i++)
            {
                CadFigure fig = db.GetFigure(idlist[i]);

                if (fig == null)
                {
                    continue;
                }

                int j;
                for (j = 0; j < fig.PointList.Count; j++)
                {
                    if (!fig.PointList[j].Selected)
                    {
                        continue;
                    }

                    CadVertex p = fig.PointList[j];

                    p.Z = 0;

                    fig.PointList[j] = p;
                }
            }
        }

        private void test008()
        {
            var dc = Controller.CurrentDC;

            CadFigure fig = GetTargetFigure();


            MinMax2D mm = FigureMinMaxScrn(dc, fig);
        }

        public static MinMax2D FigureMinMaxScrn(DrawContext dc, CadFigure fig)
        {
            MinMax2D mm = MinMax2D.Create();
            CadVertex p0;
            CadVertex p1;

            fig.ForEachSegment(seg =>
            {
                p0 = dc.WorldPointToDevPoint(seg.P0);
                p1 = dc.WorldPointToDevPoint(seg.P1);

                if (p0.X < mm.Min.X)
                {
                    mm.Min.X = p0.X;
                }

                if (p1.X < mm.Min.X)
                {
                    mm.Min.X = p1.X;
                }

                if (p0.X > mm.Max.X)
                {
                    mm.Max.X = p0.X;
                }

                if (p1.X > mm.Max.X)
                {
                    mm.Max.X = p1.X;
                }


                if (p0.Y < mm.Min.Y)
                {
                    mm.Min.Y = p0.Y;
                }

                if (p1.Y < mm.Min.Y)
                {
                    mm.Min.Y = p1.Y;
                }

                if (p0.Y > mm.Max.Y)
                {
                    mm.Max.Y = p0.Y;
                }

                if (p1.Y > mm.Max.Y)
                {
                    mm.Max.Y = p1.Y;
                }

                return true;
            });

            return mm;
        }

        private void test009()
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            int i = 0;
            int layerCnt = db.LayerList.Count;

            for (; i < layerCnt; i++)
            {
                CadLayer layer = db.LayerList[i];

                int j = 0;
                int figCnt = layer.FigureList.Count;

                for (; j < figCnt; j++)
                {
                    CadFigure fig = layer.FigureList[j];

                    int k = 0;
                    int pcnt = fig.PointList.Count;

                    for (; k < pcnt; k++)
                    {
                        CadVertex p = fig.PointList[k];
                        CadVertex sp = dc.WorldPointToDevPoint(p);
                    }
                }
            }

            sw.Stop();
            DOut.pl(sw.ElapsedMilliseconds.ToString() + " milli sec");
        }

        private void test010()
        {
            MinMax2D mm = MinMax2D.Create();
        }

        private void formatTest(string format, params object[] args)
        {
            string s = String.Format(format, args);
        }

        private void test011()
        {
            //formatTest("{0},{1}", 10, 20);

            ItConsole.printf("{0},{1}\n", 10, 20);
            ItConsole.print("test");
            ItConsole.println("_test");
            ItConsole.println("abc\ndef");
            ItConsole.println("end");
        }

        private void test012()
        {
            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            List<CadFigure> triangles = TriangleSplitter.Split(fig);

            Controller.TempFigureList.AddRange(triangles);
        }

        private void testMesh()
        {
            var figlist = Controller.DB.GetSelectedFigList();

            CadOpeList opeRoot = new CadOpeList();

            CadOpe ope;

            for (int i = 0; i < figlist.Count; i++)
            {
                CadFigure fig = figlist[i];

                if (fig == null)
                {
                    continue;
                }

                if (fig.Type != CadFigure.Types.POLY_LINES)
                {
                    continue;
                }

                CadFigureMesh mesh = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

                mesh.CreateModel(fig);


                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, mesh.ID);
                opeRoot.Add(ope);

                Controller.CurrentLayer.AddFigure(mesh);


                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);
                opeRoot.Add(ope);

                Controller.CurrentLayer.RemoveFigureByID(fig.ID);
            }

            if (opeRoot.OpeList.Count > 0)
            {
                Controller.HistoryMan.foward(opeRoot);
            }

            Controller.UpdateTreeView(true);
        }

        private void test013()
        {
            ItConsole.println("test013 start");
            test013sub();
            ItConsole.println("test013 end");
        }

        private async void test013sub()
        {
            ItConsole.println("test013Sub start");

            await Task.Run(()=>
            {
                ItConsole.println("Run");
                Thread.Sleep(2000);
                ItConsole.println("Run end");
            });

            ItConsole.println("test013Sub end");
        }

        private void testLoadOff()
        {
            string fname = @"F:\TestFiles\bunny.off";

            CadMesh cm = IglW.ReadOFF(fname);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            for (int i=0; i< hem.VertexStore.Count; i++)
            {
                hem.VertexStore[i] *= 500.0;
            }

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);
        }

        private void testAminusB()
        {
            List<CadFigure> figList = Controller.DB.GetSelectedFigList();

            if (figList.Count < 2)
            {
                return;
            }

            if (figList[0].Type != CadFigure.Types.MESH)
            {
                return;
            }

            if (figList[1].Type != CadFigure.Types.MESH)
            {
                return;
            }

            CadFigureMesh fig_a = (CadFigureMesh)figList[0];
            CadFigureMesh fig_b = (CadFigureMesh)figList[1];

            if (fig_a.Current)
            {
                CadFigureMesh t = fig_a;
                fig_a = fig_b;
                fig_b = t;
            }

            ItConsole.println("ID:" + fig_a.ID.ToString() + " - ID:" + fig_b.ID.ToString());

            HeModel he_a = fig_a.mHeModel;
            HeModel he_b = fig_b.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.AMinusB(a, b);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);
        }

        private void testInvert()
        {
            CadFigure fig = GetTargetFigure();

            fig.InvertDir();
        }


        private void testLoadDxf()
        {
            CadDxfLoader loader = new CadDxfLoader();

            CadMesh cm = loader.Load(@"F:\work\恐竜.DXF", 20.0);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);

            RunOnMainThread(() =>
            {
                Controller.UpdateTreeView(true);
            });

            Redraw();
        }

        private void testRB()
        {
            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            CadMesh cm = MeshMaker.CreateRotatingBody(32, fig.PointList, MeshMaker.FaceType.QUADRANGLE);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh meshFig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            meshFig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(meshFig);

            Controller.UpdateTreeView(true);
        }

        private void testNu()
        {
            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            CadFigureNurbsLine nfig = (CadFigureNurbsLine)Controller.DB.NewFigure(CadFigure.Types.NURBS_LINE);

            nfig.AddPoints(fig.PointList);

            nfig.Setup(3, 32, false, true);

            Controller.CurrentLayer.AddFigure(nfig);

            Controller.UpdateTreeView(true);
        }

        private void testNus()
        {
            CadFigureNurbsSurface nfig = (CadFigureNurbsSurface)Controller.DB.NewFigure(CadFigure.Types.NURBS_SURFACE);

            int ucnt = 8;
            int vcnt = 5;

            VertexList vl =SplineUtil.CreateFlatControlPoints(ucnt, vcnt, Vector3d.UnitX * 20.0, Vector3d.UnitZ * 20.0);

            nfig.Setup(2, ucnt, vcnt, vl, null, 16, 16);


            Controller.CurrentLayer.AddFigure(nfig);

            RunOnMainThread(() =>
            {
                Controller.UpdateTreeView(true);
            });
        }

        private void testNus2()
        {
            CadFigureNurbsSurface nfig = (CadFigureNurbsSurface)Controller.DB.NewFigure(CadFigure.Types.NURBS_SURFACE);

            int ucnt = 4;
            int vcnt = 4;

            VertexList vl = SplineUtil.CreateBoxControlPoints(
                ucnt, vcnt, Vector3d.UnitX * 20.0, Vector3d.UnitZ * 20.0, Vector3d.UnitY * -20.0 );

            nfig.Setup(2, ucnt*2, vcnt, vl, null, 16, 16, false, false, true, true);

            Controller.CurrentLayer.AddFigure(nfig);

            RunOnMainThread(() =>
            {
                Controller.UpdateTreeView(true);
            });
        }

        public CadFigure GetTargetFigure()
        {
            List<uint> idlist = Controller.DB.GetSelectedFigIDList();

            if (idlist.Count == 0)
            {
                return null;
            }

            return Controller.DB.GetFigure(idlist[0]);
        }

        private void Test()
        {
        }

        private void testTriangulate()
        {
            CadFigure tfig = GetTargetFigure();
            if (tfig == null || tfig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            if (tfig.PointCount < 3)
            {
                return;
            }

            VertexList vl = tfig.GetPoints(12);

            CadMesh m = IglW.Triangulate(vl, "a1000q");

            HeModel hem = HeModelConverter.ToHeModel(m);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);
        }

        private void testSvg()
        {
            List<CadFigure> figList = Controller.DB.GetSelectedFigList();

            SvgExporter svgExporter = new SvgExporter();

            XDocument doc = svgExporter.ToSvg(figList,
                        Controller.CurrentDC,
                        Controller.PageSize.Width,
                        Controller.PageSize.Height);

            DOut.pl(doc.ToString());
            doc.Save(@"f:\work2\test.svg");
        }

        public bool ExecCommand(string s)
        {
            string[] ss = Regex.Split(s, @"[ \t]+");

            string cmd = ss[0];

            if (cmd == "@loadOff")
            {
                testLoadOff();
            }

            else if (cmd == "@svg")
            {
                testSvg();
            }

            else if (cmd == "@triangle")
            {
                testTriangulate();
            }

            else if (cmd == "@nu")
            {
                testNu();
            }
            else if (cmd == "@nus")
            {
                testNus();
            }
            else if (cmd == "@nus2")
            {
                testNus2();
            }
            else if (cmd == "@testMesh")
            {
                testMesh();
            }
            else if (cmd == "@testInvert")
            {
                testInvert();
            }

            else if (cmd == "@loadDxf")
            {
                testLoadDxf();

            }

            else if (cmd == "@test")
            {
                Test();
            }

            else if (cmd == "@tcons1")
            {
                ItConsole.println("test");
            }
            else if (cmd == "@tcons2")
            {
                ItConsole.println("test" + AnsiEsc.BCyan + "-cyan-" + AnsiEsc.Reset + "abc");
            }
            else if (cmd == "@tcons3")
            {
                ItConsole.print("test");
                ItConsole.print(AnsiEsc.BGreen + "xx");
                ItConsole.print("-Green!!!");
                ItConsole.print(AnsiEsc.Reset);
                ItConsole.print("abc");
                ItConsole.print("\n");
            }
            else if (cmd == "@tcons4")
            {
                ItConsole.print("1/5");
                Thread.Sleep(1000);
                ItConsole.print("\r2/5");
                Thread.Sleep(1000);
                ItConsole.print("\r3/5");
                Thread.Sleep(1000);
                ItConsole.print("\r4/5");
                Thread.Sleep(1000);
                ItConsole.print("\r5/5");
                ItConsole.print("\nFinish!\n");
            }
            else
            {
                return false;
            }

            return true;
        }

        public void Redraw()
        {
            RunOnMainThread(() => {
                Controller.Clear();
                Controller.DrawAll();
                Controller.PushDraw();
            });
        }

        public void RunOnMainThread(Action action)
        {
            ThreadUtil.RunOnMainThread(action, true);
        }
    }
}