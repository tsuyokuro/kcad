using KCad.Properties;
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
    public partial class ScriptEnvironment
    {
        private PlotterController Controller;

        private ScriptEngine Engine;

        private ScriptScope Scope;

        private ScriptSource Source;

        private List<string> mAutoCompleteList = new List<string>();

        private bool UpdateTreeViewFlag = false; 

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

        //Regex FuncPtn = new Regex(@"def[ \t]+(\w+\(.*\))\:");
        Regex AutoCompPtn = new Regex(@"#\[AC\][ \t]*(.+)\r\n");

        private void InitScriptingEngine()
        {
            string script = System.Text.Encoding.GetEncoding("Shift_JIS").GetString(Resources.BaseScript);

            Engine = IronPython.Hosting.Python.CreateEngine();
            Scope = Engine.CreateScope();
            Source = Engine.CreateScriptSourceFromString(script);

            Scope.SetVariable("SE", this);
            Source.Execute(Scope);

            MatchCollection matches = AutoCompPtn.Matches(script);

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
                Controller.InteractOut.println("layer{Name: " + layer.Name + " ID: " + layer.ID + "}");
            }
        }

        public void PutMsg(string s)
        {
            Controller.InteractOut.println(s);
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

        //public void EndCreateFigure(CadFigure fig)
        //{
        //    fig.EndCreate(Controller.CurrentDC);
        //}

        public void AddFigure(CadFigure fig)
        {
            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.AddFigure(fig);
        }


        //public void SetThickness(double t)
        //{
        //CadFigure fig = GetTargetFigure();

        //CadOpe ope = CadOpe.CreateSetThickOpe(Controller.CurrentLayer.ID, fig.ID, fig.Thickness, t);
        //Controller.HistoryManager.foward(ope);

        //fig.Thickness = t;
        //}


        public CadFigure GetTargetFigure()
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count == 0)
            {
                return null;
            }

            return Controller.DB.GetFigure(idlist[0]);
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

            Exception e = RunCommand(s);

            if (e != null)
            {
                Controller.InteractOut.println("error: " + e.Message);
            }

            if (UpdateTreeViewFlag == true)
            {
                Controller.UpdateTreeView(true);
                UpdateTreeViewFlag = false;
            }

            Controller.DrawAll(Controller.CurrentDC);
            Controller.CurrentDC.Push();
        }

        public async void commandAsync(string s)
        {
            s = s.Trim();
            Controller.InteractOut.println("> " + s);

            if (s.StartsWith("@"))
            {
                SimpleCommand(s);
                return;
            }

            Exception e = null;

            await Task.Run( () =>
            {
                e = RunCommand(s);
            });

            if (e != null)
            {
                Controller.InteractOut.println("error: " + e.Message);
            }

            if (UpdateTreeViewFlag == true)
            {
                Controller.UpdateTreeView(true);
                UpdateTreeViewFlag = false;
            }

            Controller.Clear();
            Controller.DrawAll();
            Controller.PushCurrent();
        }

        public Exception RunCommand(string s)
        {
            UpdateTreeViewFlag = false;

            try
            {
                dynamic ret = Engine.Execute(s, Scope);

                if (ret != null)
                {
                    Controller.InteractOut.println(AnsiEsc.ABlue + ret.ToString());
                }
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }

        public async void runScriptAsync(string s, RunCallback callback)
        {
            if (callback != null)
            {
                callback.OnStart();
            }

            Exception e = null;

            await Task.Run(() =>
            {
                e = RunScript(s);
            });

            if (e != null)
            {
                Controller.InteractOut.println("error: " + e.Message);
            }

            Controller.UpdateTreeView(true);
            UpdateTreeViewFlag = false;

            Controller.Clear();
            Controller.DrawAll();
            Controller.PushCurrent();

            if (callback != null)
            {
                callback.OnEnd();
            }
        }

        public Exception RunScript(string s)
        {
            return RunCommand(s);
        }

        public class RunCallback
        {
            public Action OnStart = () => { };
            public Action OnEnd = () => { };
        }
    }
}
