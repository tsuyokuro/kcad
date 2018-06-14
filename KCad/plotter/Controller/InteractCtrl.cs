using CadDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public enum State
        {
            NONE,
            CANCEL,
            CONTINUE,
        }

        private SemaphoreSlim Sem = new SemaphoreSlim(0, 1);

        public Mode CurrentMode = Mode.NONE;

        public VectorList PointList = new VectorList();

        public State mState = State.NONE;


        public void Cancel()
        {
            mState = State.CANCEL;
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
            mState = State.CONTINUE;

            lock (PointList)
            {
                PointList.Clear();
            }
        }

        public void End()
        {
            CurrentMode = Mode.NONE;
        }

        public State WaitPoint()
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
