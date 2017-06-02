using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class PlotterController
    {
        public void Remove(DrawContext dc)
        {
            StartEdit();

            RemoveSelectedPoints();

            EndEdit();

            Clear(dc);
            Draw(dc);
        }

        public void ToBezier(DrawContext dc)
        {
            ToBezier(dc, mSelectedSegs.LastSel);
            ClearSelection();
            Draw(dc);
        }

        public void ToBezier(DrawContext dc, MarkSeg seg)
        {
            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = mDB.getFigure(seg.FigureID);

            int num = CadPointUtil.initBezier(fig, seg.PtIndexA, seg.PtIndexB);

            if (num > 0)
            {
                CadOpe ope = CadOpe.CreateInsertPointsOpe(
                    fig.LayerID, fig.ID, seg.PtIndexA + 1, num);

                mHistoryManager.foward(ope);
            }

            ClearSelection();

            Draw(dc);
        }

        public void SeparateFigures(DrawContext dc)
        {
            SeparateFigures(mSelList.List);
            ClearSelection();
            Draw(dc);
        }

        public void SeparateFigures(List<SelectItem> selList)
        {
            CadFigureCutter fa = new CadFigureCutter(mDB);

            var res = fa.cut(selList);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.addFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.removeFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void BondFigures(DrawContext dc)
        {
            BondFigures(mSelList.List);
            ClearSelection();
            Draw(dc);
        }

        public void BondFigures(List<SelectItem> selList)
        {
            CadFigureBonder fa = new CadFigureBonder(mDB, CurrentLayer);

            var res = fa.bond(selList);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.addFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.removeFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void CutSegment(DrawContext dc)
        {
            MarkSeg ms = mSelectedSegs.LastSel;
            CutSegment(ms);
            ClearSelection();

            Draw(dc);
        }

        public void CutSegment(MarkSeg ms)
        {
            if (!ms.Valid)
            {
                return;
            }

            if (mObjDownPoint == null)
            {
                return;
            }

            CadSegmentCutter segCutter = new CadSegmentCutter(mDB);

            var res = segCutter.cutSegment(ms, mObjDownPoint.Value);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.addFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.removeFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        private CadPoint GetSelectionCenter()
        {
            CadPoint min = CadPoint.Create(CadConst.MaxValue);
            CadPoint max = CadPoint.Create(CadConst.MinValue);

            int selPointCnt = 0;

            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    foreach (CadPoint p in fig.PointList)
                    {
                        if (p.Selected)
                        {
                            selPointCnt++;

                            min.x = Math.Min(p.x, min.x);
                            min.y = Math.Min(p.y, min.y);
                            min.z = Math.Min(p.z, min.z);

                            max.x = Math.Max(p.x, max.x);
                            max.y = Math.Max(p.y, max.y);
                            max.z = Math.Max(p.z, max.z);
                        }
                    }
                }
            }

            CadPoint cp = (max - min) / 2f + min;

            DebugOut.Std.println("GetSelectionCenter() sel pt cnt=" + selPointCnt.ToString());

            return cp;
        }

        public void SetLoop(DrawContext dc, bool isLoop)
        {
            List<uint> list = GetSelectedFigIDList();

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (uint id in list)
            {
                CadFigure fig = DB.getFigure(id);

                if (fig.Type != CadFigure.Types.POLY_LINES)
                {
                    continue;
                }

                if (fig.Closed != isLoop)
                {
                    fig.Closed = isLoop;

                    if (isLoop)
                    {
                        fig.Normal = CadMath.Normal(fig.PointList);
                    }

                    ope = CadOpe.CreateSetCloseOpe(CurrentLayer.ID, id, isLoop);
                    opeRoot.OpeList.Add(ope);
                }
            }

            mHistoryManager.foward(opeRoot);

            Clear(dc);
            Draw(dc);
        }

        public void Flip(DrawContext dc, TargetCoord coord)
        {
            CadPoint cp = GetSelectionCenter();

            StartEdit();

            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    int num = fig.PointList.Count;
                    int selnum = 0;

                    for (int i = 0; i < num; i++)
                    {
                        CadPoint p = fig.PointList[i];

                        if (p.Selected)
                        {
                            selnum++;

                            CadPoint np = p;
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

                    if (selnum == num)
                    {
                        fig.PointList.Reverse();
                    }
                }
            }

            EndEdit();

            Clear(dc);
            DrawAll(dc);
        }

        public void FlipX(DrawContext dc)
        {
            Flip(dc, TargetCoord.X);
        }

        public void FlipY(DrawContext dc)
        {
            Flip(dc, TargetCoord.Y);
        }

        public void FlipZ(DrawContext dc)
        {
            Flip(dc, TargetCoord.Z);
        }

        public void FlipNormal(DrawContext dc)
        {
            List<uint> ids = GetSelectedFigIDList();

            CadOpeList opeList = CadOpe.CreateListOpe();

            foreach (uint id in ids)
            {
                CadFigure fig = mDB.getFigure(id);
                CadPoint old = fig.Normal;

                fig.Normal *= -1;

                CadOpe ope = CadOpe.CreateChangeNormalOpe(id, old, fig.Normal);
                opeList.Add(ope);
            }


            HistoryManager.foward(opeList);

            Clear(dc);
            DrawAll(dc);
        }

        public bool InsPointToLastSelectedSeg()
        {
            MarkSeg seg = SelSegList.LastSel;

            CadFigure fig = DB.getFigure(seg.FigureID);

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

            handle |= fig.GetPointAt(seg.PtIndexA).Type == CadPoint.Types.HANDLE;
            handle |= fig.GetPointAt(seg.PtIndexB).Type == CadPoint.Types.HANDLE;

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

            StartEdit();

            fig.InsertPointAt(ins, LastDownPoint);

            EndEdit();

            return true;
        }

        public void AddCentroid(DrawContext dc)
        {
            List<uint> idList = GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = mDB.getFigure(id);

                Centroid t = fig.GetCentroid();

                if (cent.IsInvalid)
                {
                    cent = t;
                    continue;
                }

                if (t.IsInvalid)
                {
                    continue;
                }

                cent = CadUtil.MergeCentroid(cent, t, false);
            }

            if (cent.IsInvalid)
            {
                return;
            }

            CadFigure pointFig = mDB.newFigure(CadFigure.Types.POINT);
            pointFig.AddPoint(cent.Point);

            pointFig.EndCreate(dc);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, pointFig.ID);
            HistoryManager.foward(ope);
            CurrentLayer.addFigure(pointFig);

            String s = string.Format("({0:0.000},{1:0.000},{2:0.000})",
                               cent.Point.x, cent.Point.y, cent.Point.z);

            InteractOut.print("Centroid:" + s);
            InteractOut.print("Area:" + (cent.Area / 100).ToString() + "(㎠)");
        }

        public double Area()
        {
            List<uint> idList = GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = mDB.getFigure(id);

                Centroid t = fig.GetCentroid();

                if (cent.IsInvalid)
                {
                    cent = t;
                    continue;
                }

                if (t.IsInvalid)
                {
                    continue;
                }

                cent = CadUtil.MergeCentroid(cent, t, false);
            }

            if (cent.IsInvalid)
            {
                return 0;
            }

            return cent.Area;
        }
    }
}
