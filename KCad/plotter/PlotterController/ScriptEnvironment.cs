﻿using MyScript;
using System;
using System.Collections.Generic;

namespace Plotter
{
    public class ScriptEnvironment
    {
        private PlotterController Controller;

        private Executor mScrExecutor;

        private enum Coord
        {
            XY,
            XZ,
            ZY,
        }


        public ScriptEnvironment(PlotterController controller)
        {
            Controller = controller;
            initScrExecutor();
        }


        // スクリプトエンジンが関数を呼び出す直前にこのメソッドが呼び出されます
        // stackに値をpushすると引数の追加が出来ます
        private void PreFuncCall(Evaluator evaluator, string funcName, Evaluator.ValueStack stack)
        {
        }

        private void initScrExecutor()
        {
            mScrExecutor = new Executor();
            mScrExecutor.evaluator.PreFuncCall = PreFuncCall;
            mScrExecutor.addFunction("rect", addRect);
            mScrExecutor.addFunction("rectSide", addRectSide);
            mScrExecutor.addFunction("rectTop", addRectTop);
            mScrExecutor.addFunction("distance", distance);
            mScrExecutor.addFunction("group", group);
            mScrExecutor.addFunction("ungroup", ungroup);
            mScrExecutor.addFunction("addLayer", addLayer);
            mScrExecutor.addFunction("revOrder", reverseOrder);
        }

        private int group(int argCount, Evaluator.ValueStack stack)
        {
            List<uint> idlist = Controller.GetSelectedFigIDList();

            if (idlist.Count < 2+1)
            {
                Controller.InteractOut.print("Please select two or more objects.");
                return 0;
            }

            CadFigure parent = Controller.DB.newFigure(CadFigure.Types.GROUP);

            foreach (uint id in idlist)
            {
                CadFigure fig = Controller.DB.getFigure(id);

                if (fig == null)
                {
                    continue;
                }

                parent.addChild(fig);
            }

            var ope = new CadOpeAddChildlen(parent, parent.ChildList);
            Controller.HistoryManager.foward(ope);

            Controller.InteractOut.print("Grouped");

            return 0;
        }

        private int ungroup(int argCount, Evaluator.ValueStack stack)
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

                fig.releaseAllChildlen();
            }

            Controller.HistoryManager.foward(opeList);

            Controller.InteractOut.print("Ungrouped");

            return 0;
        }

        private int distance(int argCount, Evaluator.ValueStack stack)
        {
            if (Controller.SelList.List.Count == 2)
            {
                CadPoint a = Controller.SelList.List[0].Point;
                CadPoint b = Controller.SelList.List[1].Point;

                CadPoint d = a - b;

                Controller.InteractOut.print("" + d.Norm() + "(mm)");
            }
            else
            {
                Controller.InteractOut.print("cmd dist error. After select 2 points");
            }

            return 0;
        }

        private int addRect(int argCount, Evaluator.ValueStack stack)
        {
            createRect(argCount, stack, Coord.XY);
            stack.push(0);
            return 1;
        }

        private int addRectSide(int argCount, Evaluator.ValueStack stack)
        {
            createRect(argCount, stack, Coord.ZY);
            stack.push(0);
            return 1;
        }

        private int addRectTop(int argCount, Evaluator.ValueStack stack)
        {
            createRect(argCount,stack, Coord.XZ);
            stack.push(0);
            return 1;
        }

        private void createRect(int argCount, Evaluator.ValueStack stack, Coord coord)
        {
            if (!(argCount == 2 || argCount == 4))
            {
                return;
            }

            CadPoint p0 = default(CadPoint);
            double w = 0;
            double h = 0;

            if (argCount == 2)
            {
                Evaluator.Value v2 = stack.pop();
                Evaluator.Value v1 = stack.pop();
                w = v1.getDouble();
                h = v2.getDouble();

                p0 = Controller.FreeDownPoint;
            }
            else if (argCount == 4)
            {
                Evaluator.Value vh = stack.pop();
                Evaluator.Value vw = stack.pop();
                Evaluator.Value y = stack.pop();
                Evaluator.Value x = stack.pop();
                p0 = CadPoint.Create(x.getDouble(), y.getDouble(), 0);

                w = vw.getDouble();
                h = vh.getDouble();
            }


            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.RECT);

            CadPoint p1 = p0;

            fig.addPoint(p0);
            if (coord == Coord.XY)
            {
                p1.x = p0.x + w;
                p1.y = p0.y;
                p1.z = p0.z;
                fig.addPoint(p1);

                p1.x = p0.x + w;
                p1.y = p0.y + h;
                p1.z = p0.z;
                fig.addPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y + h;
                p1.z = p0.z;
                fig.addPoint(p1);
            }
            else if (coord == Coord.XZ)
            {
                p1.x = p0.x + w;
                p1.y = p0.y;
                p1.z = p0.z;
                fig.addPoint(p1);

                p1.x = p0.x + w;
                p1.y = p0.y;
                p1.z = p0.z - h;
                fig.addPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y;
                p1.z = p0.z - h;
                fig.addPoint(p1);
            }
            else if (coord == Coord.ZY)
            {
                p1.x = p0.x;
                p1.y = p0.y;
                p1.z = p0.z - w;
                fig.addPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y + h;
                p1.z = p0.z - w;
                fig.addPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y + h;
                p1.z = p0.z;
                fig.addPoint(p1);
            }

            fig.Closed = true;

            fig.endCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.getAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.addFigure(fig);
        }

        private int addLayer(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            PlotterController controller = (PlotterController)(v0.getObj());
            argCount--;

            String name = null;

            if (argCount > 0)
            {
                Evaluator.Value namev = stack.pop();
                name = namev.getString();
            }

            Controller.AddLayer(name);

            return 0;
        }

        private int reverseOrder(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            PlotterController controller = (PlotterController)(v0.getObj());
            argCount--;

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

            return 0;
        }


        public void command(string s)
        {
            s = s.Trim();

            if (!s.EndsWith(";"))
            {
                s += ";";
            }

            mScrExecutor.eval(s);
        }
    }
}
