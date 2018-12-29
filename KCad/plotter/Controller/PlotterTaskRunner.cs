using CadDataTypes;
using KCad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter.Controller.TaskRunner
{
    public class PlotterTaskRunner
    {
        public PlotterController Controller;

        public TaskScheduler mMainThreadScheduler;

        public int mMainThreadID = -1;

        public PlotterTaskRunner(PlotterController controller)
        {
            Controller = controller;

            mMainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            mMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public (CadVector p0, CadVector p1, InteractCtrl.States state) InputLine()
        {
            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start(InteractCtrl.Mode.LINE);

            ItConsole.println(AnsiEsc.BYellow + "<< Input point 1 >>");

            InteractCtrl.States ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return (
                    CadVector.InvalidValue,
                    CadVector.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVector p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());

            ItConsole.println(AnsiEsc.BYellow + "<< Input point 2 >>");

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ItConsole.println("Cancel!");
                return (
                    CadVector.InvalidValue,
                    CadVector.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVector p1 = ctrl.PointList[1];
            ItConsole.println(p1.CoordString());

            ctrl.End();

            return (p0, p1, InteractCtrl.States.END);
        }

        public async void FlipWithInteractive(List<CadFigure> figList)
        {
            await Task.Run(() =>
            {
                Controller.StartEdit();
                var res = InputLine();

                if (res.state != InteractCtrl.States.END)
                {
                    Controller.AbendEdit();
                    return;
                }

                CadVector normal = CadMath.Normal(
                    res.p1 - res.p0, (CadVector)(Controller.CurrentDC.ViewDir));

                FlipWithPlane(figList, res.p0, normal);
                Controller.EndEdit();
            });
        }

        public void FlipWithPlane(List<CadFigure> figList, CadVector p0, CadVector normal)
        {
            foreach (CadFigure fig in figList)
            {
                FlipWithPlane(fig, p0, normal);
            }
        }

        public void FlipWithPlane(CadFigure fig, CadVector p0, CadVector normal)
        {
            VectorList vl = fig.PointList;

            for (int i = 0; i < vl.Count; i++)
            {
                CadVector v = vl[i];

                CadVector cp = CadUtil.CrossPlane(v, p0, normal);

                CadVector d = v - cp;

                v = cp - d;

                vl[i] = v;
            }
        }
    }
}
