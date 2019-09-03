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
                Env.Controller.PushDraw();
            });
        }
    }
}