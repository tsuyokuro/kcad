using MyScript;
using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class PlotterController
    {
        private Executor mScrExecutor;

        // スクリプトエンジンが関数を呼び出す直前にこのメソッドが呼び出されます
        private void PreFuncCall(Evaluator evaluator, string funcName, Evaluator.ValueStack stack)
        {
            // 第一引数(最初にPOPした値)にDrawContextを設定します
            stack.push(CurrentDC);
        }

        private void initScrExecutor()
        {
            mScrExecutor = new Executor();
            mScrExecutor.evaluator.PreFuncCall = PreFuncCall;
            mScrExecutor.addFunction("rect", addRect);
            mScrExecutor.addFunction("distance", distance);
            mScrExecutor.addFunction("group", group);
            mScrExecutor.addFunction("ungroup", ungroup);
            mScrExecutor.addFunction("addLayer", addLayer);
        }

        private int group(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            DrawContext dc = (DrawContext)(v0.getObj());
            argCount--;

            List<uint> idlist = getSelectedFigIDList();

            if (idlist.Count < 2+1)
            {
                InteractOut.print("Please select two or more objects.");
                return 0;
            }

            CadFigure parent = mDB.newFigure(CadFigure.Types.GROUP);

            foreach (uint id in idlist)
            {
                CadFigure fig = mDB.getFigure(id);

                if (fig == null)
                {
                    continue;
                }

                parent.addChild(fig);
            }

            var ope = new CadOpeAddChildlen(parent, parent.ChildList);
            mHistoryManager.foward(ope);

            InteractOut.print("Grouped");

            return 0;
        }

        private int ungroup(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            DrawContext dc = (DrawContext)(v0.getObj());
            argCount--;

            List<uint> idlist = getSelectedFigIDList();

            var idSet = new HashSet<uint>();

            foreach (uint id in idlist)
            {
                CadFigure fig = mDB.getFigure(id);

                CadFigure root = fig.getGroupRoot();

                if (root.ChildList.Count > 0)
                {
                    idSet.Add(root.ID);
                }
            }

            CadOpeList opeList = new CadOpeList();

            foreach (uint id in idSet)
            {
                CadFigure fig = mDB.getFigure(id);

                CadOpeRemoveChildlen ope = new CadOpeRemoveChildlen(fig, fig.ChildList);

                opeList.OpeList.Add(ope);

                fig.releaseAllChildlen();
            }

            mHistoryManager.foward(opeList);

            InteractOut.print("Ungrouped");

            return 0;
        }

        private int distance(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            DrawContext dc = (DrawContext)(v0.getObj());
            argCount--;

            if (mSelList.List.Count == 2)
            {
                CadPoint a = mSelList.List[0].Point;
                CadPoint b = mSelList.List[1].Point;

                CadPoint d = a - b;

                InteractOut.print("" + d.norm() + "(mm)");
            }
            else
            {
                InteractOut.print("cmd dist error. After select 2 points");
            }

            return 0;
        }

        private int addRect(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            DrawContext dc = (DrawContext)(v0.getObj());
            argCount--;

            if (argCount == 2)
            {
                Evaluator.Value v2 = stack.pop();
                Evaluator.Value v1 = stack.pop();

                double w = v1.getDouble();
                double h = v2.getDouble();

                CadFigure fig = mDB.newFigure(CadFigure.Types.POLY_LINES);

                CadPoint p0 = mFreeDownPoint;
                CadPoint p1 = p0;

                fig.addPoint(p0);

                p1.x = p0.x + w;
                fig.addPoint(p1);

                p1.y = p0.y + h;
                fig.addPoint(p1);

                p1 = p0;
                p1.y = p0.y + h;
                fig.addPoint(p1);

                fig.Closed = true;

                fig.endCreate(dc);

                CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, fig.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(fig);

                stack.push(0);
            }
            else if (argCount == 4)
            {
                Evaluator.Value vh = stack.pop();
                Evaluator.Value vw = stack.pop();
                Evaluator.Value y = stack.pop();
                Evaluator.Value x = stack.pop();

                CadFigure fig = mDB.newFigure(CadFigure.Types.RECT);

                CadPoint p0 = default(CadPoint);

                double w = vw.getDouble();
                double h = vh.getDouble();

                CadPoint p1 = p0;

                fig.addPoint(p0);

                p1.x = p0.x + w;
                fig.addPoint(p1);

                p1.y = p0.y + h;
                fig.addPoint(p1);

                p1 = p0;
                p1.y = p0.y + h;
                fig.addPoint(p1);

                fig.Closed = true;

                fig.endCreate(dc);

                CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, fig.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(fig);

                stack.push(0);
            }
            else
            {
                stack.push(1);
            }

            return 1;
        }

        private int addLayer(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.pop();
            DrawContext dc = (DrawContext)(v0.getObj());
            argCount--;

            String name = null;

            if (argCount > 0)
            {
                Evaluator.Value namev = stack.pop();
                name = namev.getString();
            }

            addLayer(name);

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
