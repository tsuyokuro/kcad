using MyScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public partial class PlotterController
    {
        private Executor mScrExecutor;

        private void initScrExecutor()
        {
            mScrExecutor = new Executor();
            mScrExecutor.addFunction("rect", addRect);
            mScrExecutor.addFunction("distance", distance);
            mScrExecutor.addFunction("group", group);
            mScrExecutor.addFunction("ungroup", ungroup);
            mScrExecutor.addFunction("addLayer", addLayer);
        }

        private int group(int argCount, Evaluator.ValueStack stack)
        {
            List<uint> idlist = getSelectedFigIDList();

            if (idlist.Count < 2)
            {
                Interact.print("Please select two or more objects.");
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

            Interact.print("Grouped");

            return 0;
        }

        private int ungroup(int argCount, Evaluator.ValueStack stack)
        {
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

            Interact.print("Ungrouped");

            return 0;
        }

        private int distance(int argCount, Evaluator.ValueStack stack)
        {
            int i;
            for (i = 0; i < argCount; i++)
            {
                Evaluator.Value v = stack.pop();
            }

            if (mSelList.List.Count == 2)
            {
                CadPoint a = mSelList.List[0].Point;
                CadPoint b = mSelList.List[1].Point;

                CadPoint d = a - b;

                Interact.print("" + d.length() + "(mm)");
            }
            else
            {
                Interact.print("cmd dist error. After select 2 points");
            }

            return 0;
        }

        private int addRect(int argCount, Evaluator.ValueStack stack)
        {
            if (argCount == 2)
            {
                Evaluator.Value v2 = stack.pop();
                Evaluator.Value v1 = stack.pop();

                double w = v1.getDouble();
                double h = v2.getDouble();

                CadFigure fig = mDB.newFigure(CadFigure.Types.RECT);

                if (mFreeDownPoint != null)
                {
                    CadPoint p0 = mFreeDownPoint.Value;
                    CadPoint p1 = p0;

                    p1.x += w;
                    p1.y += h;

                    fig.addPoint(p0);
                    fig.addPoint(p1);
                    fig.endCreate();

                    CadOpe ope = CadOpe.getAddFigureOpe(CurrentLayer.ID, fig.ID);
                    mHistoryManager.foward(ope);
                    CurrentLayer.addFigure(fig);

                    stack.push(0);
                }
                else
                {
                    stack.push(1);
                }
            }
            else if (argCount == 4)
            {
                Evaluator.Value h = stack.pop();
                Evaluator.Value w = stack.pop();
                Evaluator.Value y = stack.pop();
                Evaluator.Value x = stack.pop();

                CadFigure fig = mDB.newFigure(CadFigure.Types.RECT);

                CadPoint p0 = default(CadPoint);
                p0.x = x.getDouble();
                p0.y = y.getDouble();

                CadPoint p1 = p0;

                p1.x += w.getDouble();
                p1.y += h.getDouble();

                fig.addPoint(p0);
                fig.addPoint(p1);

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
            CadLayer layer = mDB.newLayer();


            if (argCount > 0)
            {
                Evaluator.Value name = stack.pop();
                layer.Name = name.getString();
            }
            else
            {
                layer.Name = null;
            }

            CurrentLayer = layer;

            mDB.LayerList.Add(layer);

            NotifyLayerInfo();

            Interact.print("Layer added.  Name:" + layer.Name + " ID:" + layer.ID);

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
