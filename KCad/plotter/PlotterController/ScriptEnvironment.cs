using KCad.Properties;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
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

        public void LayerList()
        {
            foreach (CadLayer layer in Controller.DB.LayerList)
            {
                Controller.InteractOut.print("layer{Name: " + layer.Name + " ID: " + layer.ID + "}");
            }
        }

        public void PutMsg(string s)
        {
            Controller.InteractOut.print(s);
        }

        public CadVector GetLastDownPoint()
        {
            return Controller.LastDownPoint;
        }

        public CadVector CreatePoint(double x, double y, double z)
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
            searcher.SearchAllLayer(dc, pixp, Controller.DB);


            List<MarkPoint> list = searcher.GetXYMatches();


            foreach (MarkPoint mp in list)
            {
                string s = "fig{ id: " + mp.FigureID.ToString() + "; idx: " + mp.PointIndex.ToString() + "; }";
                Controller.InteractOut.print(s);
            }
        }

        public void SelectFigure(uint id)
        {
            CadFigure fig = Controller.DB.getFigure(id);

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
            Controller.InteractOut.print(
                "( " +
                Controller.LastDownPoint.x.ToString() + ", " +
                Controller.LastDownPoint.y.ToString() + ", " +
                Controller.LastDownPoint.z.ToString() +
                " )"
            );
        }

        public void Group()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count < 2)
            {
                Controller.InteractOut.print(
                    global::KCad.Properties.Resources.error_select_2_or_more
                    );

                return;
            }

            CadFigure parent = Controller.DB.newFigure(CadFigure.Types.GROUP);

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.getFigure(id);

                if (fig == null)
                {
                    continue;
                }

                parent.AddChild(fig);
            }

            var ope = new CadOpeAddChildlen(parent, parent.ChildList);
            Controller.HistoryManager.foward(ope);

            Controller.InteractOut.print(
                    global::KCad.Properties.Resources.notice_was_grouped
                );
        }

        public void Ungroup()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            var idSet = new HashSet<uint>();

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.getFigure(id);

                CadFigure root = fig.getGroupRoot();

                if (root.ChildList.Count > 0)
                {
                    idSet.Add(root.ID);
                }
            }

            CadOpeList opeList = new CadOpeList();

            foreach (uint id in idSet)
            {
                CadFigure fig = Controller.DB.getFigure(id);

                CadOpeRemoveChildlen ope = new CadOpeRemoveChildlen(fig, fig.ChildList);

                opeList.OpeList.Add(ope);

                fig.ReleaseAllChildlen();
            }

            Controller.HistoryManager.foward(opeList);

            Controller.InteractOut.print(
                global::KCad.Properties.Resources.notice_was_ungrouped
                );
        }

        public void Distance()
        {
            if (Controller.SelList.List.Count == 2)
            {
                CadVector a = Controller.SelList.List[0].Point;
                CadVector b = Controller.SelList.List[1].Point;

                CadVector d = a - b;

                Controller.InteractOut.print("" + d.Norm() + "(mm)");
            }
            else
            {
                Controller.InteractOut.print(
                    global::KCad.Properties.Resources.error_select_2_points
                    );
            }
        }

        public void MoveCursor(double x, double y, double z)
        {
            Controller.LastDownPoint.x += x;
            Controller.LastDownPoint.y += y;
            Controller.LastDownPoint.z += z;
        }

        public void MoveCursorAbs(double x, double y, double z)
        {
            Controller.LastDownPoint.x = x;
            Controller.LastDownPoint.y = y;
            Controller.LastDownPoint.z = z;
        }

        public void Line(double x, double y, double z)
        {
            CadVector p0 = Controller.LastDownPoint;

            CadVector p1 = default(CadVector);

            p1 = p0;

            p1.x += x;
            p1.y += y;
            p1.z += z;

            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.POLY_LINES);
            fig.AddPoint(p0);
            fig.AddPoint(p1);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.addFigure(fig);


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

            p.set(x, y, z);

            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.POINT);
            fig.AddPoint(p);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.addFigure(fig);
        }

        public void Rect(double w, double h)
        {
            CadVector p0 = default(CadVector);

            p0 = Controller.LastDownPoint;

            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.RECT);

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

            fig.Closed = true;

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.addFigure(fig);
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
                CadFigure fig = Controller.DB.getFigure(id);

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

            CadFigure fig = Controller.DB.getFigure(seg.FigureID);

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
                Controller.InteractOut.print(
                    global::KCad.Properties.Resources.error_operation_failed
                    );
                return;
            }

            Controller.InteractOut.print(
                global::KCad.Properties.Resources.notice_operation_success
                );
        }

        public void Area()
        {
            double area = Controller.Area();
            Controller.InteractOut.print("Area: " + (area / 100).ToString() + " (㎠)");
        }

        public CadVector Centroid()
        {
            Centroid c = Controller.Centroid();
            return c.Point;
        }

        public dynamic ExecPartial(string fname)
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

        public CadVector NewPoint()
        {
            return default(CadVector);
        }

        public CadFigure NewPolyLines()
        {
            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.POLY_LINES);
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
            Controller.CurrentLayer.addFigure(fig);
        }

        public void command(string s)
        {
            s = s.Trim();
            Controller.InteractOut.print("> " + s);


            try
            {
                dynamic ret = Engine.Execute(s, Scope);

                if (ret != null)
                {
                    Controller.InteractOut.print(ret.ToString());
                }
            }
            catch (Exception e)
            {
                Controller.InteractOut.print("error: " + e.Message);
            }
        }
    }
}
