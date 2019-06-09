﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using CadDataTypes;
using HalfEdgeNS;
using CarveWapper;
using MeshUtilNS;
using MeshMakerNS;
using KCad;
using static Plotter.CadFigure;
using LibiglWrapper;
using GLUtil;
using OpenTK;

namespace Plotter.Controller
{
    public class ScriptSession
    {
        ScriptEnvironment Env;

        private CadOpeList mCadOpeList = null;

        private bool NeedUpdateTreeView = false;
        private bool NeedRemakeTreeView = false;
        private bool NeedRedraw = false;

        public ScriptSession(ScriptEnvironment env)
        {
            Env = env;
        }

        public CadOpeList OpeList
        {
            get => mCadOpeList;
        }

        public void AddOpe(CadOpe ope)
        {
            mCadOpeList.Add(ope);
        }

        public void Start()
        {
            mCadOpeList = new CadOpeList();

            ResetFlags();
        }

        public void End()
        {
            if (NeedUpdateTreeView)
            {
                UpdateTV(NeedRemakeTreeView);
            }

            if (NeedRedraw)
            {
                Redraw();
            }
        }

        public void ResetFlags()
        {
            NeedUpdateTreeView = false;
            NeedRemakeTreeView = false;
            NeedRedraw = false;
        }

        public void PostUpdateTreeView()
        {
            NeedUpdateTreeView = true;
        }

        public void PostRemakeTreeView()
        {
            NeedUpdateTreeView = true;
            NeedRemakeTreeView = true;
        }

        public void PostRedraw()
        {
            NeedRedraw = true;
        }

        public void UpdateTV(bool remakeTree)
        {
            Env.RunOnMainThread(() =>
            {
                Env.Controller.UpdateTreeView(remakeTree);
            });
        }

        public void Redraw()
        {
            Env.RunOnMainThread(() =>
            {
                Env.Controller.Clear();
                Env.Controller.DrawAll();
                Env.Controller.PushCurrent();
            });
        }
    }


    public class ScriptFunctions
    {
        PlotterController Controller;

        ScriptEnvironment Env;

        public ScriptSession Session;

        public ScriptFunctions(ScriptEnvironment env)
        {
            Env = env;
            Controller = env.Controller;

            Session = new ScriptSession(Env);
        }
        
        public void StartSession()
        {
            Session.Start();
        }

        public void EndSession()
        {
            Session.End();

            if (Session.OpeList.Count() > 0)
            {
                Controller.HistoryMan.foward(Session.OpeList);
            }
        }

        public void PutMsg(string s)
        {
            ItConsole.println(s);
        }

        public void CursorAngleX(double d)
        {
            double t = -CadMath.Deg2Rad(d);

            Controller.CrossCursor.DirX.X = Math.Cos(t);
            Controller.CrossCursor.DirX.Y = Math.Sin(t);
        }

        public void CursorAngleY(double d)
        {
            double t = -CadMath.Deg2Rad(d) + Math.PI / 2;

            Controller.CrossCursor.DirY.X = Math.Cos(t);
            Controller.CrossCursor.DirY.Y = Math.Sin(t);
        }

        public void PrintVector(CadVertex v)
        {
            var sb = new StringBuilder();

            sb.Append(CadUtil.ValToString(v.X));
            sb.Append(", ");
            sb.Append(CadUtil.ValToString(v.Y));
            sb.Append(", ");
            sb.Append(CadUtil.ValToString(v.Z));

            ItConsole.println(sb.ToString());
        }

        public CadVertex WorldVToDevV(CadVertex w)
        {
            return Controller.CurrentDC.WorldVectorToDevVector(w);
        }

        public CadVertex DevVToWorldV(CadVertex d)
        {
            return Controller.CurrentDC.DevVectorToWorldVector(d);
        }

        public CadVertex WorldPToDevP(CadVertex w)
        {
            return Controller.CurrentDC.WorldVectorToDevVector(w);
        }

        public CadVertex DevPToWorldP(CadVertex d)
        {
            return Controller.CurrentDC.DevVectorToWorldVector(d);
        }

        public void DumpVector(CadVertex v)
        {
            string s = v.CoordString();
            ItConsole.println(s);
        }

