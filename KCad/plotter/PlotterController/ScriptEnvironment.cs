﻿using MyScript;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Plotter
{
    public class ScriptEnvironment
    {
        private PlotterController Controller;

        private Executor mScrExecutor;

        private List<string> mAutoCompleteList;

        public List<string> AutoCompleteList
        {
            get
            {
                return mAutoCompleteList;
            }
        }

        private enum Coord
        {
            XY,
            XZ,
            ZY,
        }


        public ScriptEnvironment(PlotterController controller)
        {
            Controller = controller;

            InitAutoCompleteList();
            InitScrExecutor();
        }

        // スクリプトエンジンが関数を呼び出す直前にこのメソッドが呼び出されます
        // stackに値をpushすると引数の追加が出来ます
        private void PreFuncCall(Evaluator evaluator, string funcName, Evaluator.ValueStack stack)
        {
        }

        private void InitScrExecutor()
        {
            // 引数の数とstackを受け取る
            // int func(int argCount, Evaluator.ValueStack stack)
            // 戻り値は、stackにpushした値の数

            mScrExecutor = new Executor();
            mScrExecutor.evaluator.PreFuncCall = PreFuncCall;
            mScrExecutor.AddFunction("rect", AddRect);
            mScrExecutor.AddFunction("rectSide", AddRectSide);
            mScrExecutor.AddFunction("rectTop", AddRectTop);
            mScrExecutor.AddFunction("point", AddPoint);
            mScrExecutor.AddFunction("distance", Distance);
            mScrExecutor.AddFunction("group", Group);
            mScrExecutor.AddFunction("ungroup", Ungroup);
            mScrExecutor.AddFunction("addLayer", AddLayer);
            mScrExecutor.AddFunction("revOrder", ReverseOrder);
            mScrExecutor.AddFunction("tapltest", tapltest);
            mScrExecutor.AddFunction("move", Move);
            mScrExecutor.AddFunction("length", SegLen);
            mScrExecutor.AddFunction("insPoint", InsPoint);
            mScrExecutor.AddFunction("area", Area);
            mScrExecutor.AddFunction("cursor1", ShowLastDownPoint);
            mScrExecutor.AddFunction("scale", Scale);
            mScrExecutor.AddFunction("find", Find);
        }

        private void InitAutoCompleteList()
        {
            mAutoCompleteList = new List<string>()
            {
                "rect(10,10)",
                "distance",
                "revOrder",
                "group",
                "ungroup",
                "addLayer",
                "move(0,0,0)",
                "length()",
                "insPoint()",
                "cursor1()",
                "scale(0.5)",
                "find(4)",
            };
        }

        private int Find(int argCount, Evaluator.ValueStack stack)
        {
            CadPoint org = Controller.LastDownPoint;

            double range = 8;

            if (argCount == 1)
            {
                Evaluator.Value v = stack.Pop();
                range = v.GetDouble();
            }

            DrawContext dc = Controller.CurrentDC;

            CadPoint pixp = dc.CadPointToUnitPoint(org);


            PointSearcher searcher = new PointSearcher();

            searcher.SetRangePixel(dc, range);
            searcher.SearchAllLayer(dc, pixp, Controller.DB);


            List<MarkPoint> list = searcher.GetXYMatches();


            foreach (MarkPoint mp in list)
            {
                string s = "fig{id:" + mp.FigureID.ToString() + ";idx:" + mp.PointIndex.ToString() + ";}";
                Controller.InteractOut.print(s);
            }

            return 0;
        }


        Regex FigPtn = new Regex(@"fig[ ]*{[ ]*id\:[ ]*([0-9]+)[ ]*;[ ]*idx\:[ ]*([0-9]+)[ ]*;[ ]*}[ ]*");

        public void MessageSelected(List<string> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            string s = messages[messages.Count - 1];

            Match match = FigPtn.Match(s);

            if (match.Success && match.Groups.Count==3)
            {
                string sId = match.Groups[1].Value;
                string sIdx = match.Groups[2].Value;

                uint id = UInt32.Parse(sId);
                int idx = Int32.Parse(sIdx);

                if (Controller.SelectMode == PlotterController.SelectModes.POINT)
                {
                    Controller.SelectById(id, idx);
                }
                else
                {
                    Controller.SelectById(id, -1);
                }
            }
        }

        private int Scale(int argCount, Evaluator.ValueStack stack)
        {
            CadPoint org = Controller.LastDownPoint;

            Evaluator.Value v = stack.Pop();

            double scale = v.GetDouble();

            Controller.Scale(org, scale);

            return 0;
        }

        private int ShowLastDownPoint(int argCount, Evaluator.ValueStack stack)
        {
            stack.Push(Controller.LastDownPoint.x);
            stack.Push(Controller.LastDownPoint.y);
            stack.Push(Controller.LastDownPoint.z);

            return 3;
        }

        private int Group(int argCount, Evaluator.ValueStack stack)
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

                parent.AddChild(fig);
            }

            var ope = new CadOpeAddChildlen(parent, parent.ChildList);
            Controller.HistoryManager.foward(ope);

            Controller.InteractOut.print("Grouped");

            return 0;
        }

        private int Ungroup(int argCount, Evaluator.ValueStack stack)
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

                fig.ReleaseAllChildlen();
            }

            Controller.HistoryManager.foward(opeList);

            Controller.InteractOut.print("Ungrouped");

            return 0;
        }

        private int Distance(int argCount, Evaluator.ValueStack stack)
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

        private int AddPoint(int argCount, Evaluator.ValueStack stack)
        {
            if (!(argCount == 0 || argCount == 2 || argCount == 3))
            {
                return 0;
            }


            CadPoint p = default(CadPoint);

            if (argCount == 0)
            {
                p = Controller.LastDownPoint;
            }
            else if (argCount == 2)
            {
                Evaluator.Value y = stack.Pop();
                Evaluator.Value x = stack.Pop();
                p.set(x.GetDouble(), y.GetDouble(), 0);
            }
            else if (argCount == 3)
            {
                Evaluator.Value z = stack.Pop();
                Evaluator.Value y = stack.Pop();
                Evaluator.Value x = stack.Pop();
                p.set(x.GetDouble(), y.GetDouble(), z.GetDouble());
            }

            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.POINT);
            fig.AddPoint(p);

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.addFigure(fig);

            return 0;
        }

        private int AddRect(int argCount, Evaluator.ValueStack stack)
        {
            CreateRect(argCount, stack, Coord.XY);
            return 0;
        }

        private int AddRectSide(int argCount, Evaluator.ValueStack stack)
        {
            CreateRect(argCount, stack, Coord.ZY);
            return 0;
        }

        private int AddRectTop(int argCount, Evaluator.ValueStack stack)
        {
            CreateRect(argCount,stack, Coord.XZ);
            return 0;
        }

        private void CreateRect(int argCount, Evaluator.ValueStack stack, Coord coord)
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
                Evaluator.Value v2 = stack.Pop();
                Evaluator.Value v1 = stack.Pop();
                w = v1.GetDouble();
                h = v2.GetDouble();

                p0 = Controller.LastDownPoint;
            }
            else if (argCount == 4)
            {
                Evaluator.Value vh = stack.Pop();
                Evaluator.Value vw = stack.Pop();
                Evaluator.Value y = stack.Pop();
                Evaluator.Value x = stack.Pop();
                p0 = CadPoint.Create(x.GetDouble(), y.GetDouble(), 0);

                w = vw.GetDouble();
                h = vh.GetDouble();
            }


            CadFigure fig = Controller.DB.newFigure(CadFigure.Types.RECT);

            CadPoint p1 = p0;

            fig.AddPoint(p0);
            if (coord == Coord.XY)
            {
                p1.x = p0.x + w;
                p1.y = p0.y;
                p1.z = p0.z;
                fig.AddPoint(p1);

                p1.x = p0.x + w;
                p1.y = p0.y + h;
                p1.z = p0.z;
                fig.AddPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y + h;
                p1.z = p0.z;
                fig.AddPoint(p1);
            }
            else if (coord == Coord.XZ)
            {
                p1.x = p0.x + w;
                p1.y = p0.y;
                p1.z = p0.z;
                fig.AddPoint(p1);

                p1.x = p0.x + w;
                p1.y = p0.y;
                p1.z = p0.z - h;
                fig.AddPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y;
                p1.z = p0.z - h;
                fig.AddPoint(p1);
            }
            else if (coord == Coord.ZY)
            {
                p1.x = p0.x;
                p1.y = p0.y;
                p1.z = p0.z - w;
                fig.AddPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y + h;
                p1.z = p0.z - w;
                fig.AddPoint(p1);

                p1.x = p0.x;
                p1.y = p0.y + h;
                p1.z = p0.z;
                fig.AddPoint(p1);
            }

            fig.Closed = true;

            fig.EndCreate(Controller.CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(Controller.CurrentLayer.ID, fig.ID);
            Controller.HistoryManager.foward(ope);
            Controller.CurrentLayer.addFigure(fig);
        }

        private int AddLayer(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.Pop();
            PlotterController controller = (PlotterController)(v0.GetObj());
            argCount--;

            String name = null;

            if (argCount > 0)
            {
                Evaluator.Value namev = stack.Pop();
                name = namev.GetString();
            }

            Controller.AddLayer(name);

            return 0;
        }

        private int ReverseOrder(int argCount, Evaluator.ValueStack stack)
        {
            Evaluator.Value v0 = stack.Pop();
            PlotterController controller = (PlotterController)(v0.GetObj());
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

        private int tapltest(int argCount, Evaluator.ValueStack stack)
        {
            if (argCount != 2)
            {
                return 0;
            }
            Evaluator.Value v1 = stack.Pop();
            Evaluator.Value v0 = stack.Pop();

            stack.Push(v0);
            stack.Push(v1);

            return 2;
        }

        private int Move(int argCount, Evaluator.ValueStack stack)
        {
            if (argCount != 3)
            {
                Controller.InteractOut.print("Invalid argument.");
                Controller.InteractOut.print("move(x, y, z)");

                return 0;
            }
            Evaluator.Value v2 = stack.Pop();
            Evaluator.Value v1 = stack.Pop();
            Evaluator.Value v0 = stack.Pop();

            double x = v0.GetDouble();
            double y = v1.GetDouble();
            double z = v2.GetDouble();

            CadPoint delta = CadPoint.Create(x, y, z);

            Controller.MoveSelectedPoints(delta);

            return 0;
        }

        private int SegLen(int argCount, Evaluator.ValueStack stack)
        {
            if (argCount != 1)
            {
                Controller.InteractOut.print("Invalid argument.");
                Controller.InteractOut.print("len( LineLength )");

                return 0;
            }

            Evaluator.Value v0 = stack.Pop();

            double len = v0.GetDouble();

            MarkSeg seg = Controller.SelSegList.LastSel;

            if (seg.FigureID == 0)
            {
                return 0;
            }

            CadFigure fig = Controller.DB.getFigure(seg.FigureID);

            CadPoint pa = fig.GetPointAt(seg.PtIndexA);
            CadPoint pb = fig.GetPointAt(seg.PtIndexB);

            CadPoint v;

            v = pa - Controller.LastDownPoint;
            double da = v.Norm();

            v = pb - Controller.LastDownPoint;
            double db = v.Norm();


            if (da < db)
            {
                CadPoint np = CadUtil.LinePoint(pb, pa, len);
                Controller.StartEdit();

                fig.SetPointAt(seg.PtIndexA, np);

                Controller.EndEdit();
            }
            else
            {
                CadPoint np = CadUtil.LinePoint(pa, pb, len);
                Controller.StartEdit();

                fig.SetPointAt(seg.PtIndexB, np);

                Controller.EndEdit();
            }

            return 0;
        }

        private int InsPoint(int argCount, Evaluator.ValueStack stack)
        {
            if (!Controller.insPointToLastSelectedSeg())
            {
                Controller.InteractOut.print("Can not inser point.");
            }

            return 0;
        }

        private int Area(int argCount, Evaluator.ValueStack stack)
        {
            double area = Controller.Area();

            Controller.InteractOut.print("Area:" + (area / 100).ToString() + "(㎠)");
            return 0;
        }

        public void command(string s)
        {
            s = s.Trim();
            Controller.InteractOut.print("> " + s);

            if (!s.EndsWith(";"))
            {
                s += ";";
            }

            Executor.Error error = mScrExecutor.Eval(s);

            if (error == Executor.Error.NO_ERROR)
            {
                List<Evaluator.Value> vlist = mScrExecutor.GetOutput();

                foreach (Evaluator.Value value in vlist)
                {
                    Controller.InteractOut.print(value.GetString());
                }
            }
        }
    }
}
