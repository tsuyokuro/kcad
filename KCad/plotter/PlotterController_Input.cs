#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public partial class PlotterController
    {
        const int SnapRange = 16;

        public CadMouse Mouse { get; } = new CadMouse();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();


        private CadPoint StoreViewOrg = default(CadPoint);

        private CadPoint mSnapPoint;

        private CadPoint mSnapScrnPoint;

        private CadPoint mMoveOrgScrnPoint;

        private CadPoint mFreeDownPoint = default(CadPoint);

        private CadPoint? mObjDownPoint = null;


        private CadPoint mOffsetScrn = default(CadPoint);

        private CadPoint mOffsetWld = default(CadPoint);

        private void initHid()
        {
            Mouse.setButtonDownProc(CadMouse.Buttons.L_BUTTON, LDown);
            Mouse.setButtonUpProc(CadMouse.Buttons.L_BUTTON, LUp);

            Mouse.setButtonDownProc(CadMouse.Buttons.R_BUTTON, RDown);
            Mouse.setButtonUpProc(CadMouse.Buttons.R_BUTTON, RUp);

            Mouse.setButtonDownProc(CadMouse.Buttons.M_BUTTON, MDown);
            Mouse.setButtonUpProc(CadMouse.Buttons.M_BUTTON, MUp);

            Mouse.setDragProc(CadMouse.Buttons.L_BUTTON, LDrag);
            Mouse.setDragProc(CadMouse.Buttons.M_BUTTON, MDrag);

            Mouse.setMoveProc(PointerMove);

            Mouse.setWheelProc(Wheel);
        }

        #region "Clear selection control"
        private bool isSelected(MarkPoint mp)
        {
            if (SelectMode == SelectModes.POINT)
            {
                return mSelList.isSelected(mp);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                return mSelList.isSelectedFigure(mp.FigureID);
            }

            return false;
        }

        private bool isSelectedSeg(MarkSeg ms)
        {
            if (SelectMode == SelectModes.POINT)
            {
                return mSelectedSegs.isSelected(ms);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                return mSelectedSegs.isSelectedFigure(ms.FigureID);
            }

            return false;
        }

        private void clearSelListConditional(CadMouse pointer, MarkPoint newSel)
        {
            if (!pointer.isDownCombiKey(CadMouse.CombiKeys.CTRL))
            {
                if (!isSelected(newSel))
                {
                    clearSelection();
                }
            }
        }

        private void clearSelListConditional(CadMouse pointer, MarkSeg newSel)
        {
            if (!pointer.isDownCombiKey(CadMouse.CombiKeys.CTRL))
            {
                if (!isSelectedSeg(newSel))
                {
                    clearSelection();
                }
            }
        }
        #endregion

        private void LDown(CadMouse pointer, DrawContext dc, int x, int y)
        {
            CadPoint pixp = CadPoint.GetNew(x, y, 0);
            CadPoint cp = dc.pixelPointToCadPoint(x,y,0);

            mOffsetScrn = pixp - mSnapScrnPoint;
            mOffsetWld = cp - mSnapPoint;

            switch (State)
            {
                case States.SELECT:
                    draw(dc);

                    //double d = dc.pixelDeltaToCadDelta(SnapRange);

                    mObjDownPoint = null;

                    mPointSearcher.clean();
                    mPointSearcher.setRangePixel(dc, SnapRange);
                    mPointSearcher.searchAllLayer(dc, pixp, mDB);

                    mPointSearcher.checkRelativePoints(dc, mDB);

                    MarkPoint mp = mPointSearcher.getXYMatch();

                    if (mp.FigureID != 0 && mp.Type == MarkPoint.Types.POINT)
                    {
                        mObjDownPoint = mp.Point;

                        mMoveOrgScrnPoint = dc.pointToPixelPoint(mp.Point);

                        State = States.START_DRAGING_POINTS;
                        CadFigure fig = mDB.getFigure(mp.FigureID);

                        CadLayer layer = mDB.getLayer(mp.LayerID);

                        if (!layer.Locked)
                        {
                            clearSelListConditional(pointer, mp);

                            if (SelectMode == SelectModes.POINT)
                            {
                                mSelList.add(mp);
                                fig.selectPointAt(mp.PointIndex, true);
                            }
                            else if (SelectMode == SelectModes.OBJECT)
                            {
                                mSelList.add(mp.LayerID, mDB.getFigure(mp.FigureID));
                                fig.SelectWithGroup();
                            }

                            // Set ignore liset for snap cursor
                            mPointSearcher.setIgnoreList(mSelList.List);
                            mSegSearcher.setIgnoreList(mSelList.List);
                        }
                    }
                    else if (mp.Type == MarkPoint.Types.RELATIVE_POINT)
                    {
                        CadLayer layer = mDB.getLayer(mp.LayerID);
                        CadRelativePoint rp = layer.RelPointList[mp.PointIndex];
                        rp.Selected = true;
                    }
                    else
                    {
                        mSegSearcher.clean();
                        mSegSearcher.setRangePixel(dc, SnapRange);
                        mSegSearcher.searchAllLayer(dc, pixp, mDB);
                        MarkSeg mseg = mSegSearcher.getMatch();

                        CadLayer layer = mDB.getLayer(mseg.LayerID);

                        if (mseg.FigureID != 0 && !layer.Locked)
                        {
                            mObjDownPoint = mseg.CrossPoint;

                            CadFigure fig = mDB.getFigure(mseg.FigureID);

                            clearSelListConditional(pointer, mseg);

                            if (SelectMode == SelectModes.POINT)
                            {
                                mSelList.add(mseg.LayerID, mDB.getFigure(mseg.FigureID), mseg.PtIndexA, mseg.PtIndexB);
                                fig.selectPointAt(mseg.PtIndexA, true);
                                fig.selectPointAt(mseg.PtIndexB, true);
                            }
                            else if (SelectMode == SelectModes.OBJECT)
                            {
                                mSelList.add(mseg.LayerID, mDB.getFigure(mseg.FigureID));
                                fig.SelectWithGroup();
                            }

                            mSelectedSegs.Add(mseg);

                            mMoveOrgScrnPoint = dc.pointToPixelPoint(mObjDownPoint.Value);

                            State = States.START_DRAGING_POINTS;

                            // Set ignore liset for snap cursor
                            mPointSearcher.setIgnoreList(mSelList.List);
                            mSegSearcher.setIgnoreList(mSelList.List);
                            mSegSearcher.setIgnoreSeg(mSelectedSegs.List);
                        }
                        else
                        {
                            if (!pointer.isDownCombiKey(CadMouse.CombiKeys.CTRL))
                            {
                                clearSelection();
                            }
                        }
                    }

                    if (mObjDownPoint == null)
                    {
                        mFreeDownPoint = cp;
                    }
                    else
                    {
                        mFreeDownPoint = mObjDownPoint.Value;
                    }

                    drawSelectedItems(dc);

                    return;

                case States.START_CREATE:
                    {
                        mFreeDownPoint = mSnapPoint;

                        CreatingFigure = mDB.newFigure(CreatingFigType);
                        State = States.CREATING;

                        CreatingFigure.startCreate();


                        CadPoint p = dc.pixelPointToCadPoint(mSnapScrnPoint);

                        setPointInCreating(dc, p);
                        draw(dc);
                    }
                    break;

                case States.CREATING:
                    {
                        mFreeDownPoint = mSnapPoint;

                        CadPoint p = dc.pixelPointToCadPoint(mSnapScrnPoint);

                        setPointInCreating(dc, p);
                        draw(dc);
                    }
                    break;

                default:
                    break;

            }
        }

        private void MDown(CadMouse pointer, DrawContext dc, int x, int y)
        {
            StoreViewOrg = dc.ViewOrg;
        }

        private void MUp(CadMouse pointer, DrawContext dc, int x, int y)
        {
            if (pointer.DownPoint.x == x && pointer.DownPoint.y == y)
            {
                adjustOrigin(dc, x, y, dc.ViewWidth, dc.ViewHeight);
            }
        }

        private void MDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            CadPoint cp = default(CadPoint);
            cp.set(x, y, 0);

            CadPoint d = cp - pointer.DownPoint;

            CadPoint op = StoreViewOrg + d;

            setOrigin(dc, (int)op.x, (int)op.y);
        }

        private void Wheel(CadMouse pointer, DrawContext dc, int x, int y, int delta)
        {
            if (pointer.isDownCombiKey(CadMouse.CombiKeys.CTRL))
            {
                double f = 1.0;

                if (delta > 0)
                {
                    f = 1.2;
                }
                else
                {
                    f = 0.8;
                }

                dpiUpDown(dc, f);
            }
        }

        private void RDown(CadMouse pointer, DrawContext dc, int x, int y)
        {
            draw(dc);
            drawSubItems(dc);

            if (RequestContextMenu != null)
            {
                StateInfo si = default(StateInfo);
                si.set(this);
                RequestContextMenu(this, si, x, y);
            }
        }

        private void LUp(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("LUp");
            switch (State)
            {
                case States.SELECT:
                    break;

                case States.START_DRAGING_POINTS:
                case States.DRAGING_POINTS:
                    mPointSearcher.setIgnoreList(null);

                    mSegSearcher.setIgnoreList(null);
                    mSegSearcher.setIgnoreSeg(null);

                    if (State == States.DRAGING_POINTS)
                    {
                        endEdit();
                    }

                    State = States.SELECT;

                    break;
            }

            mOffsetScrn = default(CadPoint);
            mOffsetWld = default(CadPoint);
        }

        private void RUp(CadMouse pointer, DrawContext dc, int x, int y)
        {
        }

        private void PointerMove(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("Move");

            if (State == States.START_DRAGING_POINTS)
            {
                State = States.DRAGING_POINTS;
                startEdit();
            }

            CadPoint pixp = CadPoint.GetNew(x, y, 0);
            CadPoint cp = dc.pixelPointToCadPoint(pixp);
            CadPoint tp = default(CadPoint);

            mSnapScrnPoint = pixp - mOffsetScrn;
            mSnapPoint = cp - mOffsetWld;

            clear(dc);
            draw(dc);

            mPointSearcher.clean();
            mPointSearcher.setRangePixel(dc, SnapRange);
            mPointSearcher.setTargetPoint(pixp);

            if (CreatingFigure != null)
            {
                if (CreatingFigure.PointCount == 1)
                {
                    mPointSearcher.check(dc, CreatingFigure.getPointAt(0));
                }
            }

            mPointSearcher.checkRelativePoints(dc, mDB);

            // Search point
            mPointSearcher.searchAllLayer(dc, mDB);

            MarkPoint mx = mPointSearcher.getXMatch();
            MarkPoint my = mPointSearcher.getYMatch();

            double dist = Double.MaxValue;

            if ((mx.Flag & MarkPoint.X_MATCH) != 0)
            {
                Drawer.drawHighlitePoint(dc, mx.Point);

                tp = dc.pointToPixelPoint(mx.Point);

                mSnapScrnPoint.x = tp.x;
                mSnapScrnPoint.z = 0;

                mSnapPoint = dc.pixelPointToCadPoint(mSnapScrnPoint);

                dist = Math.Min(mx.DistX, dist);
            }

            if ((my.Flag & MarkPoint.Y_MATCH) != 0)
            {
                Drawer.drawHighlitePoint(dc, my.Point);

                tp = dc.pointToPixelPoint(my.Point);

                mSnapScrnPoint.y = tp.y;
                mSnapScrnPoint.z = 0;

                mSnapPoint = dc.pixelPointToCadPoint(mSnapScrnPoint);

                dist = Math.Min(my.DistY, dist);
            }

            // Search segment
            mSegSearcher.clean();
            mSegSearcher.setRangePixel(dc, SnapRange);
            mSegSearcher.searchAllLayer(dc, pixp, mDB);

            MarkSeg seg = mSegSearcher.getMatch();

            if (seg.FigureID != 0)
            {
                if (seg.Distance < dist)
                {
                    CadFigure fig = mDB.getFigure(seg.FigureID);
                    fig.drawSeg(dc, dc.Tools.MatchSegPen, seg.PtIndexA, seg.PtIndexB);

                    mSnapPoint = seg.CrossPoint;

                    mSnapScrnPoint = seg.CrossViewPoint;
                    mSnapScrnPoint.z = 0;
                }
            }

            switch (State)
            {
                case States.DRAGING_POINTS:
                    {
                        CadPoint p0 = dc.pixelPointToCadPoint(mMoveOrgScrnPoint);
                        CadPoint p1 = dc.pixelPointToCadPoint(mSnapScrnPoint);

                        CadPoint delta = p1 - p0;

                        //DebugOut o = new DebugOut();
                        //mSnapCursorPos.dump(o);

                        moveSelectedPoints(delta);

                        break;
                    }

                case States.CREATING:
                    {
                        if (CreatingFigure != null)
                        {
                            CadPoint p = dc.pixelPointToCadPoint(mSnapScrnPoint);
                            CreatingFigure.drawTemp(dc, p, dc.Tools.TempFigurePen);
                        }
                        break;
                    }
            }

            drawSubItems(dc);
        }

        private void LDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("LDrag");
            PointerMove(pointer, dc, x, y);
        }
    }
}
