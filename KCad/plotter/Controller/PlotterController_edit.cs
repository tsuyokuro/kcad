﻿using System;
using System.Collections.Generic;
using CadDataTypes;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public void ToBezier()
        {
            if (LastSelSegment == null)
            {
                return;
            }

            ToBezier(LastSelSegment.Value);
            ClearSelection();
        }

        public void ToBezier(MarkSegment seg)
        {
            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = mDB.GetFigure(seg.FigureID);

            int num = CadUtil.InitBezier(fig, seg.PtIndexA, seg.PtIndexB);

            if (num > 0)
            {
                CadOpe ope = CadOpe.CreateInsertPointsOpe(
                    fig.LayerID, fig.ID, seg.PtIndexA + 1, num);

                HistoryMan.foward(ope);
            }

            ClearSelection();
        }

        public void SeparateFigures()
        {
            if (LastSelPoint == null)
            {
                return;
            }

            SeparateFigures(LastSelPoint.Value.Figure, LastSelPoint.Value.PointIndex);
            ClearSelection();
        }

        public void SeparateFigures(CadFigure fig, int pointIdx)
        {
            var res = CadFigureCutter.Cut(mDB, fig, pointIdx);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (EditResult.Item ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (EditResult.Item ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            HistoryMan.foward(opeRoot);
        }

        public void BondFigures()
        {
            BondFigures(CurrentFigure);
            ClearSelection();
        }

        public void BondFigures(CadFigure fig)
        {
            var res = CadFigureBonder.Bond(mDB, fig);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (EditResult.Item ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (EditResult.Item ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            HistoryMan.foward(opeRoot);
        }

        public void CutSegment()
        {
            if (LastSelSegment == null)
            {
                return;
            }

            MarkSegment ms = LastSelSegment.Value;
            CutSegment(ms);
            ClearSelection();
        }

        public void CutSegment(MarkSegment ms)
        {
            if (!ms.Valid)
            {
                return;
            }

            if (!ms.CrossPoint.Valid)
            {
                return;
            }

            var res = CadSegmentCutter.CutSegment(mDB, ms, ms.CrossPoint);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (EditResult.Item ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (EditResult.Item ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            HistoryMan.foward(opeRoot);
        }

        public void SetLoop(bool isLoop)
        {
            List<uint> list = DB.GetSelectedFigIDList();

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (uint id in list)
            {
                CadFigure fig = DB.GetFigure(id);

                if (fig.Type != CadFigure.Types.POLY_LINES)
                {
                    continue;
                }

                if (fig.IsLoop != isLoop)
                {
                    fig.IsLoop = isLoop;

                    if (isLoop)
                    {
                        fig.Normal = CadUtil.Normal(fig);
                    }

                    ope = CadOpe.CreateSetCloseOpe(CurrentLayer.ID, id, isLoop);
                    opeRoot.OpeList.Add(ope);
                }
            }

            HistoryMan.foward(opeRoot);
        }

        public void Flip(TargetCoord coord)
        {
            CadVector cp = PlotterUtil.GetSelectionCenter(this);

            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    int num = fig.PointList.Count;
                    int selnum = 0;

                    for (int i = 0; i < num; i++)
                    {
                        CadVector p = fig.PointList[i];

                        if (p.Selected)
                        {
                            selnum++;

                            CadVector np = p;
                            if ((coord & TargetCoord.X) != 0)
                            {
                                np.x -= cp.x;
                                np.x = -np.x + cp.x;
                            }

                            if ((coord & TargetCoord.Y) != 0)
                            {
                                np.y -= cp.y;
                                np.y = -np.y + cp.y;
                            }

                            if ((coord & TargetCoord.Z) != 0)
                            {
                                np.z -= cp.z;
                                np.z = -np.z + cp.z;
                            }

                            fig.SetPointAt(i, np);
                        }
                    }

                    /*
                    if (selnum == num)
                    {
                        fig.PointList.Reverse();
                    }
                    */
                }
            }
        }

        public void FlipWithVector()
        {
            mPlotterTaskRunner.FlipWithInteractive(GetSelectedFigureList());
        }

        private void RemoveSelectedPoints()
        {
            List<CadFigure> figList = DB.GetSelectedFigList();
            foreach (CadFigure fig in figList)
            {
                fig.RemoveSelected();
            }

            foreach (CadFigure fig in figList)
            {
                fig.RemoveGarbageChildren();
            }

            UpdateTreeView(true);
        }

        public void FlipNormal()
        {
            List<uint> ids = DB.GetSelectedFigIDList();

            CadOpeList opeList = CadOpe.CreateListOpe();

            foreach (uint id in ids)
            {
                CadFigure fig = mDB.GetFigure(id);
                CadVector old = fig.Normal;

                fig.Normal *= -1;

                CadOpe ope = CadOpe.CreateChangeNormalOpe(id, old, fig.Normal);
                opeList.Add(ope);
            }


            HistoryMan.foward(opeList);
        }

        public bool InsPointToLastSelectedSeg()
        {
            if (LastSelSegment == null)
            {
                return false;
            }

            MarkSegment seg = LastSelSegment.Value;

            CadFigure fig = DB.GetFigure(seg.FigureID);

            if (fig == null)
            {
                return false;
            }

            if (fig.Type != CadFigure.Types.POLY_LINES)
            {
                return false;
            }

            int ins = 0;

            bool handle = false;

            handle |= fig.GetPointAt(seg.PtIndexA).IsHandle;
            handle |= fig.GetPointAt(seg.PtIndexB).IsHandle;

            if (handle)
            {
                return false;
            }

            if (seg.PtIndexA < seg.PtIndexB)
            {
                ins = seg.PtIndexB;
            }
            else
            {
                ins = seg.PtIndexA;
            }

            fig.InsertPointAt(ins, LastDownPoint);

            return true;
        }

        public void AddCentroid()
        {
            Centroid cent = PlotterUtil.Centroid(this);

            if (cent.IsInvalid)
            {
                return;
            }

            CadFigure pointFig = mDB.NewFigure(CadFigure.Types.POINT);
            pointFig.AddPoint(cent.Point);

            pointFig.EndCreate(CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, pointFig.ID);
            HistoryMan.foward(ope);
            CurrentLayer.AddFigure(pointFig);

            string s = string.Format("({0:0.000},{1:0.000},{2:0.000})",
                               cent.Point.x, cent.Point.y, cent.Point.z);

            ItConsole.println("Centroid:" + s);
            ItConsole.println("Area:" + (cent.Area / 100).ToString() + "(㎠)");
        }
    }
}
