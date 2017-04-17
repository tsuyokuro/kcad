#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;

namespace Plotter
{
    public partial class PlotterController
    {
        const int SnapRange = 6;

        public CadMouse Mouse { get; } = new CadMouse();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();

        private CadRulerSet mRulerSet = new CadRulerSet();


        private CadPoint StoreViewOrg = default(CadPoint);

        private CadPoint mSnapPoint;

        private CadPoint mSnapScreenPoint;

        private CadPoint mMoveOrgScrnPoint;

        public CadPoint FreeDownPoint = default(CadPoint);

        private CadPoint? mObjDownPoint = null;


        private CadPoint mOffsetScreen = default(CadPoint);

        private CadPoint mOffsetWorld = default(CadPoint);

        private Gridding mGridding = new Gridding();

        public bool SnapToGrid
        {
            set
            {
                mGridding.Enable = value;
            }

            get
            {
                return mGridding.Enable;
            }
        }

        public Gridding Grid
        {
            get
            {
                return mGridding;
            }
        }

        private bool mSnapToFigure = true;

        public bool SnapToFigure
        {
            set
            {
                mSnapToFigure = value;
            }

            get
            {
                return mSnapToFigure;
            }
        }

        public bool SnapToLine
        {
            get; set;
        } = true;

        private void initHid()
        {
            Mouse.LDown = LDown;
            Mouse.LUp = LUp;

            Mouse.RDown = RDown;
            Mouse.RUp = RUp;

            Mouse.MDown = MDown;
            Mouse.MUp= MUp;

            Mouse.MovePointer = MovePointer;

            Mouse.Wheel = Wheel;
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
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                if (!isSelected(newSel))
                {
                    ClearSelection();
                }
            }
        }

