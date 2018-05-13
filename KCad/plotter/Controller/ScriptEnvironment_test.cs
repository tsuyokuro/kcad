﻿using KCad.Properties;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CadDataTypes;
using LibiglWrapper;
using HalfEdgeNS;
using CarveWapper;
using MeshMakerNS;

namespace Plotter
{
    public partial class ScriptEnvironment
    {
        private void test001()
        {
            VectorList vl = new VectorList();

            vl.Add(CadVector.Create(0, 20, 0));
            vl.Add(CadVector.Create(15, 15, 0));
            vl.Add(CadVector.Create(18, 0, 0));
            vl.Add(CadVector.Create(15, -15, 0));
            vl.Add(CadVector.Create(10, -20, 0));

            CadMesh cm = MeshMaker.CreateRotatingBody(16, vl);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
            Controller.UpdateTreeView(true);
        }

        private void test002()
        {
            CadMesh cm = MeshMaker.CreateSphere(20, 16, 16);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
            Controller.UpdateTreeView(true);
        }

        private void test003()
        {
            Controller.CurrentLayer.ForEachFig(fig =>
            {
                DebugOut.println("fig:" + fig.ID.ToString());
                return true;
            });
        }

        private void test004()
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            FaceToDirection(dc, fig, Controller.LastDownPoint, (CadVector)dc.ViewDir);
        }

        private void test005()
        {
            CadFigure fig = GetTargetFigure();

            if (fig.PointCount < 3)
            {
                return;
            }

            CadVector v1 = fig.PointList[0] - fig.PointList[1];
            CadVector v2 = fig.PointList[2] - fig.PointList[1];

            double t = CadMath.AngleOfVector(v1, v2);

            double a = CadMath.Rad2Deg(t);

            Controller.InteractOut.println(string.Format("angle:{0}(deg)", a));
        }

        private void test006()
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            List<uint> idlist = Controller.GetSelectedFigIDList();

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

                    CadVector p = fig.PointList[j];

                    p.z = 0;

