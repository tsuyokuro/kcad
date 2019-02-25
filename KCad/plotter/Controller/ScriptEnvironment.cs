using KCad.Properties;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KCad;

namespace Plotter.Controller
{
    public partial class ScriptEnvironment
    {
        public PlotterController Controller;

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

        ThreadUtil mThreadUtil;

        private ScriptFunctions mScriptFunctions;

        public ScriptEnvironment(PlotterController controller)
        {
            Controller = controller;

            mThreadUtil = new ThreadUtil();

            mScriptFunctions = new ScriptFunctions(this);

            InitScriptingEngine();
        }

        Regex AutoCompPtn = new Regex(@"#\[AC\][ \t]*(.+)\n");

        private void InitScriptingEngine()
        {
            string script = System.Text.Encoding.GetEncoding("Shift_JIS").GetString(Resources.BaseScript);

            Engine = IronPython.Hosting.Python.CreateEngine();
            Scope = Engine.CreateScope();
            Source = Engine.CreateScriptSourceFromString(script);

            Scope.SetVariable("SE", mScriptFunctions);
            Source.Execute(Scope);

            MatchCollection matches = AutoCompPtn.Matches(script);

            foreach (Match m in matches)
            {
                string s = m.Groups[1].Value.TrimEnd('\r', '\n');
                mAutoCompleteList.Add(s);
            }
        }

        public void ExecuteCommandSync(string s)
        {
            s = s.Trim();
            ItConsole.println(AnsiEsc.White + s);

            if (s.StartsWith("@"))
            {
                SimpleCommand(s);
                return;
            }

            Exception e = RunScript(s);

            if (e != null)
            {
                ItConsole.println("error: " + e.Message);
            }

            Controller.Clear();
            Controller.DrawAll();
            Controller.PushCurrent();
        }

        public async void ExecuteCommandAsync(string s)
        {
            s = s.Trim();
            ItConsole.println(s);

            if (s.StartsWith("@"))
            {
                await Task.Run(() =>
                {
                    SimpleCommand(s);
                });

                return;
            }

            Exception e = null;

            await Task.Run( () =>
            {
                e = RunScript(s);
            });

            if (e != null)
            {
                ItConsole.println("error: " + e.Message);
            }

            Controller.Clear();
            Controller.DrawAll();
            Controller.PushCurrent();
        }

        public Exception RunScript(string s)
        {
            mScriptFunctions.StartSession();

            try
            {
                dynamic ret = Engine.Execute(s, Scope);

                if (ret != null)
                {
                    ItConsole.println(AnsiEsc.BGreen + ret.ToString());
                }
            }
            catch (Exception e)
            {
                mScriptFunctions.EndSession();
                return e;
            }

            mScriptFunctions.EndSession();
            return null;
        }

        public async void RunScriptAsync(string s, RunCallback callback)
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
                ItConsole.println("error: " + e.Message);
            }

            Controller.Clear();
            Controller.DrawAll();
            Controller.PushCurrent();

            if (callback != null)
            {
                callback.OnEnd();
            }
        }

        public class RunCallback
        {
            public Action OnStart = () => { };
            public Action OnEnd = () => { };
        }

        public void RunOnMainThread(Action action)
        {
            mThreadUtil.RunOnMainThread(action, true);
        }
    }
}
