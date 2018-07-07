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
using System.Windows.Forms;
using CadDataTypes;
using HalfEdgeNS;
using CarveWapper;
using MeshUtilNS;
using MeshMakerNS;
using System.Threading.Tasks;
using KCad;

namespace Plotter.Controller
{
    public class ScriptFunctions
    {
        PlotterController Controller;

        ScriptEnvironment Env;

        TaskScheduler mMainThreadScheduler;

        int mMainThreadID = -1;

        public ScriptFunctions(ScriptEnvironment env)
        {
            Env = env;
            Controller = env.Controller;

            mMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

            mMainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public void PutMsg(string s)
        {
            Controller.InteractOut.println(s);
        }

        public void CursorAngleX(double d)
        {
            double t = -CadMath.Deg2Rad(d);

            Controller.CrossCursor.DirX.x = Math.Cos(t);
            Controller.CrossCursor.DirX.y = Math.Sin(t);
        }

        public void CursorAngleY(double d)
        {
            double t = -CadMath.Deg2Rad(d) + Math.PI / 2;

            Controller.CrossCursor.DirY.x = Math.Cos(t);
            Controller.CrossCursor.DirY.y = Math.Sin(t);
        }

        public void ShowVector(CadVector v)
        {
            Controller.InteractOut.println(
                "( " +
                v.x.ToString() + ", " +
                v.y.ToString() + ", " +
                v.z.ToString() +
                " )"
            );
        }

        public void PrintVector(CadVector v)
        {
            var sb = new StringBuilder();

            sb.Append(CadUtil.ValToString(v.x));
            sb.Append(", ");
            sb.Append(CadUtil.ValToString(v.y));
            sb.Append(", ");
            sb.Append(CadUtil.ValToString(v.z));

            Controller.InteractOut.println(sb.ToString());
        }

        public void DumpVector(CadVector v)
        {
            string s = v.CoordString();
            Controller.InteractOut.println(s);
        }

        public CadVector GetLastDownPoint()
        {
            return Controller.LastDownPoint;
        }

        public CadVector CreateVector(double x, double y, double z)
        {
            return CadVector.Create(x, y, z);
        }

        public CadVector GetProjectionDir()
        {
            CadVector viewv = CadVector.Create(Controller.CurrentDC.ViewDir);
            return -viewv;
        }

        public void Find(double range)
        {
            CadVector org = Controller.LastDownPoint;

            DrawContext dc = Controller.CurrentDC;

            CadVector pixp = dc.CadPointToUnitPoint(org);


            PointSearcher searcher = new PointSearcher();

            searcher.SetRangePixel(dc, range);

            CadCursor cc = CadCursor.Create(pixp);

            searcher.SetTargetPoint(cc);
            searcher.SearchAllLayer(dc, Controller.DB);


            List<MarkPoint> list = searcher.GetXYMatches();


            foreach (MarkPoint mp in list)
            {
                string s = "fig{ id: " + mp.FigureID.ToString() + "; idx: " + mp.PointIndex.ToString() + "; }";
                Controller.InteractOut.println(s);
            }
        }

        public void FindFigureById(uint id)
        {
            int idx = Controller.FindTreeViewItem(id);

            if (idx < 0)
            {
                Controller.InteractOut.println(
                    String.Format("ID:{0} is not found", id));
                return;
            }

            Controller.SetTreeViewPos(idx);
        }

        public void LayerList()
        {
            foreach (CadLayer layer in Controller.DB.LayerList)
            {
                Controller.InteractOut.println("layer{Name: " + layer.Name + " ID: " + layer.ID + "}");
            }
        }

        public void SelectFigure(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            fig.Select();
        }

        public void Scale(double scale)
        {
            CadVector org = Controller.LastDownPoint;
            Controller.ScaleSelectedFigure(org, scale);
        }

        public List<CadFigure> GetRootFigList(List<CadFigure> srcList)
        {
            HashSet<CadFigure> set = new HashSet<CadFigure>();

            foreach (CadFigure fig in srcList)
            {
                set.Add(CadFigure.Util.GetRootFig(fig));
            }

            List<CadFigure> ret = new List<CadFigure>();

            ret.AddRange(set);

            return ret;
        }

        public void Group()
        {
            List<CadFigure> list = GetRootFigList(Controller.GetSelectedFigList());

            if (list.Count < 2)
            {
                Controller.InteractOut.println(
                    global::KCad.Properties.Resources.error_select_2_or_more
                    );

                return;
            }

            CadFigure parent = Controller.DB.NewFigure(CadFigure.Types.GROUP);

            CadOpeList opeRoot = new CadOpeList();

            CadOpe ope;

            foreach (CadFigure fig in list)
            {
                int idx = Controller.CurrentLayer.GetFigureIndex(fig.ID);

                if (idx < 0)
                {
                    continue;
                }

                ope = CadOpe.CreateRemoveFigureOpe(Controller.CurrentLayer, fig.ID);

                opeRoot.Add(ope);

                Controller.CurrentLayer.RemoveFigureByIndex(idx);

                parent.AddChild(fig);
            }

            Controller.CurrentLayer.AddFigure(parent);

            ope = new CadOpeAddChildlen(parent, parent.ChildList);

            opeRoot.Add(ope);

            ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, parent.ID);

            opeRoot.Add(ope);

            Controller.HistoryManager.foward(opeRoot);

            Controller.InteractOut.println(
                    global::KCad.Properties.Resources.notice_was_grouped
                );

            UpdateTreeView();
        }