        public Vector3d GetLastDownPoint()
        {
            return Controller.LastDownPoint;
        }

        public CadVertex CreateVertex(double x, double y, double z)
        {
            return CadVertex.Create(x, y, z);
        }

        public Vector3d CreateVector(double x, double y, double z)
        {
            return new Vector3d(x, y, z);
        }

        public CadVertex GetProjectionDir()
        {
            CadVertex viewv = CadVertex.Create(Controller.CurrentDC.ViewDir);
            return -viewv;
        }

        public void FindFigureById(uint id)
        {
            int idx = Controller.FindTreeViewItem(id);

            if (idx < 0)
            {
                ItConsole.println(
                    String.Format("ID:{0} is not found", id));
                return;
            }

            Controller.SetTreeViewPos(idx);
        }

        public void LayerList()
        {
            foreach (CadLayer layer in Controller.DB.LayerList)
            {
                ItConsole.println("layer{Name: " + layer.Name + " ID: " + layer.ID + "}");
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

        public void Scale(uint id, CadVertex org, double scale)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            fig.Select();

            Controller.StartEdit();

            fig.ForEachFig((f) => {
                CadUtil.ScaleFigure(f, org, scale);
            });

            Controller.EndEdit();

            Session.PostRedraw();
        }

        public List<CadFigure> FilterRootFigure(List<CadFigure> srcList)
        {
            HashSet<CadFigure> set = new HashSet<CadFigure>();

            foreach (CadFigure fig in srcList)
            {
                set.Add(FigUtil.GetRootFig(fig));
            }

            List<CadFigure> ret = new List<CadFigure>();

            ret.AddRange(set);

            return ret;
        }

        // Scriptから使いやすいようにintで受ける
        public List<CadFigure> ToFigList(IList<int> idList)
        {
            var figList = new List<CadFigure>();

            foreach (uint id in idList)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                if (fig != null)
                {
                    figList.Add(fig);
                }
            }

            return figList;
        }

        public List<CadFigure> GetSelectedFigList()
        {
            return Controller.DB.GetSelectedFigList();
        }

        public void Group(IList<int> idList)
        {
            List<CadFigure> figList = ToFigList(idList);
            Group(figList);
        }

        public void Group(List<CadFigure> targetList)
        {
            List<CadFigure> list = FilterRootFigure(targetList);

            if (list.Count < 2)
            {
                ItConsole.println(
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

                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);

                opeRoot.Add(ope);

                Controller.CurrentLayer.RemoveFigureByIndex(idx);

                parent.AddChild(fig);
            }

            Controller.CurrentLayer.AddFigure(parent);

            ope = new CadOpeAddChildlen(parent, parent.ChildList);

            opeRoot.Add(ope);

            ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, parent.ID);

            opeRoot.Add(ope);

            Session.AddOpe(opeRoot);

            ItConsole.println(
                    global::KCad.Properties.Resources.notice_was_grouped
                );

            Session.PostRemakeTreeView();
        }

        public void Ungroup(IList<int> idList)
        {
            List<CadFigure> figList = ToFigList(idList);
            Ungroup(figList);
        }

        public void Ungroup(int id)
        {
            List<CadFigure> figList = new List<CadFigure>();

            CadFigure fig = Controller.DB.GetFigure((uint)id);

            if (fig == null)
            {
                return;
            }

            figList.Add(fig);

            Ungroup(figList);
        }

        public void Ungroup(List<CadFigure> targetList)
        {
            List<CadFigure> list = FilterRootFigure(targetList);

            CadOpeList opeList = new CadOpeList();

            CadOpe ope;

            foreach (CadFigure root in list)
            {
                root.ForEachFig((fig) => {
                    if (fig.Parent == null)
                    {
                        return;
                    }

                    fig.Parent = null;
                    
                    if (fig.PointCount > 0)
                    {
                        ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                        opeList.Add(ope);
                        Controller.CurrentLayer.AddFigure(fig);
                    }
                });

                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, root.ID);
                opeList.Add(ope);
                Controller.CurrentLayer.RemoveFigureByID(root.ID);
            }

            Session.AddOpe(opeList);

