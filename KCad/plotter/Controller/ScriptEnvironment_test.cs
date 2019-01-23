using System;
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

namespace Plotter.Controller
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
            Controller.HistoryMan.foward(ope);
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


            CadMesh cm = MeshMaker.CreateExtruded(tfig.GetPoints(16), CadVector.UnitZ * -20);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
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

            CadVector v1 = fig.PointList[0] - fig.PointList[1];
            CadVector v2 = fig.PointList[2] - fig.PointList[1];

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

                    CadVector p = fig.PointList[j];

                    p.z = 0;

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
            CadVector p0;
            CadVector p1;

            fig.ForEachSegment(seg =>
            {
                p0 = dc.WorldPointToDevPoint(seg.P0);
                p1 = dc.WorldPointToDevPoint(seg.P1);

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
                        CadVector sp = dc.WorldPointToDevPoint(p);
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


                ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, mesh.ID);
                opeRoot.Add(ope);

                Controller.CurrentLayer.AddFigure(mesh);


                ope = CadOpe.CreateRemoveFigureOpe(Controller.CurrentLayer, fig.ID);
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

            VectorList vl =SplineUtil.CreateFlatControlPoints(ucnt, vcnt, CadVector.UnitX * 20.0, CadVector.UnitZ * 20.0);

            nfig.Setup(2, ucnt, vcnt, vl, null, 16, 16);


            Controller.CurrentLayer.AddFigure(nfig);

            Controller.UpdateTreeView(true);
        }

        private void testNus2()
        {
            CadFigureNurbsSurface nfig = (CadFigureNurbsSurface)Controller.DB.NewFigure(CadFigure.Types.NURBS_SURFACE);

            int ucnt = 4;
            int vcnt = 4;

            VectorList vl = SplineUtil.CreateBoxControlPoints(
                ucnt, vcnt, CadVector.UnitX * 20.0, CadVector.UnitZ * 20.0, CadVector.UnitY * -20.0 );

            nfig.Setup(2, ucnt*2, vcnt, vl, null, 16, 16, false, false, true, true);

            Controller.CurrentLayer.AddFigure(nfig);

            Controller.UpdateTreeView(true);
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
            #region 別スレッド例外処理のテスト
            //CadFigure fig = null;

            //await Task.Run(() =>
            //{
            //    fig.AddPoint(CadVector.Create(0, 0, 0));
            //});
            #endregion

            MpCadData data = MpCadData.Create(Controller.DB);

            data.ViewInfo.WorldScale = Controller.CurrentDC.WorldScale;

            data.ViewInfo.PaperSettings.Set(Controller.PageSize);

            byte[] bin_data = MessagePackSerializer.Serialize(data);

            string s = MessagePackSerializer.ToJson(bin_data);

            JObject jo = JObject.Parse(s);

            s = jo.ToString();
        }

        private void SimpleCommand(string s)
        {
            string[] ss = Regex.Split(s, @"[ \t]+");

            string cmd = ss[0];


            if (cmd == "@clear" || s == "@cls")
            {
                ItConsole.clear();
            }
            else if (cmd == "@h" || cmd == "@help")
            {
                if (ss.Length > 1)
                {
                    mScriptFunctions.MyHelp(ss[1]);
                }
            }

            else if (cmd == "@dump")
            {
                Controller.DB.dump();
            }

            else if (cmd == "@clearTemp")
            {
                Controller.TempFigureList.Clear();
            }
            else if (cmd == "@loadOff")
            {
                testLoadOff();

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
            }
        }

        public void Redraw()
        {
            RunOnMainThread(() => {
                Controller.Clear();
                Controller.DrawAll();
                Controller.PushCurrent();
            });
        }
    }
}