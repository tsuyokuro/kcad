using System.Collections.Generic;

namespace Plotter
{
    public partial class PlotterController
    {
        public void remove(DrawContext dc)
        {
            startEdit();

            removeSelectedPoints();

            markRemoveSelectedRelPoints();

            endEdit();

            clear(dc);
            draw(dc);
        }

        public void closeFigure(DrawContext g)
        {
            CreatingFigure.Closed = true;

            CadOpe ope = CadOpe.getSetCloseOpe(CurrentLayer.ID, CreatingFigure.ID, true);
            mHistoryManager.foward(ope);

            nextState();
        }

        public void toBezier(DrawContext dc)
        {
            toBezier(dc, mSelectedSegs.LastSel);
            clearSelection();
            draw(dc);
        }

        public void toBezier(DrawContext dc, MarkSeg seg)
        {
            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = mDB.getFigure(seg.FigureID);

            int num = CadPointUtil.initBezier(fig, seg.PtIndexA, seg.PtIndexB);

            if (num > 0)
            {
                CadOpe ope = CadOpe.getInsertPointsOpe(
                    fig.LayerID, fig.ID, seg.PtIndexA + 1, num);

                mHistoryManager.foward(ope);
            }

            clearSelection();

            draw(dc);
        }

        public void separateFigures(DrawContext dc)
        {
            separateFigures(mSelList.List);
            clearSelection();
            draw(dc);
        }

        public void separateFigures(List<SelectItem> selList)
        {
            CadFigureCutter fa = new CadFigureCutter(mDB);

            var res = fa.cut(selList);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.getListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.getAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.addFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.getRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.removeFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void bondFigures(DrawContext dc)
        {
            bondFigures(mSelList.List);
            clearSelection();
            draw(dc);
        }

        public void bondFigures(List<SelectItem> selList)
        {
            CadFigureBonder fa = new CadFigureBonder(mDB, CurrentLayer);

            var res = fa.bond(selList);

            if (!res.isValid())
            {
                return;
            }

            CadOpeList opeRoot = CadOpe.getListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.getAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.addFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.getRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.removeFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void cutSegment(DrawContext dc)
        {
            MarkSeg ms = mSelectedSegs.LastSel;
            cutSegment(ms);
            clearSelection();

            draw(dc);
        }

        public void cutSegment(MarkSeg ms)
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

            CadOpeList opeRoot = CadOpe.getListOpe();
            CadOpe ope;

            foreach (CadFigureAssembler.ResultItem ri in res.AddList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.getAddFigureOpe(ri.LayerID, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.addFigure(ri.Figure);
            }

            foreach (CadFigureAssembler.ResultItem ri in res.RemoveList)
            {
                CadLayer layer = mDB.getLayer(ri.LayerID);

                ope = CadOpe.getRemoveFigureOpe(layer, ri.FigureID);
                opeRoot.OpeList.Add(ope);

                layer.removeFigureByID(ri.FigureID);
            }

            mHistoryManager.foward(opeRoot);
        }

        public void addCenterPoint(DrawContext dc)
        {
            if (mSelectedSegs.List.Count > 0)
            {
                foreach (MarkSeg seg in mSelectedSegs.List)
                {
                    CadFigure fig = mDB.getFigure(seg.FigureID);

                    CadRelativePoint rp = mDB.newRelPoint();

                    rp.set(
                        CadRelativePoint.Types.CENTER,
                        fig, seg.PtIndexA,
                        fig, seg.PtIndexB
                        );

                    CadLayer layer = mDB.getLayer(seg.LayerID);

                    layer.RelPointList.Add(rp);
                }

                draw(dc);
                return;
            }

            if (mSelList.List.Count == 2)
            {
                SelectItem si0 = mSelList.List[0];
                SelectItem si1 = mSelList.List[1];

                if (si0.LayerID == si1.LayerID)
                {
                    CadFigure fig0 = mDB.getFigure(si0.FigureID);
                    CadFigure fig1 = mDB.getFigure(si1.FigureID);

                    CadRelativePoint rp = mDB.newRelPoint();

                    rp.set(
                        CadRelativePoint.Types.CENTER,
                        fig0, si0.PointIndex,
                        fig1, si1.PointIndex
                        );

                    CadLayer layer = mDB.getLayer(si0.LayerID);
                    layer.RelPointList.Add(rp);
                }
            }

            draw(dc);
        }
    }
}
