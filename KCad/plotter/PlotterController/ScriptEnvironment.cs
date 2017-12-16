#define NEW_GROUPING

using KCad.Properties;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace Plotter
{
    public class ScriptEnvironment
    {
        private PlotterController Controller;

        private ScriptEngine Engine;

        private ScriptScope Scope;

        private ScriptSource Source;


        private List<string> mAutoCompleteList = new List<string>();

        public List<string> AutoCompleteList
        {
            get
            {
                return mAutoCompleteList;
            }
        }

        public ScriptEnvironment(PlotterController controller)
        {
            Controller = controller;

            InitScriptingEngine();
        }


        Regex FuncPtn = new Regex(@"def[ \t]+(\w+\(.*\))\:");

        private void InitScriptingEngine()
        {
            string script = Resources.BaseScript;

            Engine = IronPython.Hosting.Python.CreateEngine();
            Scope = Engine.CreateScope();
            Source = Engine.CreateScriptSourceFromString(script);

            Scope.SetVariable("SE", this);
            Source.Execute(Scope);

            MatchCollection matches = FuncPtn.Matches(script);

            foreach (Match m in matches)
            {
                string s = m.Groups[1].Value;
                mAutoCompleteList.Add(s);
            }
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

        public void LayerList()
        {
            foreach (CadLayer layer in Controller.DB.LayerList)
            {
                Controller.InteractOut.println("layer{Name: " + layer.Name + " ID: " + layer.ID + "}");
            }
        }

        public void PutMsg(string s)
        {
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

        public void SelectFigure(uint id)
        {
            CadFigure fig = Controller.DB.GetFigure(id);

            if (fig == null)
            {
                return;
            }

            fig.Select();
        }

        Regex FigPtn = new Regex(@"fig[ ]*{[ ]*id\:[ ]*([0-9]+)[ ]*;[ ]*idx\:[ ]*([0-9]+)[ ]*;[ ]*}[ ]*");

        public void MessageSelected(List<string> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            string s = messages[messages.Count - 1];

            Match match = FigPtn.Match(s);

            if (match.Success && match.Groups.Count==3)
            {
                string sId = match.Groups[1].Value;
                string sIdx = match.Groups[2].Value;

                uint id = UInt32.Parse(sId);
                int idx = Int32.Parse(sIdx);

                if (Controller.SelectMode == PlotterController.SelectModes.POINT)
                {
                    Controller.SelectById(id, idx);
                }
                else
                {
                    Controller.SelectById(id, -1);
                }
            }
        }

        public void Scale(double scale)
        {
            CadVector org = Controller.LastDownPoint;
            Controller.ScaleSelectedFigure(org, scale);
        }

        public void ShowLastDownPoint()
        {
            Controller.InteractOut.println(
                "( " +
                Controller.LastDownPoint.x.ToString() + ", " +
                Controller.LastDownPoint.y.ToString() + ", " +
                Controller.LastDownPoint.z.ToString() +
                " )"
            );
        }

#if NEW_GROUPING
        public void Group()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count < 2)
            {
                Controller.InteractOut.println(
                    global::KCad.Properties.Resources.error_select_2_or_more
                    );

                return;
            }

            CadFigure parent = Controller.DB.NewFigure(CadFigure.Types.GROUP);

            CadOpeList opeRoot = new CadOpeList();

            CadOpe ope;

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                if (fig == null)
                {
                    continue;
                }

                int idx = Controller.CurrentLayer.GetFigureIndex(id);

                if (idx < 0)
                {
                    continue;
                }

                ope = CadOpe.CreateRemoveFigureOpe(Controller.CurrentLayer, id);

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

            Controller.UpdateTreeView(true);
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

                if (parent==null)
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

            Controller.UpdateTreeView(true);
        }
#else
        public void Group()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count < 2)
            {
                Controller.InteractOut.println(
                    global::KCad.Properties.Resources.error_select_2_or_more
                    );

                return;
            }

            CadFigure parent = Controller.DB.NewFigure(CadFigure.Types.GROUP);

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                if (fig == null)
                {
                    continue;
                }
                parent.AddChild(fig);
            }

            var ope = new CadOpeAddChildlen(parent, parent.ChildList);
            Controller.HistoryManager.foward(ope);

            Controller.InteractOut.println(
                    global::KCad.Properties.Resources.notice_was_grouped
                );
        }

        public void Ungroup()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            var idSet = new HashSet<uint>();

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                CadFigure root = fig.GetGroupRoot();

                if (root.ChildList.Count > 0)
                {
                    idSet.Add(root.ID);
                }
            }

            CadOpeList opeList = new CadOpeList();

            foreach (uint id in idSet)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                CadOpeRemoveChildlen ope = new CadOpeRemoveChildlen(fig, fig.ChildList);

                opeList.OpeList.Add(ope);

                fig.ReleaseAllChildlen();
            }

            Controller.HistoryManager.foward(opeList);

            Controller.InteractOut.println(
                global::KCad.Properties.Resources.notice_was_ungrouped
                );
        }
