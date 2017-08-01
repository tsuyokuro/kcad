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
        public double PointSnapRange
        {
            set;
            get;
        } = 6;

        public double LineSnapRange
        {
            set;
            get;
        } = 8;

        public CadMouse Mouse { get; } = new CadMouse();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();

        private CadRulerSet mRulerSet = new CadRulerSet();


        private CadVector StoreViewOrg = default(CadVector);

        private CadVector mSnapPoint;

        private CadVector mSnapScreenPoint;

        private CadVector mMoveOrgScrnPoint;

        public CadVector LastDownPoint = default(CadVector);

        private CadVector? mObjDownPoint = null;


        private CadVector mOffsetScreen = default(CadVector);

        private CadVector mOffsetWorld = default(CadVector);

        private Gridding mGridding = new Gridding();

        private CadFigure CurrentFigure = null;

        private int MatchIndex = 0;

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

        private bool mSnapToPoint = true;

        public bool SnapToPoint
        {
            set
            {
                mSnapToPoint = value;
            }

            get
            {
                return mSnapToPoint;
            }
        }

        private bool mSnapToSegment = true;

        public bool SnapToSegment
        {
            set
            {
                mSnapToSegment = value;
            }

            get
            {
                return mSnapToSegment;
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
        
        public void SetCurrentFigure(CadFigure fig)
        {
            if (CurrentFigure != null)
            {
                CurrentFigure.Current = false;
            }

            if (fig != null)
            {
                fig.Current = true;
            }

            CurrentFigure = fig;
        }

        private void LDown(CadMouse pointer, DrawContext dc, int x, int y)
        {
            CadVector pixp = CadVector.Create(x, y, 0);
            CadVector cp = dc.UnitPointToCadPoint(pixp);

            mOffsetScreen = pixp - mSnapScreenPoint;
            mOffsetWorld = cp - mSnapPoint;

            switch (State)
            {
                case States.SELECT:
                    mObjDownPoint = null;

                    mPointSearcher.CleanMatches();
                    mPointSearcher.SetRangePixel(dc, PointSnapRange);
                    mPointSearcher.SearchAllLayer(dc, pixp, mDB);

                    MarkPoint mp = default(MarkPoint);

                    mp = mPointSearcher.GetXYMatch(MatchIndex);

                    if (CadKeyboard.IsShiftKeyDown())
                    {
                        MatchIndex++;
                        mp = mPointSearcher.GetXYMatch(MatchIndex);
                    }

                    if (mp.FigureID == 0)
                    {
                        MatchIndex = 0;
                    }

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

                            // Set ignore list for snap cursor
                            mPointSearcher.SetIgnoreList(mSelList.List);
                            mSegSearcher.SetIgnoreList(mSelList.List);

                            mRulerSet.Set(fig.PointList, mp.PointIndex, cp);

                            SetCurrentFigure(fig);
                        }
                    }
                    else
                    {
                        mSegSearcher.Clean();
                        mSegSearcher.SetRangePixel(dc, LineSnapRange);
                        mSegSearcher.SearchAllLayer(dc, pixp, mDB);
                        MarkSeg mseg = mSegSearcher.GetMatch();

                        CadLayer layer = mDB.getLayer(mseg.LayerID);

                        if (mseg.FigureID != 0 && !layer.Locked)
                        {

                            CadVector center = mseg.CenterPoint;

                            CadVector t = dc.CadPointToUnitPoint(center);

                            if ((t - pixp).Norm() < LineSnapRange)
                            {
                                mObjDownPoint = center;
                            }
                            else
                            {
                                mObjDownPoint = mseg.CrossPoint;
                            }


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

                            SetCurrentFigure(fig);
                        }
                        else
                        {
                            if (!CadKeyboard.IsCtrlKeyDown())
                            {
                                ClearSelection();
                                SetCurrentFigure(null);
                            }
                        }
                    }

                    if (mObjDownPoint == null)
                    {
                        LastDownPoint = mSnapPoint;

                        #region Gridding

                        if (mGridding.Enable)
                        {
                            CadVector p = pixp;

                            bool match = false;

                            mGridding.Clear();
                            mGridding.Check(dc, pixp);

                            if (mGridding.XMatchU.Valid)
                            {
                                p.x = mGridding.XMatchU.x;
                                match = true;
                            }

                            if (mGridding.YMatchU.Valid)
                            {
                                p.y = mGridding.YMatchU.y;
                                match = true;
                            }

                            if (match)
                            {
                                LastDownPoint = dc.UnitPointToCadPoint(p);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        LastDownPoint = mObjDownPoint.Value;
                    }

                    Clear(dc);
                    DrawAll(dc);

                    return;

                case States.START_CREATE:
                    {
                        LastDownPoint = mSnapPoint;

                        CreatingFigure = mDB.newFigure(CreatingFigType);
                        State = States.CREATING;

                        CreatingFigure.StartCreate(dc);


                        CadVector p = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        SetPointInCreating(dc, p);
                        Draw(dc);
                    }
                    break;

                case States.CREATING:
                    {
                        LastDownPoint = mSnapPoint;

                        CadVector p = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        SetPointInCreating(dc, p);
                        Draw(dc);
                    }
                    break;

                case States.MEASURING:
                    {
                        LastDownPoint = mSnapPoint;
                        CadVector p = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        SetPointInMeasuring(dc, p);
                        Draw(dc);

                        PutMeasure();
                    }
                    break;

                default:
                    break;

            }

            CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }

        private void PutMeasure()
        {
            double d = CadUtil.AroundLength(MeasureFigure);

            d = Math.Round(d, 4);

            int cnt = MeasureFigure.PointCount;

            if (d >= 10.0)
            {
                InteractOut.print("(" + cnt.ToString() + ") " + (d / 10.0).ToString() + "cm");
            }
            else
            {
                InteractOut.print("(" + cnt.ToString() + ") " + d.ToString() + "mm");
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
            CadVector cp = default(CadVector);
            cp.set(x, y, 0);

            CadVector d = cp - pointer.MDownPoint;

            CadVector op = StoreViewOrg + d;

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

            mOffsetScreen = default(CadVector);
            mOffsetWorld = default(CadVector);
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

            CadVector pixp = CadVector.Create(x, y, 0);
            CadVector cp = dc.UnitPointToCadPoint(pixp);
            CadVector tp = default(CadVector);

            bool xmatch = false;
            bool ymatch = false;
            bool segmatch = false;

            double dist = CadConst.MaxValue;

            mSnapScreenPoint = pixp - mOffsetScreen;
            mSnapPoint = cp - mOffsetWorld;

            //mSnapScrnPoint.dump(DebugOut.Std);

            Clear(dc);

            if (SnapToPoint)
            {
                mPointSearcher.CleanMatches();
                mPointSearcher.SetRangePixel(dc, PointSnapRange);
                mPointSearcher.SetTargetPoint(pixp);

                if (CreatingFigure != null)
                {
                    if (CreatingFigure.PointCount == 1)
                    {
                        mPointSearcher.Check(dc, CreatingFigure.GetPointAt(0));
                    }
                }

                if (MeasureFigure != null)
                {
                    mPointSearcher.Check(dc, MeasureFigure.PointList);
                }


                // Search point
                mPointSearcher.SearchAllLayer(dc, mDB);

                MarkPoint mxy = mPointSearcher.GetXYMatch();
                MarkPoint mx = mPointSearcher.GetXMatch();
                MarkPoint my = mPointSearcher.GetYMatch();

                if ((mx.Flag & MarkPoint.X_MATCH) != 0)
                {
                    dc.Drawing.DrawHighlightPoint(mx.Point);

                    tp = dc.CadPointToUnitPoint(mx.Point);

                    mSnapScreenPoint.x = tp.x;
                    mSnapScreenPoint.z = 0;

                    mSnapPoint = dc.UnitPointToCadPoint(mSnapScreenPoint);

                    dist = (tp - pixp).Norm();

                    xmatch = true;
                }

                if ((my.Flag & MarkPoint.Y_MATCH) != 0)
                {
                    dc.Drawing.DrawHighlightPoint(my.Point);

                    tp = dc.CadPointToUnitPoint(my.Point);

                    mSnapScreenPoint.y = tp.y;
                    mSnapScreenPoint.z = 0;

                    mSnapPoint = dc.UnitPointToCadPoint(mSnapScreenPoint);

                    dist = (tp - pixp).Norm();

                    ymatch = true;
                }

                if (mxy.FigureID != 0 && mxy.Type == MarkPoint.Types.POINT)
                {
                    dc.Drawing.DrawHighlightPoint(mxy.Point, DrawTools.PEN_POINT_HIGHTLITE2);
                }
            }

            if (mSnapToSegment)
            {
                // Search segment
                mSegSearcher.Clean();
                mSegSearcher.SetRangePixel(dc, LineSnapRange);
                mSegSearcher.SearchAllLayer(dc, pixp, mDB);

                MarkSeg seg = mSegSearcher.GetMatch();

                if (seg.FigureID != 0)
                {
                    if (seg.Distance < dist)
                    {
                        CadFigure fig = mDB.getFigure(seg.FigureID);
                        fig.DrawSeg(dc, DrawTools.PEN_MATCH_SEG, seg.PtIndexA, seg.PtIndexB);

                        CadVector center = seg.CenterPoint;

                        CadVector t = dc.CadPointToUnitPoint(center);

                        if ((t - pixp).Norm() < LineSnapRange)
                        {
                            dc.Drawing.DrawHighlightPoint(center);

                            mSnapPoint = center;
                            mSnapScreenPoint = t;
                            mSnapScreenPoint.z = 0;
                        }
                        else
                        {
                            mSnapPoint = seg.CrossPoint;
                            mSnapScreenPoint = seg.CrossViewPoint;
                            mSnapScreenPoint.z = 0;
                        }


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
                RulerInfo ri = mRulerSet.Capture(dc, cp, LineSnapRange);

                if (!xmatch && !ymatch)
                {
                    if (ri.IsValid)
                    {
                        mSnapPoint = ri.CrossPoint;
                        mSnapScreenPoint = dc.CadPointToUnitPoint(mSnapPoint);
                    }
                }
            }

            switch (State)
            {
                case States.DRAGING_POINTS:
                    {
                        CadVector p0 = dc.UnitPointToCadPoint(mMoveOrgScrnPoint);
                        CadVector p1 = dc.UnitPointToCadPoint(mSnapScreenPoint);

                        CadVector delta = p1 - p0;

                        MoveSelectedPoints(dc, delta);

                        DrawAll(dc);
                        break;
                    }

                case States.CREATING:
                    {
                        DrawAll(dc);

                        if (CreatingFigure != null)
                        {
                            CadVector p = dc.UnitPointToCadPoint(mSnapScreenPoint);
                            CreatingFigure.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                        }
                        break;
                    }
                case States.MEASURING:
                    {
                        DrawAll(dc);

                        if (MeasureFigure != null)
                        {
                            CadVector p = dc.UnitPointToCadPoint(mSnapScreenPoint);
                            MeasureFigure.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                        }
                        break;
                    }
                default:
                    DrawAll(dc);
                    break;
            }

            CursorPosChanged(this, mSnapPoint, CursorType.TRACKING);
            CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }

        private void LDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("LDrag");
            MovePointer(pointer, dc, x, y);
        }


        private void SetPointInCreating(DrawContext dc, CadVector p)
        {
            CreatingFigure.AddPointInCreating(dc, p);

            CadFigure.States state = CreatingFigure.State;

            if (state == CadFigure.States.FULL)
            {
                CreatingFigure.EndCreate(dc);

                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, CreatingFigure.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(CreatingFigure);

                NextState();
            }
            else if (state == CadFigure.States.ENOUGH)
            {
                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, CreatingFigure.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.addFigure(CreatingFigure);
            }
            else if (state == CadFigure.States.CONTINUE)
            {
                CadOpe ope = CadOpe.CreateAddPointOpe(
                    CurrentLayer.ID,
                    CreatingFigure.ID,
                    CreatingFigure.PointCount - 1,
                    ref p
                    );

                mHistoryManager.foward(ope);
            }
        }

        private void SetPointInMeasuring(DrawContext dc, CadVector p)
        {
            MeasureFigure.AddPointInCreating(dc, p);
        }
    }
}
