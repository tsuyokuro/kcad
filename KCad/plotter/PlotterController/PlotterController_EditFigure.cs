using System;
using System.Collections.Generic;

namespace Plotter
{
    public partial class PlotterController
    {
        public void Remove()
        {
            StartEdit();

            RemoveSelectedPoints();

            EndEdit();
        }

        public void ToBezier()
        {
            ToBezier(mSelectedSegs.LastSel);
            ClearSelection();
        }

        public void ToBezier(MarkSeg seg)
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

                mHistoryManager.foward(ope);
            }

            ClearSelection();
        }

        public void SeparateFigures()
        {
            SeparateFigures(mSelList.List);
            ClearSelection();
        }

        public void SeparateFigures(IReadOnlyList<SelectItem> selList)
        {
            CadFigureCutter fa = new CadFigureCutter(mDB);

            var res = fa.Cut(selList);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void BondFigures()
        {
            BondFigures(mSelList.List);
            ClearSelection();
        }

        public void BondFigures(IReadOnlyList<SelectItem> selList)
        {
            CadFigureBonder fa = new CadFigureBonder(mDB, CurrentLayer);

            var res = fa.Bond(selList);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void CutSegment()
        {
            MarkSeg ms = mSelectedSegs.LastSel;
            CutSegment(ms);
            ClearSelection();
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

            var res = segCutter.CutSegment(ms, mObjDownPoint.Value);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.CreateListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.AddFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.GetLayer(ri.LayerID);

                ope = CadOpe.CreateRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.RemoveFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        private CadVector GetSelectionCenter()
        {
            CadVector min = CadVector.Create(CadConst.MaxValue);
            CadVector max = CadVector.Create(CadConst.MinValue);

            int selPointCnt = 0;

            foreach (CadLayer layer in mDB.LayerList)
            {
                foreach (CadFigure fig in layer.FigureList)
                {
                    foreach (CadVector p in fig.PointList)
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

            CadVector cp = (max - min) / 2f + min;

            DebugOut.Std.println("GetSelectionCenter() sel pt cnt=" + selPointCnt.ToString());

            return cp;
        }

        public void SetLoop(bool isLoop)
        {
            List<uint> list = GetSelectedFigIDList();

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
                        fig.Normal = CadMath.Normal(fig.PointList);
                    }

                    ope = CadOpe.CreateSetCloseOpe(CurrentLayer.ID, id, isLoop);
                    opeRoot.OpeList.Add(ope);
                }
            }

            mHistoryManager.foward(opeRoot);
        }

        public void Flip(TargetCoord coord)
        {
            CadVector cp = GetSelectionCenter();

            StartEdit();

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

                    if (selnum == num)
                    {
                        fig.PointList.Reverse();
                    }
                }
            }

            EndEdit();
        }

        public void FlipX()
        {
            Flip(TargetCoord.X);
        }

        public void FlipY()
        {
            Flip(TargetCoord.Y);
        }

        public void FlipZ()
        {
            Flip(TargetCoord.Z);
        }

        public void FlipNormal()
        {
            List<uint> ids = GetSelectedFigIDList();

            CadOpeList opeList = CadOpe.CreateListOpe();

            foreach (uint id in ids)
            {
                CadFigure fig = mDB.GetFigure(id);
                CadVector old = fig.Normal;

                fig.Normal *= -1;

                CadOpe ope = CadOpe.CreateChangeNormalOpe(id, old, fig.Normal);
                opeList.Add(ope);
            }


            HistoryManager.foward(opeList);
        }

        public bool InsPointToLastSelectedSeg()
        {
            MarkSeg seg = SelSegList.LastSel;

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

            handle |= fig.GetPointAt(seg.PtIndexA).Type == CadVector.Types.HANDLE;
            handle |= fig.GetPointAt(seg.PtIndexB).Type == CadVector.Types.HANDLE;

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

        public Centroid Centroid()
        {
            List<uint> idList = GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = mDB.GetFigure(id);

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

            return cent;
        }


        public void AddCentroid()
        {
            Centroid cent = Centroid();

            if (cent.IsInvalid)
            {
                return;
            }

            CadFigure pointFig = mDB.NewFigure(CadFigure.Types.POINT);
            pointFig.AddPoint(cent.Point);

            pointFig.EndCreate(CurrentDC);

            CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, pointFig.ID);
            HistoryManager.foward(ope);
            CurrentLayer.AddFigure(pointFig);

            String s = string.Format("({0:0.000},{1:0.000},{2:0.000})",
                               cent.Point.x, cent.Point.y, cent.Point.z);

            InteractOut.println("Centroid:" + s);
            InteractOut.println("Area:" + (cent.Area / 100).ToString() + "(㎠)");
        }

        public double Area()
        {
            List<uint> idList = GetSelectedFigIDList();

            Centroid cent = default(Centroid);

            cent.IsInvalid = true;

            foreach (uint id in idList)
            {
                CadFigure fig = mDB.GetFigure(id);

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