        public void Ungroup()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            var idSet = new HashSet<uint>();

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                if (fig.Parent != null)
                {
                    idSet.Add(fig.ID);
                }
            }

            CadOpeList opeList = new CadOpeList();

            CadOpe ope;

            foreach (uint id in idSet)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                CadFigure parent = fig.Parent;

                if (parent == null)
                {
                    continue;
                }

                ope = new CadOpeRemoveChild(parent, fig);
                opeList.Add(ope);
                parent.ChildList.Remove(fig);
                fig.Parent = null;

                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                opeList.Add(ope);
                Controller.CurrentLayer.AddFigure(fig);
            }

            Controller.HistoryManager.foward(opeList);

            Controller.InteractOut.println(
                global::KCad.Properties.Resources.notice_was_ungrouped
                );

            UpdateTreeView();
        }

        public void Distance()
        {
            if (Controller.SelList.List.Count == 2)
            {
                CadVector a = Controller.SelList.List[0].Point;
                CadVector b = Controller.SelList.List[1].Point;

                CadVector d = a - b;

                Controller.InteractOut.println("" + d.Norm() + "(mm)");
            }
            else
            {
                Controller.InteractOut.println(
                    global::KCad.Properties.Resources.error_select_2_points
                    );
            }
        }

        public void MoveCursor(double x, double y, double z)
        {
            Controller.LastDownPoint.x += x;
            Controller.LastDownPoint.y += y;
            Controller.LastDownPoint.z += z;

            Controller.NotifyDataChanged(true);
        }

        public void SetCursor(double x, double y, double z)
        {
            Controller.LastDownPoint.x = x;
            Controller.LastDownPoint.y = y;
            Controller.LastDownPoint.z = z;

            Controller.NotifyDataChanged(true);
        }

        public void Line(double x, double y, double z)
        {
            CadVector p0 = Controller.LastDownPoint;

            CadVector p1 = default(CadVector);

            p1 = p0;

            p1.x += x;
            p1.y += y;
            p1.z += z;

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            fig.AddPoint(p0);
            fig.AddPoint(p1);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);


            Controller.LastDownPoint = p1;
        }

        public void AddPoint()
        {
            AddPoint(
                Controller.LastDownPoint.x,
                Controller.LastDownPoint.y,
                Controller.LastDownPoint.z);

        }

        public void AddPoint(double x, double y, double z)
        {
            CadVector p = default(CadVector);

            p.Set(x, y, z);

            AddPoint(p);
        }

        public void AddPoint(CadVector p)
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POINT);
            fig.AddPoint(p);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public int Rect(double w, double h, string plane)
        {
            return RectAt(Controller.LastDownPoint, w, h, plane);
        }

        public int RectAt(CadVector p, double w, double h, string plane)
        {
            CadVector p0 = default(CadVector);

            p0 = p;

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.RECT);

            CadVector p1 = p0;

            fig.AddPoint(p0);

            if (plane == "xy")
            {
                p1 = p0;

                p1.x = p0.x + w;
                fig.AddPoint(p1);

                p1.y = p0.y + h;
                fig.AddPoint(p1);

                p1.x = p0.x;
                fig.AddPoint(p1);
            }
            else if (plane == "xz")
            {
                p1.x = p0.x + w;
                fig.AddPoint(p1);

                p1.z = p0.z - h;
                fig.AddPoint(p1);

                p1.x = p0.x;
                fig.AddPoint(p1);
            }
            else if (plane == "zy")
            {
                p1.z = p0.z + w;
                fig.AddPoint(p1);

                p1.y = p0.y + h;
                fig.AddPoint(p1);

                p1.z = p0.z;
                fig.AddPoint(p1);
            }
            else
            {
                Controller.InteractOut.println("error! \"xy\" or \"xz\" or \"zy\"");
                return 0;
            }

            fig.IsLoop = true;

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);

            return (int)fig.ID;
        }

        public void AddBox(double x, double y, double z)
        {
            CadMesh cm =
                MeshMaker.CreateBox(
                    Controller.LastDownPoint, CadVector.Create(x, y, z), MeshMaker.FaceType.TRIANGLE);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public void AddCylinder(int slices, double r, double len)
        {
            CadMesh cm = MeshMaker.CreateCylinder(Controller.LastDownPoint, slices, r, len);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public void AddSphere(int slices, double r)
        {
            CadMesh cm = MeshMaker.CreateSphere(r, slices, slices);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public void AddLayer(string name)
        {
            Controller.AddLayer(name);
        }

        public void Move(double x, double y, double z)
        {
            CadVector delta = CadVector.Create(x, y, z);

            Controller.MoveSelectedPoints(delta);
        }

        public void SegLen(double len)
        {
            MarkSeg seg = Controller.SelSegList.LastSel;

            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = Controller.DB.GetFigure(seg.FigureID);

            CadVector pa = fig.GetPointAt(seg.PtIndexA);
            CadVector pb = fig.GetPointAt(seg.PtIndexB);

            CadVector v;

            v = pa - Controller.LastDownPoint;
            double da = v.Norm();

            v = pb - Controller.LastDownPoint;
            double db = v.Norm();


            if (da < db)
            {
                CadVector np = CadUtil.LinePoint(pb, pa, len);
                Controller.StartEdit();

                fig.SetPointAt(seg.PtIndexA, np);

                Controller.EndEdit();
            }
            else
            {
                CadVector np = CadUtil.LinePoint(pa, pb, len);
                Controller.StartEdit();

                fig.SetPointAt(seg.PtIndexB, np);

                Controller.EndEdit();
            }
        }

        public void InsPoint()
        {
            if (!Controller.InsPointToLastSelectedSeg())
            {
                Controller.InteractOut.println(
                    global::KCad.Properties.Resources.error_operation_failed
                    );
                return;
            }

            Controller.InteractOut.println(
                global::KCad.Properties.Resources.notice_operation_success
                );
        }

        public double Area()
        {
            double area = Controller.Area();
            Controller.InteractOut.println("Area: " + (area / 100).ToString() + " (㎠)");

            return area;
        }

        public CadVector Centroid()
        {
            Centroid c = Controller.Centroid();
            return c.Point;
        }

        public CadVector NewPoint()
        {
            return default(CadVector);
        }

        public CadFigure NewPolyLines()
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            return fig;
        }

        public void Rotate(CadVector org, CadVector axisDir, double angle)
        {
            axisDir = axisDir.UnitVector();

            Controller.StartEdit();

            Controller.RotateSelectedFigure(org, axisDir, CadMath.Deg2Rad(angle));

            Controller.EndEdit();

            Controller.NotifyDataChanged(true);
        }

        public void CreateBitmap(int w, int h, uint argb, int lineW, string fname)
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            List<uint> idlist = Controller.GetSelectedFigIDList();

            var figList = new List<CadFigure>();

            idlist.ForEach(id =>
            {
                figList.Add(db.GetFigure(id));
            });

            CadRect r = CadUtil.GetContainsRectScrn(dc, figList);

            CadRect wr = default(CadRect);
            wr.p0 = dc.UnitPointToCadPoint(r.p0);
            wr.p1 = dc.UnitPointToCadPoint(r.p1);

            DrawContextGDI tdc = new DrawContextGDI();

            tdc.CopyMetrics(dc);

            tdc.SetViewSize(w, h);

            tdc.ViewOrg = CadVector.Create(w / 2, h / 2, 0);

            tdc.SetupTools(DrawTools.ToolsType.DARK);

            Pen pen = tdc.Pen(DrawTools.PEN_DEFAULT_FIGURE);
            pen.Color = Color.FromArgb((int)argb);
            pen.Width = lineW;

            double sw = r.p1.x - r.p0.x;
            double sh = r.p1.y - r.p0.y;

            double a = Math.Min(w, h) / (Math.Max(sw, sh) + lineW);

            tdc.DeviceScaleX *= a;
            tdc.DeviceScaleY *= a;

            CadRect tr = CadUtil.GetContainsRectScrn(tdc, figList);

            CadVector trcp = (tr.p1 - tr.p0) / 2 + tr.p0;

            CadVector d = trcp - tdc.ViewOrg;

            tdc.ViewOrg -= d;

            tdc.Drawing.Clear(DrawTools.BRUSH_TRANSPARENT);

            tdc.graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (CadFigure fig in figList)
            {
                fig.Draw(tdc, DrawTools.PEN_DEFAULT_FIGURE);
            }

            if (fname.Length > 0)
            {
                tdc.Image.Save(fname);
            }
            else
            {
                BitmapUtil.BitmapToClipboardAsPNG(tdc.Image);
            }

            tdc.Dispose();
        }

        public void FaceToDirection(CadVector dir)
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            FaceToDirection(dc, fig, Controller.LastDownPoint, dir);
        }

        private void FaceToDirection(DrawContext dc, CadFigure fig, CadVector org, CadVector dir)
        {
            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            CadVector faceNormal = CadUtil.RepresentativeNormal(fig.PointList);

            //   | 回転軸
            //   |
            //   |
            //   | --------->向けたい方向
            //   /
            //  /
            // 法線
            CadVector rv = CadMath.Normal(faceNormal, dir);

            double t = CadMath.AngleOfVector(faceNormal, dir);

            CadUtil.RotateFigure(fig, org, rv, t);
        }

        public void SwapXZ(double ax, double az)
        {
            var figlist = Controller.GetSelectedFigList();

            int i = 0;
            int j = 0;

            for (; i < figlist.Count; i++)
            {
                CadFigure fig = figlist[i];

                for (j = 0; j < fig.PointCount; j++)
                {
                    CadVector v = fig.GetPointAt(j);
                    CadVector rv = v;

                    rv.x = az * v.z;
                    rv.z = ax * v.x;

                    fig.SetPointAt(j, rv);
                }

                CadVector nv = fig.Normal;

                fig.Normal.x = az * nv.z;
                fig.Normal.z = ax * nv.x;
            }
        }

        public void SwapYZ(double ay, double az)
        {
            var figlist = Controller.GetSelectedFigList();

            int j = 0;
            int i = 0;

            for (; i < figlist.Count; i++)
            {
                CadFigure fig = figlist[i];

                for (j = 0; j < fig.PointCount; j++)
                {
                    CadVector v = fig.GetPointAt(j);
                    CadVector rv = v;

                    rv.y = az * v.z;
                    rv.z = ay * v.y;

                    fig.SetPointAt(j, rv);
                }

                CadVector nv = fig.Normal;

                fig.Normal.y = az * nv.z;
                fig.Normal.z = ay * nv.y;
            }
        }

        public void Extrude(uint id, CadVector v, double d, int divide)
        {
            CadFigure tfig = Controller.DB.GetFigure(id);

            if (tfig == null || tfig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            v = v.UnitVector();

            v *= -d;

            CadMesh cm = MeshMaker.CreateExtruded(tfig.GetPoints(16), v, divide);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.RecalcNormal();

            fig.SetMesh(hem);

            CadOpeList root = CadOpe.CreateListOpe();
            CadOpe ope;

            ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            root.Add(ope);

            ope = CadOpe.CreateRemoveFigureOpe(Controller.CurrentLayer, tfig.ID);
            root.Add(ope);

            Controller.HistoryManager.foward(root);

            Controller.CurrentLayer.AddFigure(fig);
            Controller.CurrentLayer.RemoveFigureByID(tfig.ID);
        }

        public void ToMesh()
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

            Controller.ClearSelection();
        }

        public void InvertDir()
        {
            List<CadFigure> figList = Controller.GetSelectedFigList();

            CadOpeList opeRoot = new CadOpeList();
            CadOpeInvertDir ope;

            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure fig = figList[i];
                fig.InvertDir();

                ope = new CadOpeInvertDir(fig.ID);
                opeRoot.Add(ope);
            }

            Controller.HistoryManager.foward(opeRoot);
        }

        private CadFigureMesh GetCadFigureMesh(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);
            if (fig == null || fig.Type != CadFigure.Types.MESH) return null;

            return (CadFigureMesh)fig;
        }


        public void AsubB(uint idA, uint idB)
        {
            CadFigureMesh figA = GetCadFigureMesh(idA);
            CadFigureMesh figB = GetCadFigureMesh(idB);

            if (figA == null || figB == null)
            {
                ItConsole.println("invalid ID");
                return;
            }
            HeModel he_a = figA.mHeModel;
            HeModel he_b = figB.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.AMinusB(a, b);

            MeshUtil.SplitAllFace(c);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);

            Controller.CurrentLayer.AddFigure(fig);
        }

        public void Union(uint idA, uint idB)
        {
            CadFigureMesh figA = GetCadFigureMesh(idA);
            CadFigureMesh figB = GetCadFigureMesh(idB);

            if (figA == null || figB == null)
            {
                ItConsole.println("invalid ID");
                return;
            }
            HeModel he_a = figA.mHeModel;
            HeModel he_b = figB.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.Union(a, b);

            MeshUtil.SplitAllFace(c);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);

            Controller.CurrentLayer.AddFigure(fig);
        }

        public void Intersection(uint idA, uint idB)
        {
            CadFigureMesh figA = GetCadFigureMesh(idA);
            CadFigureMesh figB = GetCadFigureMesh(idB);

            if (figA == null || figB == null)
            {
                ItConsole.println("invalid ID");
                return;
            }
            HeModel he_a = figA.mHeModel;
            HeModel he_b = figB.mHeModel;

            CadMesh a = HeModelConverter.ToCadMesh(he_a);
            CadMesh b = HeModelConverter.ToCadMesh(he_b);

            CadMesh c = CarveW.Intersection(a, b);

            MeshUtil.SplitAllFace(c);


            HeModel hem = HeModelConverter.ToHeModel(c);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);

            Controller.CurrentLayer.AddFigure(fig);
        }

        public void DumpMesh(uint id)
        {
            CadFigureMesh fig = GetCadFigureMesh(id);

            if (fig == null)
            {
                ItConsole.println("dumpMesh(id) error: invalid ID");
                return;
            }

            CadMesh cm = HeModelConverter.ToCadMesh(fig.mHeModel);

            for (int i = 0; i < cm.VertexStore.Count; i++)
            {
                CadVector v = cm.VertexStore[i];
                ItConsole.printf("{0}:{1},{2},{3}\n", i, v.x, v.y, v.z);
            }

            for (int i = 0; i < cm.FaceStore.Count; i++)
            {
                CadFace f = cm.FaceStore[i];

                string s = "";

                for (int j = 0; j < f.VList.Count; j++)
                {
                    s += f.VList[j].ToString() + ",";
                }

                ItConsole.println(s);
            }
        }

        public void AddMoveGide(CadVector dir)
        {
            Controller.GideLines.Add(dir);

            //CadVector v2 = RotateVector(dir, (CadVector)(Controller.CurrentDC.ViewDir), 90.0);
            //Controller.GideLines.Add(v2);
        }

        public void ClearMoveGide()
        {
            Controller.GideLines.Clear();
        }

        public void EnableMoveGide(bool enable)
        {
            Controller.GideLines.Enabled = enable;
        }

        public CadVector RotateVector(CadVector v, CadVector axis, double angle)
        {
            axis = axis.UnitVector();

            double t = CadMath.Deg2Rad(angle);

            CadQuaternion q = CadQuaternion.RotateQuaternion(axis, t);
            CadQuaternion r = q.Conjugate(); ;

            CadQuaternion qp;

            qp = CadQuaternion.FromPoint(v);

            qp = r * qp;
            qp = qp * q;

            CadVector rv = qp.ToPoint();

            return rv;
        }

        public uint GetCurrentFigureID()
        {
            CadFigure fig = Controller.CurrentFigure;

            if (fig == null)
            {
                return 0;
            }

            return fig.ID;
        }

        public CadVector InputPoint()
        {
            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start(InteractCtrl.Mode.POINT);

            Controller.println(AnsiEsc.AYellow + "Input point >>");

            InteractCtrl.State ret = ctrl.WaitPoint();
            ctrl.End();

            if (ret != InteractCtrl.State.CONTINUE)
            {
                Controller.println("Cancel!");
                return CadVector.InvalidValue;
            }

            CadVector p = ctrl.PointList[0];

            Controller.println(p.CoordString());

            return p;
        }

        public CadVector InputUnitVector()
        {
            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start(InteractCtrl.Mode.LINE);

            Controller.println(AnsiEsc.AYellow + "Input point 1 >>");

            InteractCtrl.State ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.State.CONTINUE)
            {
                ctrl.End();
                Controller.println("Cancel!");
                return CadVector.InvalidValue;
            }

            CadVector p0 = ctrl.PointList[0];
            Controller.println(p0.CoordString());

            Controller.println(AnsiEsc.AYellow + "Input point 2 >>");

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.State.CONTINUE)
            {
                ctrl.End();
                Controller.println("Cancel!");
                return CadVector.InvalidValue;
            }

            CadVector p1 = Controller.mInteractCtrl.PointList[1];

            ctrl.End();


            CadVector v = p1 - p0;

            v = v.UnitVector();

            Controller.println(v.CoordString());

            return v;
        }

        public dynamic ExecPartial(string fname)
        {
            return Env.ExecPartial(fname);
        }

        public void UpdateTreeView()
        {
            if (mMainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Controller.UpdateTreeView(true);
                return;
            }

            new Task(() =>
            {
                Controller.UpdateTreeView(true);
            }
            ).Start(mMainThreadScheduler);
        }

        public void PostRedraw()
        {
            if (mMainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Controller.Clear();
                Controller.DrawAll();
                Controller.PushCurrent();
                return;
            }

            new Task(() =>
            {
                Controller.Clear();
                Controller.DrawAll();
                Controller.PushCurrent();
            }
            ).Start(mMainThreadScheduler);
        }

        public void Test(CadVector v)
        {

        }


        public CadFigure GetTargetFigure()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count == 0)
            {
                return null;
            }

            return Controller.DB.GetFigure(idlist[0]);
        }
    }
}