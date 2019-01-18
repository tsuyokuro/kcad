using CadDataTypes;
using System.Threading;

namespace Plotter.Controller
{
    public class InteractCtrl
    {
        public enum Mode
        {
            NONE,
            POINT,
            LINE,
        }

        public enum States
        {
            NONE,
            CANCEL,
            CONTINUE,
            END,
        }

        private SemaphoreSlim Sem = new SemaphoreSlim(0, 1);

        public Mode CurrentMode = Mode.NONE;

        public VectorList PointList = new VectorList();

        public States mState = States.NONE;
        public States State
        {
            get => mState;
            set => mState = value;
        }

        public bool IsActive => (mState == States.CONTINUE);

        public void Cancel()
        {
            mState = States.CANCEL;
            Sem.Release();
        }

        public void SetPoint(CadVector v)
        {
            lock (PointList)
            {
                PointList.Add(v);
            }

            Sem.Release();
        }

        public void Start(Mode mode)
        {
            CurrentMode = mode;
            mState = States.CONTINUE;

            lock (PointList)
            {
                PointList.Clear();
            }
        }

        public void End()
        {
            mState = States.END;
            CurrentMode = Mode.NONE;
        }

        public States WaitPoint()
        {
            Sem.Wait();
            return mState;
        }

        public void Draw(DrawContext dc, CadVector tp)
        {
            if (PointList.Count == 0)
            {
                return;
            }

            CadVector p0 = PointList[0];
            CadVector p1;

            for (int i = 1; i < PointList.Count; i++)
            {
                p1 = PointList[i];

                dc.Drawing.DrawLine(DrawTools.PEN_DEFAULT_FIGURE, p0, p1);

                p0 = p1;
            }

            dc.Drawing.DrawLine(DrawTools.PEN_TEMP_FIGURE, p0, tp);
        }
    }
}