        private void clearSelListConditional(CadMouse pointer, MarkSeg newSel)
        {
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                if (!isSelectedSeg(newSel))
                {
                    ClearSelection();
                }
            }
        }
        #endregion

        private void LDown(CadMouse pointer, DrawContext dc, int x, int y)
        {
            CadPoint pixp = CadPoint.Create(x, y, 0);
            CadPoint cp = dc.UnitPointToCadPoint(pixp);

            mOffsetScreen = pixp - mSnapScreenPoint;
            mOffsetWorld = cp - mSnapPoint;

            switch (State)
            {
                case States.SELECT:
                    mObjDownPoint = null;

                    mPointSearcher.Clean();
                    mPointSearcher.SetRangePixel(dc, SnapRange);
                    mPointSearcher.SearchAllLayer(dc, pixp, mDB);

                    mPointSearcher.CheckRelativePoints(dc, mDB);

                    MarkPoint mp = mPointSearcher.GetXYMatch();

                    if (mp.FigureID != 0 && mp.Type == MarkPoint.Types.POINT)
                    {
                        mObjDownPoint = mp.Point;

                        mMoveOrgScrnPoint = dc.CadPointToUnitPoint(mp.Point);

                        mMoveOrgScrnPoint.z = 0;


                        State = States.START_DRAGING_POINTS;
                        CadFigure fig = mDB.getFigure(mp.FigureID);

                        CadLayer layer = mDB.getLayer(mp.LayerID);

                        if (!layer.Locked)
                        {
                            clearSelListConditional(pointer, mp);

                            if (SelectMode == SelectModes.POINT)
                            {
                                mSelList.add(mp);
                                fig.SelectPointAt(mp.PointIndex, true);
                            }
                            else if (SelectMode == SelectModes.OBJECT)
                            {
                                mSelList.add(mp.LayerID, mDB.getFigure(mp.FigureID));
                                fig.SelectWithGroup();
                            }

                            // Set ignore liset for snap cursor
                            mPointSearcher.SetIgnoreList(mSelList.List);
                            mSegSearcher.SetIgnoreList(mSelList.List);

                            mRulerSet.Set(fig.PointList, mp.PointIndex, cp);
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
                        mSegSearcher.Clean();
                        mSegSearcher.SetRangePixel(dc, SnapRange);
                        mSegSearcher.SearchAllLayer(dc, pixp, mDB);
                        MarkSeg mseg = mSegSearcher.GetMatch();

                        CadLayer layer = mDB.getLayer(mseg.LayerID);

                        if (mseg.FigureID != 0 && !layer.Locked)
                        {
                            mObjDownPoint = mseg.CrossPoint;

                            CadFigure fig = mDB.getFigure(mseg.FigureID);

                            clearSelListConditional(pointer, mseg);

                            if (SelectMode == SelectModes.POINT)
                            {
                                mSelList.add(mseg.LayerID, mDB.getFigure(mseg.FigureID), mseg.PtIndexA, mseg.PtIndexB);
                                fig.SelectPointAt(mseg.PtIndexA, true);
                                fig.SelectPointAt(mseg.PtIndexB, true);
                            }
                            else if (SelectMode == SelectModes.OBJECT)
                            {
                                mSelList.add(mseg.LayerID, mDB.getFigure(mseg.FigureID));
                                fig.SelectWithGroup();
                            }

                            mSelectedSegs.Add(mseg);

                            mMoveOrgScrnPoint = dc.CadPointToUnitPoint(mObjDownPoint.Value);

                            State = States.START_DRAGING_POINTS;

                            // Set ignore liset for snap cursor
                            mPointSearcher.SetIgnoreList(mSelList.List);
                            mSegSearcher.SetIgnoreList(mSelList.List);
                            mSegSearcher.SetIgnoreSeg(mSelectedSegs.List);
                        }
                        else
                        {
                            if (!CadKeyboard.IsCtrlKeyDown())
                            {
                                ClearSelection();
                            }
                        }
                    }

                    if (mObjDownPoint == null)
                    {
                        CadPoint p = pixp;

                        #region Gridding
                        mGridding.Clear();
                        mGridding.Check(dc, pixp);

                        if (mGridding.XMatchU.Valid)
                        {
                            p.x = mGridding.XMatchU.x;
                        }

                        if (mGridding.YMatchU.Valid)
                        {
                            p.y = mGridding.YMatchU.y;
                        }
                        #endregion

                        FreeDownPoint = dc.UnitPointToCadPoint(p);
                    }
                    else
                    {
                        FreeDownPoint = mObjDownPoint.Value;
                    }

                    //DrawSelectedItems(dc);
                    Clear(dc);
                    DrawAll(dc);

                    return;

                case States.START_CREATE:
                    {
                        FreeDownPoint = mSnapPoint;

                        CreatingFigure = mDB.newFigure(CreatingFigType);
                        State = States.CREATING;

                        CreatingFigure.StartCreate(dc);


                        CadPoint p = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        SetPointInCreating(dc, p);
                        Draw(dc);
                    }
                    break;

                case States.CREATING:
                    {
                        FreeDownPoint = mSnapPoint;

                        CadPoint p = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        SetPointInCreating(dc, p);
                        Draw(dc);
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
            if (pointer.MDownPoint.x == x && pointer.MDownPoint.y == y)
            {
                AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
            }
        }

        private void MDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            CadPoint cp = default(CadPoint);
            cp.set(x, y, 0);

            CadPoint d = cp - pointer.MDownPoint;

            CadPoint op = StoreViewOrg + d;

            SetOrigin(dc, (int)op.x, (int)op.y);
        }

        private void Wheel(CadMouse pointer, DrawContext dc, int x, int y, int delta)
        {
            if (CadKeyboard.IsCtrlKeyDown())
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

                DpiUpDown(dc, f);
            }
        }

        private void RDown(CadMouse pointer, DrawContext dc, int x, int y)
        {
            DrawAll(dc);

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
                    mPointSearcher.SetIgnoreList(null);

                    mSegSearcher.SetIgnoreList(null);
                    mSegSearcher.SetIgnoreSeg(null);

                    if (State == States.DRAGING_POINTS)
                    {
                        EndEdit();
                    }

                    State = States.SELECT;

                    break;
            }

            mOffsetScreen = default(CadPoint);
            mOffsetWorld = default(CadPoint);
        }

        private void RUp(CadMouse pointer, DrawContext dc, int x, int y)
        {
        }

        private void MovePointer(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("Move");

            if ((Control.MouseButtons & MouseButtons.Middle) != 0)
            {
                MDrag(pointer, dc, x, y);
                return;
            }

            if (State == States.START_DRAGING_POINTS)
            {
                State = States.DRAGING_POINTS;
                StartEdit();
            }

            CadPoint pixp = CadPoint.Create(x, y, 0);
            CadPoint cp = dc.UnitPointToCadPoint(pixp);
            CadPoint tp = default(CadPoint);

            bool xmatch = false;
            bool ymatch = false;
            bool segmatch = false;


            mSnapScreenPoint = pixp - mOffsetScreen;
            mSnapPoint = cp - mOffsetWorld;

            //mSnapScrnPoint.dump(DebugOut.Std);

            Clear(dc);

            if (SnapToFigure)
            {
                mPointSearcher.Clean();
                mPointSearcher.SetRangePixel(dc, SnapRange);
                mPointSearcher.SetTargetPoint(pixp);

                if (CreatingFigure != null)
                {
                    if (CreatingFigure.PointCount == 1)
                    {
                        mPointSearcher.Check(dc, CreatingFigure.GetPointAt(0));
                    }
                }

                mPointSearcher.CheckRelativePoints(dc, mDB);

                // Search point
                mPointSearcher.SearchAllLayer(dc, mDB);

                MarkPoint mx = mPointSearcher.GetXMatch();
                MarkPoint my = mPointSearcher.GetYMatch();

                double dist = CadConst.MaxValue;

                if ((mx.Flag & MarkPoint.X_MATCH) != 0)
                {
                    dc.Drawing.DrawHighlightPoint(mx.Point);

                    tp = dc.CadPointToUnitPoint(mx.Point);

                    mSnapScreenPoint.x = tp.x;
                    mSnapScreenPoint.z = 0;

                    mSnapPoint = dc.UnitPointToCadPoint(mSnapScreenPoint);

                    dist = Math.Min(mx.DistX, dist);

                    xmatch = true;
                }

                if ((my.Flag & MarkPoint.Y_MATCH) != 0)
                {
                    dc.Drawing.DrawHighlightPoint(my.Point);

                    tp = dc.CadPointToUnitPoint(my.Point);

                    mSnapScreenPoint.y = tp.y;
                    mSnapScreenPoint.z = 0;

                    mSnapPoint = dc.UnitPointToCadPoint(mSnapScreenPoint);

                    dist = Math.Min(my.DistY, dist);

                    ymatch = true;
                }

                // Search segment
                mSegSearcher.Clean();
                mSegSearcher.SetRangePixel(dc, SnapRange);
                mSegSearcher.SearchAllLayer(dc, pixp, mDB);

                MarkSeg seg = mSegSearcher.GetMatch();

                if (seg.FigureID != 0)
                {
                    if (seg.Distance < dist)
                    {
                        CadFigure fig = mDB.getFigure(seg.FigureID);
                        fig.DrawSeg(dc, DrawTools.PEN_MATCH_SEG, seg.PtIndexA, seg.PtIndexB);

                        mSnapPoint = seg.CrossPoint;

                        mSnapScreenPoint = seg.CrossViewPoint;
                        mSnapScreenPoint.z = 0;

                        segmatch = true;

                        //mSnapScrnPoint.dump(DebugOut.Std);
                    }
                }
            }

            #region Gridding
            if (!segmatch)
            {
                mGridding.Clear();
                mGridding.Check(dc, pixp);

                if (!xmatch && mGridding.XMatchU.Valid)
                {
                    mSnapScreenPoint.x = mGridding.XMatchU.x;
                }

                if (!ymatch && mGridding.YMatchU.Valid)
                {
                    mSnapScreenPoint.y = mGridding.YMatchU.y;
                }

                mSnapPoint = dc.UnitPointToCadPoint(mSnapScreenPoint);
            }
            #endregion

            if (SnapToLine)
            {
                RulerInfo ri = mRulerSet.Capture(dc, cp, 8);

                if (!xmatch && !ymatch)
                {
                    if (ri.IsValid)
                    {
                        mSnapPoint = ri.CrossPoint;
                        mSnapScreenPoint = dc.CadPointToUnitPoint(mSnapPoint);
                    }
                }
            }

            //dc.Drawing.DrawCursorScrn(mSnapScrnPoint);

            switch (State)
            {
                case States.DRAGING_POINTS:
                    {
                        CadPoint p0 = dc.UnitPointToCadPoint(mMoveOrgScrnPoint);
                        CadPoint p1 = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        CadPoint delta = p1 - p0;

                        //mSnapScrnPoint.dump(DebugOut.Std);

                        MoveSelectedPoints(dc, delta);

                        break;
                    }

                case States.CREATING:
                    {
                        if (CreatingFigure != null)
                        {
                            CadPoint p = dc.UnitPointToCadPoint(mSnapScreenPoint);
                            CreatingFigure.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                        }
                        break;
                    }
            }

            DrawAll(dc);
            CursorPosChanged(this, mSnapPoint);
        }

        private void LDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("LDrag");
            MovePointer(pointer, dc, x, y);
        }
    }
}