                    fig.PointList[j] = p;
                }
            }
        }

        private void test007()
        {
            if (!(Controller.CurrentDC is DrawContextGDI))
            {
                return;
            }

            DrawContextGDI dc = (DrawContextGDI)Controller.CurrentDC;

            BitmapData bitmapData = dc.LockBits();

            CadVector p0 = CadVector.Create(50, 50, 0);
            CadVector p1 = CadVector.Create(300, 150, 0);

            CadSegment seg = CadUtil.Clipping2D(10, 10, 210, 110, p0, p1);

            seg.dump();

            if (seg.Valid)
            {
                BitmapUtil.BresenhamLine(bitmapData, seg.P0, seg.P1, 0xff00ffff);
            }

            dc.UnlockBits();

            dc.Push();
        }

        private void test008()
        {
            var dc = Controller.CurrentDC;

            CadFigure fig = GetTargetFigure();

            CadVector p0;
            CadVector p1;

            MinMax2D mm = FigureMinMaxScrn(dc, fig);
        }

        public static MinMax2D FigureMinMaxScrn(DrawContext dc, CadFigure fig)
        {
            MinMax2D mm = MinMax2D.Create();
            CadVector p0;
            CadVector p1;

            fig.ForEachSegment(seg =>
            {
                p0 = dc.CadPointToUnitPoint(seg.P0);
                p1 = dc.CadPointToUnitPoint(seg.P1);

                if (p0.x < mm.MinX)
                {
                    mm.MinX = p0.x;
                }

                if (p1.x < mm.MinX)
                {
                    mm.MinX = p1.x;
                }

                if (p0.x > mm.MaxX)
                {
                    mm.MaxX = p0.x;
                }

                if (p1.x > mm.MaxX)
                {
                    mm.MaxX = p1.x;
                }


                if (p0.y < mm.MinY)
                {
                    mm.MinY = p0.y;
                }

                if (p1.y < mm.MinY)
                {
                    mm.MinY = p1.y;
                }

                if (p0.y > mm.MaxY)
                {
                    mm.MaxY = p0.y;
                }

                if (p1.y > mm.MaxY)
                {
                    mm.MaxY = p1.y;
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
                        CadVector p = fig.PointList[k];
                        CadVector sp = dc.CadPointToUnitPoint(p);
                    }
                }
            }

            sw.Stop();
            DebugOut.println(sw.ElapsedMilliseconds.ToString() + " milli sec");
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

            Controller.InteractOut.printf("{0},{1}\n", 10, 20);
            Controller.InteractOut.print("test");
            Controller.InteractOut.println("_test");
            Controller.InteractOut.println("abc\ndef");
            Controller.InteractOut.println("end");

            DebugOut.printf("DebugOut {0}\n", 10);
            DebugOut.Indent++;
            DebugOut.printf("t1 {0}\n", 10);
            DebugOut.printf("t2 {0}\n", 20);
            DebugOut.reset();
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
            var figlist = Controller.GetSelectedFigList();

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


                ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, mesh.ID);
                opeRoot.Add(ope);

                Controller.CurrentLayer.AddFigure(mesh);


                ope = CadOpe.CreateRemoveFigureOpe(Controller.CurrentLayer, fig.ID);
                opeRoot.Add(ope);

                Controller.CurrentLayer.RemoveFigureByID(fig.ID);
            }

            if (opeRoot.OpeList.Count > 0)
            {
                Controller.HistoryManager.foward(opeRoot);
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

        private void testCreateCylinder()
        {
            CadMesh cm = CarveW.CrateCylinder(32, 8, 80);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);
        }

        private void testCreateRectangular()
        {
            CadMesh cm = CarveW.CrateRectangular(10, 10, 30);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);
        }

        private void testCreateBox()
        {
            CadMesh cm = MeshMaker.CreateBox(CadVector.Zero, CadVector.Create(10,30,40));

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            Controller.CurrentLayer.AddFigure(fig);
        }

        private void testAminusB()
        {
            List<CadFigure> figList = Controller.GetSelectedFigList();

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

            Controller.UpdateTreeView(true);
        }


        private void SimpleCommand(string s)
        {
            if (s == "@clear" || s == "@cls")
            {
                Controller.InteractOut.clear();
            }
            else if (s == "@clearTemp")
            {
                Controller.TempFigureList.Clear();
            }
            else if (s == "@test001")
            {
                test001();
            }
            else if (s == "@test002")
            {
                test002();
            }
            else if (s == "@test003")
            {
                test003();
            }
            else if (s == "@test004")
            {
                test004();
            }
            else if (s == "@test005")
            {
                test005();
            }
            else if (s == "@test006")
            {
                test006();
            }
            else if (s == "@test007")
            {
                test007();
            }
            else if (s == "@test008")
            {
                test008();
            }
            else if (s == "@test009")
            {
                test009();
            }
            else if (s == "@test010")
            {
                test010();
            }
            else if (s == "@test011")
            {
                test011();
            }
            else if (s == "@test012")
            {
                test012();
            }
            else if (s == "@test013")
            {
                test013();
            }
            else if (s == "@loadOff")
            {
                testLoadOff();

            }
            else if (s == "@createCylinder")
            {
                testCreateCylinder();

            }
            else if (s == "@createRect")
            {
                testCreateRectangular();

            }
            else if (s == "@a-b")
            {
                testAminusB();

            }
            else if (s == "@testMesh")
            {
                testMesh();
            }
            else if (s == "@testInvert")
            {
                testInvert();
            }
            else if (s == "@createBox")
            {
                testCreateBox();
            }

            else if (s == "@loadDxf")
            {
                testLoadDxf();

            }

            else
            {
                s = s.Remove(0, 1);

                Controller.CurrentDC.StartDraw();

                Controller.debugCommand(Controller.CurrentDC, s);

                Controller.CurrentDC.EndDraw();

                Controller.CurrentDC.Push();
            }
        }
    }
}