#endif
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

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.POINT);
            fig.AddPoint(p);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public void Rect(double w, double h)
        {
            CadVector p0 = default(CadVector);

            p0 = Controller.LastDownPoint;

            CadFigure fig = Controller.DB.NewFigure(CadFigure.Types.RECT);

            CadVector p1 = p0;

            fig.AddPoint(p0);

            p1.x = p0.x + w;
            p1.y = p0.y;
            p1.z = p0.z;
            fig.AddPoint(p1);

            p1.x = p0.x + w;
            p1.y = p0.y + h;
            p1.z = p0.z;
            fig.AddPoint(p1);

            p1.x = p0.x;
            p1.y = p0.y + h;
            p1.z = p0.z;
            fig.AddPoint(p1);

            fig.IsLoop = true;

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
            Controller.UpdateTreeView(true);
        }

        public void AddLayer(string name)
        {
            Controller.AddLayer(name);
        }

        public void ReverseOrder()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.GetFigure(id);

                if (fig == null)
                {
                    continue;
                }

                fig.PointList.Reverse();
            }
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

        public void Area()
        {
            double area = Controller.Area();
            Controller.InteractOut.println("Area: " + (area / 100).ToString() + " (㎠)");
        }

        public CadVector Centroid()
        {
            Centroid c = Controller.Centroid();
            return c.Point;
        }

        public dynamic ExecPartial(string fname)
        {
            try
            {
                Assembly myAssembly = Assembly.GetEntryAssembly();

                string str = "";

                string path = myAssembly.Location;

                path = Path.GetDirectoryName(path) + @"\script\" + fname;


                StreamReader sr = new StreamReader(
                        path, Encoding.GetEncoding("Shift_JIS"));

                str = sr.ReadToEnd();

                sr.Close();

                return Engine.Execute(str, Scope);
            }
            catch (Exception e)
            {
                Controller.InteractOut.println("error: " + e.Message);
                return null;
            }
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

        public void EndCreateFigure(CadFigure fig)
        {
            fig.EndCreate(Controller.CurrentDC);
        }

        public void AddFigure(CadFigure fig)
        {
            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }

        public void Rotate(CadVector p0, CadVector v, double t)
        {
            v = v.UnitVector();

            Controller.StartEdit();

            Controller.RotateSelectedFigure(p0, v, CadMath.Deg2Rad(t));

            Controller.EndEdit();

            Controller.NotifyDataChanged(true);
        }

        public void CreateBitmap(int w, int h, string fname)
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

            tdc.ViewOrg = CadVector.Create(w/2, h/2, 0);

            tdc.SetupTools(DrawTools.ToolsType.DARK);

            Pen pen = tdc.Pen(DrawTools.PEN_DEFAULT_FIGURE);

            double sw = r.p1.x - r.p0.x;
            double sh = r.p1.y - r.p0.y;

            double a = Math.Min(w, h) / (Math.Max(sw, sh) + 1.0);

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

            if (dc is DrawContextGDI)
            {
                ((DrawContextGDI)dc).graphics.DrawImage(tdc.Image, new System.Drawing.Point(0, 0));
            }
            dc.Push();

            tdc.Image.Save(fname);
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

        private void test001()
        {
            CadFigure fig = GetTargetFig();

            if (fig == null) return;

            fig.ForEachFigureSegment(seg =>
            {
                seg.dump(DebugOut.Std);
                return true;
            });
        }

        private void test002()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count < 2)
            {
                return;
            }

            CadFigure fig0 = Controller.DB.GetFigure(idlist[0]);
            CadFigure fig1 = Controller.DB.GetFigure(idlist[1]);

            if (fig0.PointCount < 2) return;
            if (fig1.PointCount < 2) return;


            bool ret = CadUtil.CheckCrossSegSeg2D(
                                fig0.PointList[0], fig0.PointList[1],
                                fig1.PointList[0], fig1.PointList[1]
                                );

            DebugOut.Std.println("CheckCrossSegSeg2D ret=" + ret.ToString());
        }

        private void test003()
        {
            Controller.CurrentLayer.ForEachFig(fig =>
            {
                DebugOut.StdPrintLn("fig:" + fig.ID.ToString());
                return true;
            });
        }

        private void test004()
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

            //dc.Drawing.DrawRectScrn(DrawTools.PEN_LAST_POINT_MARKER, r.p0, r.p1);

            DrawContextGDI tdc = new DrawContextGDI();

            tdc.CopyMetrics(dc);

            tdc.SetViewSize(128, 128);

            tdc.ViewOrg = CadVector.Create(64, 64, 0);

            tdc.SetupTools(DrawTools.ToolsType.DARK);

            Pen pen = tdc.Pen(DrawTools.PEN_DEFAULT_FIGURE);

            //pen.Width = 2;

            double sw = r.p1.x - r.p0.x;
            double sh = r.p1.y - r.p0.y;

            double a = 128.0 / (Math.Max(sw, sh) + 1.0);

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

            if (dc is DrawContextGDI)
            {
                ((DrawContextGDI)dc).graphics.DrawImage(tdc.Image, new System.Drawing.Point(0, 0));
            }

            dc.Push();

            //tdc.Image.Save(@"F:\work3\test.bmp");
        }

        private CadFigure GetTargetFig()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count == 0)
            {
                return null;
            }

            return Controller.DB.GetFigure(idlist[0]);
        }

        private void SimpleCommand(string s)
        {
            if (s == "@clear" || s == "@cls")
            {
                Controller.InteractOut.clear();
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
            else
            {
                s = s.Remove(0, 1);

                Controller.CurrentDC.StartDraw();

                Controller.debugCommand(Controller.CurrentDC, s);

                Controller.CurrentDC.EndDraw();
            }
        }

        public void command(string s)
        {
            s = s.Trim();
            Controller.InteractOut.println("> " + s);

            if (s.StartsWith("@"))
            {
                SimpleCommand(s);
                return;
            }

            try
            {
                dynamic ret = Engine.Execute(s, Scope);

                if (ret != null)
                {
                    Controller.InteractOut.println(ret.ToString());
                }
            }
            catch (Exception e)
            {
                Controller.InteractOut.println("error: " + e.Message);
            }

            Controller.DrawAll(Controller.CurrentDC);
            Controller.CurrentDC.Push();
        }
    }
}
