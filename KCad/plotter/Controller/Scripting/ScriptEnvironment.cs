using KCad.Properties;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KCad.Controls;
using OpenTK;

namespace Plotter.Controller
{
    public partial class ScriptEnvironment
    {
        public PlotterController Controller;

        private ScriptEngine Engine;

        private ScriptScope mScope;
        public ScriptScope Scope
        {
            get => mScope;
        }

        private ScriptSource Source;

        private List<string> mAutoCompleteList = new List<string>();
        public List<string> AutoCompleteList
        {
            get => mAutoCompleteList;
        }

        private ScriptFunctions mScriptFunctions;

        private SipmleCommands mSimpleCommands;

        private TestCommands mTestCommands;


        public ScriptEnvironment(PlotterController controller)
        {
            Controller = controller;

            mScriptFunctions = new ScriptFunctions();

            mSimpleCommands = new SipmleCommands(controller);

            mTestCommands = new TestCommands(controller);

            InitScriptingEngine();

            mScriptFunctions.Init(this, mScope);
        }

        Regex AutoCompPtn = new Regex(@"#\[AC\][ \t]*(.+)\n");

        private void InitScriptingEngine()
        {
            string script = System.Text.Encoding.GetEncoding("Shift_JIS").GetString(Resources.BaseScript);

            //string script = "";

            Engine = IronPython.Hosting.Python.CreateEngine();
            mScope = Engine.CreateScope();
            Source = Engine.CreateScriptSourceFromString(script);

            mScope.SetVariable("SE", mScriptFunctions);
            Source.Execute(mScope);

            MatchCollection matches = AutoCompPtn.Matches(script);

            foreach (Match m in matches)
            {
                string s = m.Groups[1].Value.TrimEnd('\r', '\n');
                mAutoCompleteList.Add(s);
            }

            mAutoCompleteList.AddRange(mSimpleCommands.GetAutoCompleteForSimpleCmd());
        }

        public async void ExecuteCommandAsync(string s)
        {
            s = s.Trim();
            ItConsole.println(s);

            if (s.StartsWith("@"))
            {
                await Task.Run(() =>
                {
                    if (!mSimpleCommands.ExecCommand(s))
                    {
                        mTestCommands.ExecCommand(s);
                    }
                });

                return;
            }

            await Task.Run( () =>
            {
                RunScript(s);
            });

            Controller.Clear();
            Controller.DrawAll();
            Controller.ReflectToView();
        }

        public dynamic RunScript(string s)
        {
            mScriptFunctions.StartSession();

            dynamic ret = null;

            try
            {
                ret = Engine.Execute(s, mScope);

                if (ret != null)
                {
                    if (ret is Double || ret is Int32)
                    {
                        ItConsole.println(AnsiEsc.BGreen + ret.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                mScriptFunctions.EndSession();
                ItConsole.println(AnsiEsc.BRed + "Error: " + e.Message);
            }

            mScriptFunctions.EndSession();

            return ret;
        }

        public async void RunScriptAsync(string s, RunCallback callback)
        {
            if (callback != null)
            {
                callback.OnStart();
            }

            await Task.Run(() =>
            {
                RunScript(s);
            });

            Controller.Clear();
            Controller.DrawAll();
            Controller.ReflectToView();

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
            ThreadUtil.RunOnMainThread(action, true);
        }

        public void OpenPopupMessage(string text, PlotterCallback.MessageType type)
        {
            Controller.Callback.OpenPopupMessage(text, type);
        }

        public void ClosePopupMessage()
        {
            Controller.Callback.ClosePopupMessage();
        }
    }
}