            ItConsole.println(
                global::KCad.Properties.Resources.notice_was_ungrouped
                );

            Session.PostRemakeTreeView();
        }

        public void MoveLastDownPoint(double x, double y, double z)
        {
            Vector3d p = Controller.GetLastDownPoint();

            Vector3d delta = new Vector3d(x, y, z);

            p += delta;

            Controller.SetLastDownPoint(p);

            Session.PostRedraw();
        }

        public void SetLastDownPoint(double x, double y, double z)
        {
            Vector3d p = new Vector3d(x, y, z);

            Controller.SetLastDownPoint(p);

            Session.PostRedraw();
        }

        public void AddLine(CadVertex v0, CadVertex v1)
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            fig.AddPoint(v0);
            fig.AddPoint(v1);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public void AddPoint(double x, double y, double z)
        {
            CadVertex p = default(CadVertex);

            p.Set(x, y, z);

            AddPoint(p);
        }

        public void AddPoint(CadVertex p)
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POINT);
            fig.AddPoint(p);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public int Rect(double w, double h)
        {
            return RectAt(Controller.LastDownPoint, w, h);
        }

        public int RectAt(Vector3d p, double w, double h)
        {
            Vector3d viewDir = Controller.CurrentDC.ViewDir;
            Vector3d upDir = Controller.CurrentDC.UpVector;

            Vector3d wd = CadMath.Normal(viewDir, upDir) * w;
            Vector3d hd = upDir.UnitVector() * h;

            CadVertex p0 = (CadVertex)p;
            CadVertex p1 = (CadVertex)p;

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.RECT);

            fig.AddPoint(p0);

            p1 = p0 + wd;
            fig.AddPoint(p1);

            p1 = p0 + wd + hd;
            fig.AddPoint(p1);

            p1 = p0 + hd;
            fig.AddPoint(p1);

            fig.IsLoop = true;

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);
            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();

            return (int)fig.ID;
        }

        public void AddBox(double x, double y, double z)
        {
            CadMesh cm =
                MeshMaker.CreateBox(
                    Controller.LastDownPoint, CadVertex.Create(x, y, z), MeshMaker.FaceType.TRIANGLE);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);
            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();
        }

        public void AddCylinder(int slices, double r, double len)
        {
            CadMesh cm = MeshMaker.CreateCylinder(Controller.LastDownPoint, slices, r, len);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);
            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();
        }

        public void AddSphere(int slices, double r)
        {
            CadMesh cm = MeshMaker.CreateSphere(r, slices, slices);

            HeModel hem = HeModelConverter.ToHeModel(cm);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);
            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();
        }

        public void AddLayer(string name)
        {
            Controller.AddLayer(name);
        }

        public void Move(uint figID, double x, double y, double z)
        {
            Vector3d delta = new Vector3d(x, y, z);

            CadFigure fig = Controller.DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            //Controller.SelectFigure(figID);

            var list = new List<CadFigure>() { fig };

            Controller.StartEdit(list);

            fig.ForEachFig((f) =>
            {
                f.MoveAllPoints(delta);
            });

            Controller.EndEdit(list);

            Session.PostRedraw();
        }

        public void MoveSelectedPoint(double x, double y, double z)
        {
            var figList = Controller.GetSelectedFigureList();

            Controller.StartEdit(figList);

            CadVertex d = CadVertex.Create(x, y, z);

            foreach (CadFigure fig in figList)
            {
                int i;
                for (i=0; i<fig.PointCount; i++)
                {
                    CadVertex v = fig.PointList[i];
                    if (v.Selected)
                    {
                        v += d;
                        fig.PointList[i] = v;
                    }
                }
            }

            Controller.EndEdit(figList);
        }

        public void SetSelectedSegLen(double len)
        {
            if (Controller.LastSelSegment == null)
            {
                return;
            }

            MarkSegment seg = Controller.LastSelSegment.Value;

            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = Controller.DB.GetFigure(seg.FigureID);

            CadVertex pa = fig.GetPointAt(seg.PtIndexA);
            CadVertex pb = fig.GetPointAt(seg.PtIndexB);

            CadVertex v;

            v = pa - Controller.LastDownPoint;
            double da = v.Norm();

            v = pb - Controller.LastDownPoint;
            double db = v.Norm();


            if (da < db)
            {
                CadVertex np = CadUtil.LinePoint(pb, pa, len);
                Controller.StartEdit();

                fig.SetPointAt(seg.PtIndexA, np);

                Controller.EndEdit();
            }
            else
            {
                CadVertex np = CadUtil.LinePoint(pa, pb, len);
                Controller.StartEdit();

                fig.SetPointAt(seg.PtIndexB, np);

                Controller.EndEdit();
            }
        }

        public void InsPoint()
        {
            Controller.StartEdit();

            if (!Controller.InsPointToLastSelectedSeg())
            {
                Controller.AbendEdit();

                ItConsole.println(
                    global::KCad.Properties.Resources.error_operation_failed
                    );
                return;
            }

            Controller.EndEdit();

            ItConsole.println(
                global::KCad.Properties.Resources.notice_operation_success
                );
        }

        public double Area()
        {
            double area = PlotterUtil.Area(Controller);
            ItConsole.println("Area: " + (area / 100).ToString() + " (㎠)");

            return area;
        }

        public CadVertex Centroid()
        {
            Centroid c = PlotterUtil.Centroid(Controller);
            return c.Point;
        }

        public CadVertex NewPoint()
        {
            return default(CadVertex);
        }

        public CadFigure NewPolyLines()
        {
            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            return fig;
        }

        public void Rotate(uint figID, Vector3d org, Vector3d axisDir, double angle)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);

            if (fig == null)
            {
                return;
            }

            if (axisDir.IsZero())
            {
                return;
            }

            //Controller.SelectFigure(figID);

            axisDir = axisDir.UnitVector();

            var list = new List<CadFigure>() { fig };

            Controller.StartEdit(list);

            RotateWithAxis(fig, org, axisDir, CadMath.Deg2Rad(angle));

            Controller.EndEdit(list);

            Session.PostRedraw();
        }

        public void RotateWithAxis(CadFigure fig, Vector3d org, Vector3d axisDir, double angle)
        {
            fig.ForEachFig(f =>
            {
                CadUtil.RotateFigure(f, org, axisDir, angle);
            });
        }

        public void CreateBitmap(int w, int h, uint argb, int lineW, string fname)
        {
            // TODO tdcのスケーリングがおかしいので直す必要がある

            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            List<uint> idlist = Controller.DB.GetSelectedFigIDList();

            var figList = new List<CadFigure>();

            idlist.ForEach(id =>
            {
                figList.Add(db.GetFigure(id));
            });

            CadRect r = CadUtil.GetContainsRectScrn(dc, figList);

            CadRect wr = default(CadRect);
            wr.p0 = dc.DevPointToWorldPoint(r.p0);
            wr.p1 = dc.DevPointToWorldPoint(r.p1);

            DrawContextGDIBmp tdc = new DrawContextGDIBmp();

            tdc.WorldScale = dc.WorldScale;

            tdc.SetCamera(dc.Eye, dc.LookAt, dc.UpVector);
            tdc.CalcProjectionMatrix();

            tdc.SetViewSize(w, h);

            tdc.SetViewOrg(new Vector3d(w / 2, h / 2, 0));

            tdc.SetupTools(DrawTools.ToolsType.DARK);

            Pen pen = new Pen(Color.FromArgb((int)argb), lineW);

            DrawPen drawPen = DrawPen.New(pen);

            double sw = r.p1.X - r.p0.X;
            double sh = r.p1.Y - r.p0.Y;

            double a = Math.Min(w, h) / (Math.Max(sw, sh) + lineW);

            tdc.DeviceScaleX *= a;
            tdc.DeviceScaleY *= a;

            CadRect tr = CadUtil.GetContainsRectScrn(tdc, figList);

            Vector3d trcp = (Vector3d)((tr.p1 - tr.p0) / 2 + tr.p0);

            Vector3d d = trcp - tdc.ViewOrg;

            tdc.SetViewOrg(tdc.ViewOrg - d);

            Env.RunOnMainThread((Action)(() =>
            {
                tdc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_TRANSPARENT));

                tdc.GdiGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (CadFigure fig in figList)
                {
                    fig.Draw(tdc, drawPen);
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
                drawPen.DisposeGdiPen();
            }));
        }

        public void FaceToDirection(Vector3d dir)
        {
            DrawContext dc = Controller.CurrentDC;

            CadObjectDB db = Controller.DB;

            CadFigure fig = GetTargetFigure();

            if (fig == null)
            {
                return;
            }

            FaceToDirection(fig, Controller.LastDownPoint, dir);
        }

        private void FaceToDirection(CadFigure fig, Vector3d org, Vector3d dir)
        {
            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            Vector3d faceNormal = CadUtil.RepresentativeNormal(fig.PointList).vector;

            if (faceNormal.EqualsThreshold(dir) || (-faceNormal).EqualsThreshold(dir))
            {
                // Face is already target direction
                return;
            }

            //   | 回転軸 rv
            //   |
            //   |
            //   | --------->向けたい方向 dir
            //   /
            //  /
            // 面の法線 faceNormal
            Vector3d rv = CadMath.Normal(faceNormal, dir);

            double t = CadMath.AngleOfVector(faceNormal, dir);

            CadUtil.RotateFigure(fig, org, rv, t);
        }

        public void Triangulate(uint figID, double minArea, double maxDegree)
        {
            string option = $"a{minArea}q{maxDegree}";

            Triangulate(figID, option);
        }

        // option:
        // e.g.
        // a100q30 max area = 100, min degree = 30
        // a100q max area = 100, min degree = default (20)
        // min degree < 34
        // Other options see
        // https://www.cs.cmu.edu/~quake/triangle.switch.html
        //
        public void Triangulate(uint figID, string option)
        {
            CadFigure tfig = Controller.DB.GetFigure(figID);

            if (tfig == null || tfig.Type != Types.POLY_LINES)
            {
                return;
            }

            if (tfig.PointCount < 3)
            {
                return;
            }

            CadFigure cfig = FigUtil.Clone(tfig);

            Vector3d org = cfig.PointList[0].vector;
            Vector3d dir = Vector3d.UnitZ;

            Vector3d faceNormal = CadUtil.RepresentativeNormal(cfig.PointList).vector;

            Vector3d rotateV = default;

            double t = 0;

            if (!faceNormal.EqualsThreshold(dir) && !(-faceNormal).EqualsThreshold(dir))
            {
                rotateV = CadMath.Normal(faceNormal, dir);
                t = -CadMath.AngleOfVector(faceNormal, dir);
                CadUtil.RotateFigure(cfig, org, rotateV, t);
            }

            //Controller.CurrentLayer.AddFigure(cfig);

            VertexList vl = cfig.GetPoints(12);

            CadMesh m = IglW.Triangulate(vl, option);

            HeModel hem = HeModelConverter.ToHeModel(m);

            CadFigureMesh fig = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            fig.SetMesh(hem);

            for (int i = 0; i<fig.PointCount; i++)
            {
                CadVertex v = fig.PointList[i];
                v.Z = org.Z;
                fig.PointList[i] = v;
            }

            if (t != 0)
            {
                CadUtil.RotateFigure(fig, org, rotateV, -t);
            }

            CadOpeList root = new CadOpeList();
            CadOpe ope;

            ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);

            Controller.CurrentLayer.AddFigure(fig);


            ope = new CadOpeRemoveFigure(Controller.CurrentLayer, figID);
            Session.AddOpe(ope);

            Controller.CurrentLayer.RemoveFigureByID(figID);
            Controller.CurrentFigure = null;

            Session.PostRemakeTreeView();
        }

        // 押し出し
        public void Extrude(uint id, CadVertex v, double d, int divide)
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

            CadOpeList root = new CadOpeList();
            CadOpe ope;

            ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            root.Add(ope);

            ope = new CadOpeRemoveFigure(Controller.CurrentLayer, tfig.ID);
            root.Add(ope);

            Session.AddOpe(root);

            Controller.CurrentLayer.AddFigure(fig);
            Controller.CurrentLayer.RemoveFigureByID(tfig.ID);

            Session.PostRemakeTreeView();
        }

        public void ToPolyLine(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (!(fig is CadFigureMesh))
            {
                return;
            }

            CadFigureMesh figMesh = (CadFigureMesh)fig;

            HeModel hm = figMesh.mHeModel;


            CadFigure figPoly = Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);

            hm.ForReachEdgePoint(v =>
            {
                figPoly.AddPoint(v);
            });

            if (figPoly.PointCount < 1)
            {
                return;
            }

            figPoly.IsLoop = true;


            CadOpeList opeRoot = new CadOpeList();
            CadOpe ope;

            ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, figPoly.ID);
            opeRoot.Add(ope);

            Controller.CurrentLayer.AddFigure(figPoly);


            ope = new CadOpeRemoveFigure(Controller.CurrentLayer, fig.ID);
            opeRoot.Add(ope);

            Controller.CurrentLayer.RemoveFigureByID(fig.ID);


            Session.AddOpe(opeRoot);

            Env.RunOnMainThread(() =>
            {
                Controller.ClearSelection();
            });

            Session.PostRemakeTreeView();
        }

        public void ToMesh(uint id)
        {
            CadOpeList opeRoot = new CadOpeList();

            CadOpe ope;

            CadFigure orgFig = Controller.DB.GetFigure(id);


            if (orgFig == null)
            {
                return;    
            }

            if (orgFig.Type != CadFigure.Types.POLY_LINES)
            {
                return;
            }

            CadFigureMesh mesh = (CadFigureMesh)Controller.DB.NewFigure(CadFigure.Types.MESH);

            mesh.CreateModel(orgFig);

            foreach (CadFigure fig in orgFig.ChildList)
            {
                mesh.AddChild(fig);
            }

            CadFigure parent = orgFig.Parent;

            if (parent != null)
            {
                int index = orgFig.Parent.ChildList.IndexOf(orgFig);

                // Remove original poly lines object
                ope = new CadOpeRemoveChild(parent, orgFig, index);
                opeRoot.Add(ope);

                orgFig.Parent.ChildList.Remove(orgFig);

                // Insert mesh object
                ope = new CadOpeAddChild(parent, mesh, index);
                opeRoot.Add(ope);

                orgFig.Parent.ChildList.Insert(index, mesh);
                mesh.Parent = parent;
            }
            else
            {
                // Remove original poly lines object
                ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, mesh.ID);
                opeRoot.Add(ope);
                Controller.CurrentLayer.AddFigure(mesh);

                // Insert mesh object
                ope = new CadOpeRemoveFigure(Controller.CurrentLayer, orgFig.ID);
                opeRoot.Add(ope);
                Controller.CurrentLayer.RemoveFigureByID(orgFig.ID);
            }

            Session.AddOpe(opeRoot);

            Env.RunOnMainThread(() =>
            {
                Controller.ClearSelection();
            });

            Session.PostRemakeTreeView();
            //PrintSuccess();
        }

        public void InvertDir()
        {
            List<CadFigure> figList = Controller.DB.GetSelectedFigList();

            CadOpeList opeRoot = new CadOpeList();
            CadOpeInvertDir ope;

            for (int i = 0; i < figList.Count; i++)
            {
                CadFigure fig = figList[i];
                fig.InvertDir();

                ope = new CadOpeInvertDir(fig.ID);
                opeRoot.Add(ope);
            }

            Session.AddOpe(opeRoot);
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

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();
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

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();
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

            CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
            Session.AddOpe(ope);

            Controller.CurrentLayer.AddFigure(fig);

            Session.PostRemakeTreeView();
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
                CadVertex v = cm.VertexStore[i];
                ItConsole.printf("{0}:{1},{2},{3}\n", i, v.X, v.Y, v.Z);
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

        public CadVertex RotateVector(CadVertex v, CadVertex axis, double angle)
        {
            axis = axis.UnitVector();

            double t = CadMath.Deg2Rad(angle);

            CadQuaternion q = CadQuaternion.RotateQuaternion(axis.vector, t);
            CadQuaternion r = q.Conjugate(); ;

            CadQuaternion qp;

            qp = CadQuaternion.FromPoint(v.vector);

            qp = r * qp;
            qp = qp * q;

            CadVertex rv = v;

            rv.vector = qp.ToPoint();

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

        public CadFigure GetCurrentFigure()
        {
            return Controller.CurrentFigure;
        }

        public Vector3d InputPoint()
        {
            Env.OpenPopupMessage("Input point", PlotterObserver.MessageType.INPUT);

            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start();

            ItConsole.println(AnsiEsc.Yellow + "Input point >>");

            InteractCtrl.States ret = ctrl.WaitPoint();
            ctrl.End();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                Env.ClosePopupMessage();
                ItConsole.println("Cancel!");
                return VectorUtil.InvalidVector3d;
            }

            Vector3d p = ctrl.PointList[0];

            ItConsole.println(p.CoordString());

            Env.ClosePopupMessage();

            return p;
        }

        public Vector3d ViewDir()
        {
            return Controller.CurrentDC.ViewDir;
        }

        public Vector3d InputUnitVector()
        {
            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start();

            ItConsole.println(AnsiEsc.Yellow + "Input point 1 >>");

            InteractCtrl.States ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return VectorUtil.InvalidVector3d;
            }

            Vector3d p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());

            ItConsole.println(AnsiEsc.Yellow + "Input point 2 >>");

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return VectorUtil.InvalidVector3d;
            }

            Vector3d p1 = Controller.mInteractCtrl.PointList[1];

            ctrl.End();


            Vector3d v = p1 - p0;

            v = v.UnitVector();

            ItConsole.println(v.CoordString());

            return v;
        }

        public void UpdateTV()
        {
            Env.RunOnMainThread(() =>
            {
                Controller.UpdateTreeView(true);
            });
        }

        //public void UpdateViews(bool redraw, bool remakeTree)
        //{
        //    Env.RunOnMainThread(() => {
        //        Controller.NotifyDataChanged(redraw);
        //        Controller.UpdateTreeView(remakeTree);
        //    });
        //}

        public void Redraw()
        {
            Env.RunOnMainThread(() =>
            {
                Controller.Clear();
                Controller.DrawAll();
                Controller.PushCurrent();
            });
        }

        //public void RunOnMainThread(Action action)
        //{
        //    if (mMainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
        //    {
        //        action();
        //        return;
        //    }

        //    Task task = new Task(() =>
        //    {
        //        action();
        //    }
        //    );

        //    task.Start(mMainThreadScheduler);
        //    task.Wait();
        //}

        public void SeturePoint(uint figID, int idx, CadVertex p)
        {
            CadFigure fig = Controller.DB.GetFigure(figID);
            fig.SetPointAt(idx, p);
        }

        public CadFigure CreatePolyLines()
        {
            CadFigurePolyLines fig = (CadFigurePolyLines)Controller.DB.NewFigure(CadFigure.Types.POLY_LINES);
            return fig;
        }

        public void AddFigure(CadFigure fig)
        {
            lock (Controller.DB)
            {
                Controller.DB.AddFigure(fig);
                Controller.CurrentLayer.AddFigure(fig);
                CadOpe ope = new CadOpeAddFigure(Controller.CurrentLayer.ID, fig.ID);
                Session.AddOpe(ope);
            }
        }

        public void Test()
        {
            Env.RunOnMainThread(() =>
            {
                testDraw();
            });
        }

        private void testDraw()
        {
            CadSize2D deviceSize = new CadSize2D(827, 1169);
            CadSize2D pageSize = new CadSize2D(210, 297);

            DrawContext dc = Controller.CurrentDC.CreatePrinterContext(pageSize, deviceSize);

            dc.SetupTools(DrawTools.ToolsType.PRINTER_GL);

            FrameBufferW fb = new FrameBufferW();
            fb.Create((int)deviceSize.Width, (int)deviceSize.Height);

            fb.Begin();

            dc.StartDraw();

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));

            Controller.DrawAllFigure(dc);

            dc.EndDraw();

            Bitmap bmp = fb.GetBitmap();

            fb.End();
            fb.Dispose();

            bmp.Save(@"F:\work\test2.bmp");
        }


        private void PrintSuccess()
        {
            ItConsole.println(AnsiEsc.Green + "Success");
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
    }
}