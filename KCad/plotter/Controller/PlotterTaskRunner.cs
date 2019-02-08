using CadDataTypes;
using KCad;
using KCad.Dialogs;
using System;
using System.Collections.Generic;
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


        public async void FlipWithInteractive(List<CadFigure> rootFigList)
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

                if ((res.p1 - res.p0).IsZero())
                {
                    Controller.AbendEdit();
                    ItConsole.println("Error: Same point");
                    return;
                }

                CadVector normal = CadMath.Normal(
                    res.p1 - res.p0, (CadVector)(Controller.CurrentDC.ViewDir));

                FlipWithPlane(rootFigList, res.p0, normal);
                Controller.EndEdit();
            });
        }

        public void FlipWithPlane(List<CadFigure> rootFigList, CadVector p0, CadVector normal)
        {
            foreach (CadFigure fig in rootFigList)
            {
                fig.ForEachFig(f =>
                {
                    FlipWithPlane(f, p0, normal);
                });
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

        public async void FlipAndCopyWithInteractive(List<CadFigure> rootFigList)
        {
            await Task.Run(() =>
            {
                var res = InputLine();

                if (res.state != InteractCtrl.States.END)
                {
                    return;
                }

                if ((res.p1 - res.p0).IsZero())
                {
                    ItConsole.println("Error: Same point");
                    return;
                }

                CadVector normal = CadMath.Normal(
                    res.p1 - res.p0, (CadVector)(Controller.CurrentDC.ViewDir));

                FlipAndCopyWithPlane(rootFigList, res.p0, normal);
            });
        }

        public void FlipAndCopyWithPlane(List<CadFigure> rootFigList, CadVector p0, CadVector normal)
        {
            List<CadFigure> cpy = PlotterClipboard.CopyFigures(rootFigList);

            CadOpeList opeRoot = new CadOpeList();

            CadLayer layer = Controller.CurrentLayer;

            foreach (CadFigure fig in cpy)
            {
                fig.ForEachFig(d =>
                {
                    FlipWithPlane(d, p0, normal);
                    Controller.DB.AddFigure(d);
                });

                layer.AddFigure(fig);

                CadOpe ope = new CadOpeAddFigure(layer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            Controller.HistoryMan.foward(opeRoot);

            RunOnMainThread(() =>
            {
                Controller.UpdateTreeView(true);
            });
        }

        public async void RotateWithInteractive(List<CadFigure> rootFigList)
        {
            await Task.Run(() =>
            {
                Controller.StartEdit();
                var res = InputPoint();

                if (res.state != InteractCtrl.States.END)
                {
                    Controller.AbendEdit();
                    return;
                }

                CadVector p0 = res.p0;

                double angle = 0;

                bool ok = false;

                RunOnMainThread(() =>
                {
                    AngleInputDialog dlg = new AngleInputDialog();
                    bool? dlgRet = dlg.ShowDialog();

                    ok = dlgRet.Value;

                    if (ok)
                    {
                        angle = dlg.GetDouble();
                    }
                });

                if (!ok)
                {
                    ItConsole.println("Cancel!");

                    Controller.AbendEdit();
                    return;
                }

                RotateWithAxis(
                    rootFigList,
                    p0,
                    (CadVector)Controller.CurrentDC.ViewDir,
                    CadMath.Deg2Rad(angle));

                Controller.EndEdit();
            });
        }

        public void RotateWithAxis(List<CadFigure> rootFigList, CadVector org, CadVector axisDir, double angle)
        {
            foreach (CadFigure fig in rootFigList)
            {
                fig.ForEachFig(f =>
                {
                    CadUtil.RotateFigure(fig, org, axisDir, angle);
                });
            }
        }

        public (CadVector p0, InteractCtrl.States state) InputPoint()
        {
            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start();

            OpenPopupMessage("Input rotate origin", PlotterObserver.MessageType.INPUT);
            ItConsole.println(AnsiEsc.BYellow + "<< Input point >>");

            InteractCtrl.States ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ClosePopupMessage();
                ItConsole.println("Cancel!");
                return (
                    CadVector.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVector p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());
            ctrl.End();
            ClosePopupMessage();

            return (p0, ctrl.State);
        }


        public (CadVector p0, CadVector p1, InteractCtrl.States state) InputLine()
        {
            InteractCtrl ctrl = Controller.mInteractCtrl;

            ctrl.Start();

            OpenPopupMessage("Input flip axis", PlotterObserver.MessageType.INPUT);
            ItConsole.println(AnsiEsc.BYellow + "<< Input point 1 >>");

            InteractCtrl.States ret;

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ClosePopupMessage();
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
                ClosePopupMessage();
                ItConsole.println("Cancel!");
                return (
                    CadVector.InvalidValue,
                    CadVector.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVector p1 = ctrl.PointList[1];
            ItConsole.println(p1.CoordString());

            ctrl.End();
            ClosePopupMessage();

            return (p0, p1, InteractCtrl.States.END);
        }

        public void OpenPopupMessage(string text, PlotterObserver.MessageType type)
        {
            Controller.Observer.OpenPopupMessage(text, type);
        }

        public void ClosePopupMessage()
        {
            Controller.Observer.ClosePopupMessage();
        }

        public void RunOnMainThread(Action action)
        {
            if (mMainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                action();
                return;
            }

            Task task = new Task(() =>
            {
                action();
            }
            );

            task.Start(mMainThreadScheduler);
            task.Wait();
        }
    }
}
