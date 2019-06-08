using CadDataTypes;
using KCad;
using KCad.Dialogs;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plotter.Controller.TaskRunner
{
    public class PlotterTaskRunner
    {
        public PlotterController Controller;

        public PlotterTaskRunner(PlotterController controller)
        {
            Controller = controller;
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

                CadVertex normal = CadMath.Normal(
                    res.p1 - res.p0, (CadVertex)(Controller.CurrentDC.ViewDir));

                FlipWithPlane(rootFigList, res.p0, normal);
                Controller.EndEdit();
                Controller.Redraw();
            });
        }

        public void FlipWithPlane(List<CadFigure> rootFigList, CadVertex p0, CadVertex normal)
        {
            foreach (CadFigure fig in rootFigList)
            {
                fig.ForEachFig(f =>
                {
                    FlipWithPlane(f, p0, normal);
                });
            }
        }

        public void FlipWithPlane(CadFigure fig, CadVertex p0, CadVertex normal)
        {
            VertexList vl = fig.PointList;

            for (int i = 0; i < vl.Count; i++)
            {
                CadVertex v = vl[i];

                CadVertex cp = CadUtil.CrossPlane(v, p0, normal);

                CadVertex d = v - cp;

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

                CadVertex normal = CadMath.Normal(
                    res.p1 - res.p0, (CadVertex)(Controller.CurrentDC.ViewDir));

                FlipAndCopyWithPlane(rootFigList, res.p0, normal);
            });
        }

        public void FlipAndCopyWithPlane(List<CadFigure> rootFigList, CadVertex p0, CadVertex normal)
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
                Controller.Redraw();
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

                CadVertex p0 = res.p0;

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
                    p0.vector,
                    Controller.CurrentDC.ViewDir,
                    CadMath.Deg2Rad(angle));

                Controller.EndEdit();
                Controller.Redraw();
            });
        }

        public void RotateWithAxis(List<CadFigure> rootFigList, Vector3d org, Vector3d axisDir, double angle)
        {
            foreach (CadFigure fig in rootFigList)
            {
                fig.ForEachFig(f =>
                {
                    CadUtil.RotateFigure(fig, org, axisDir, angle);
                });
            }
        }

        public (CadVertex p0, InteractCtrl.States state) InputPoint()
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
                    CadVertex.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVertex p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());
            ctrl.End();
            ClosePopupMessage();

            return (p0, ctrl.State);
        }


        public (CadVertex p0, CadVertex p1, InteractCtrl.States state) InputLine()
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
                    CadVertex.InvalidValue,
                    CadVertex.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVertex p0 = ctrl.PointList[0];
            ItConsole.println(p0.CoordString());

            ItConsole.println(AnsiEsc.BYellow + "<< Input point 2 >>");

            ret = ctrl.WaitPoint();

            if (ret != InteractCtrl.States.CONTINUE)
            {
                ctrl.End();
                ClosePopupMessage();
                ItConsole.println("Cancel!");
                return (
                    CadVertex.InvalidValue,
                    CadVertex.InvalidValue,
                    InteractCtrl.States.CANCEL);
            }

            CadVertex p1 = ctrl.PointList[1];
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

        private void RunOnMainThread(Action action)
        {
            ThreadUtil.RunOnMainThread(action, true);
        }
    }
}
