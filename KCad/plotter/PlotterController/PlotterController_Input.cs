﻿#define LOG_DEBUG

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

        public CadCursor CrossCursor = CadCursor.Create();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();

        private CadRulerSet mRulerSet = new CadRulerSet();


        private CadVector StoreViewOrg = default(CadVector);

        private CadVector mSnapPoint;

        private CadVector mSnapScrnPoint;

        private CadVector mMoveOrgScrnPoint;

        public CadVector LastDownPoint = default(CadVector);


        private CadVector? mObjDownPoint = null;

        private CadVector mOffsetScreen = default(CadVector);

        //private CadVector mOffsetWorld = default(CadVector);


        public CadVector RubberBandScrnPoint0 = CadVector.InvalidValue;

        public CadVector RubberBandScrnPoint1 = default(CadVector);


        private Gridding mGridding = new Gridding();

        private CadFigure CurrentFigure = null;

        private int MatchIndex = 0;

        private bool CursorLocked = false;

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

        private void InitHid()
        {
            Mouse.LButtonDown = LButtonDown;
            Mouse.LButtonUp = LButtonUp;

            Mouse.RButtonDown = RButtonDown;
            Mouse.RButtonUp = RButtonUp;

            Mouse.MButtonDown = MButtonDown;
            Mouse.MButtonUp = MButtonUp;

            Mouse.PointerMoved = PointerMoved;

            Mouse.Wheel = Wheel;
        }

        #region "Clear selection control"
        private bool IsSelected(MarkPoint mp)
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

        private bool IsSelectedSeg(MarkSeg ms)
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

        private void ClearSelListConditional(MarkPoint newSel)
        {
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                if (!IsSelected(newSel))
                {
                    ClearSelection();
                }
            }
        }

        private void ClearSelListConditional(MarkSeg newSel)
        {
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                if (!IsSelectedSeg(newSel))
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

        /// <summary>
        /// 指定デバイス座標について範囲内かつ最も近い図形を選択
        /// </summary>
        /// <param name="dc"> DrawContext</param>
        /// <param name="pixp">スクリーン座標</param>
        /// <returns>
        /// true:  何かオブジェクトを選択した
        /// false: 何も選択しなかった
        /// </returns>
        public bool SelectNearest(DrawContext dc, CadVector pixp)
        {
            CadVector cp = dc.UnitPointToCadPoint(pixp);

            bool sel = false;

            mObjDownPoint = null;

            mPointSearcher.CleanMatches();
            mPointSearcher.SetRangePixel(dc, PointSnapRange);

            if (CurrentFigure != null)
            {
                mPointSearcher.CheckFigure(dc, CurrentLayer, CurrentFigure);
            }

            CadCursor cc = CadCursor.Create(pixp);

            mPointSearcher.SetTargetPoint(cc);

            mPointSearcher.SearchAllLayer(dc, mDB);

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
                CadFigure fig = mDB.GetFigure(mp.FigureID);

                CadLayer layer = mDB.GetLayer(mp.LayerID);

                if (!layer.Locked)
                {
                    ClearSelListConditional(mp);

                    if (SelectMode == SelectModes.POINT)
                    {
                        mSelList.add(mp);
                        sel = true;
                        fig.SelectPointAt(mp.PointIndex, true);
                    }
                    else if (SelectMode == SelectModes.OBJECT)
                    {
                        mSelList.add(mp.LayerID, mDB.GetFigure(mp.FigureID));
                        sel = true;
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
                mSegSearcher.SetTargetPoint(cc);

                mSegSearcher.SearchAllLayer(dc, mDB);

                MarkSeg mseg = mSegSearcher.GetMatch();

                CadLayer layer = mDB.GetLayer(mseg.LayerID);

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


                    CadFigure fig = mDB.GetFigure(mseg.FigureID);

                    ClearSelListConditional(mseg);

                    if (SelectMode == SelectModes.POINT)
                    {
                        mSelList.add(mseg.LayerID, mDB.GetFigure(mseg.FigureID), mseg.PtIndexA, mseg.PtIndexB);
                        sel = true;

                        fig.SelectPointAt(mseg.PtIndexA, true);
                        fig.SelectPointAt(mseg.PtIndexB, true);
                    }
                    else if (SelectMode == SelectModes.OBJECT)
                    {
                        mSelList.add(mseg.LayerID, mDB.GetFigure(mseg.FigureID));
                        sel = true;

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

            if (mObjDownPoint != null)
            {
                LastDownPoint = mObjDownPoint.Value;

                // LastDownPointを投影面上にしたい場合は、こちら
                //LastDownPoint = mSnapPoint;
            }
            else
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

            Clear(dc);
            DrawAll(dc);

            return sel;
        }

        private void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (CursorLocked)
            {
                x = CrossCursor.Pos.x;
                y = CrossCursor.Pos.y;
                CursorLocked = false;
            }

            CadVector pixp = CadVector.Create(x, y, 0);
            CadVector cp = dc.UnitPointToCadPoint(pixp);

            RubberBandScrnPoint1 = pixp;
            RubberBandScrnPoint0 = pixp;


            mOffsetScreen = pixp - mSnapScrnPoint;
            //mOffsetWorld = cp - mSnapPoint;

            switch (State)
            {
                case States.SELECT:
                    if (!SelectNearest(dc, mSnapScrnPoint))
                    {
                        State = States.RUBBER_BAND_SELECT;
                    }
                    return;

                case States.RUBBER_BAND_SELECT:

                    return;

                case States.START_CREATE:
                    {
                        LastDownPoint = mSnapPoint;

                        CreatingFigure = mDB.NewFigure(CreatingFigType);
                        State = States.CREATING;

                        CreatingFigure.StartCreate(dc);


                        CadVector p = dc.UnitPointToCadPoint(mSnapScrnPoint);

                        SetPointInCreating(dc, p);
                        Draw(dc);
                    }
                    break;

                case States.CREATING:
                    {
                        LastDownPoint = mSnapPoint;

                        CadVector p = dc.UnitPointToCadPoint(mSnapScrnPoint);

                        SetPointInCreating(dc, p);
                        Draw(dc);
                    }
                    break;

                case States.MEASURING:
                    {
                        LastDownPoint = mSnapPoint;
                        CadVector p = dc.UnitPointToCadPoint(mSnapScrnPoint);

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
                InteractOut.println("(" + cnt.ToString() + ") " + (d / 10.0).ToString() + "cm");
            }
            else
            {
                InteractOut.println("(" + cnt.ToString() + ") " + d.ToString() + "mm");
            }
        }

        private void MButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            StoreViewOrg = dc.ViewOrg;
        }

        private void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (pointer.MDownPoint.x == x && pointer.MDownPoint.y == y)
            {
                AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
            }
        }

        private void MDrag(CadMouse pointer, DrawContext dc, double x, double y)
        {
            CadVector cp = default(CadVector);
            cp.Set(x, y, 0);

            CadVector d = cp - pointer.MDownPoint;

            CadVector op = StoreViewOrg + d;

            SetOrigin(dc, (int)op.x, (int)op.y);
        }

        private void Wheel(CadMouse pointer, DrawContext dc, double x, double y, int delta)
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

        private void RButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            DrawAll(dc);

            if (RequestContextMenu != null)
            {
                StateInfo si = default(StateInfo);
                si.set(this);
                RequestContextMenu(this, si, (int)x, (int)y);
            }
        }

        public void RubberBandSelect(CadVector p0, CadVector p1)
        {
            mSelList.clear();

            CadVector minp = CadVector.Min(p0, p1);
            CadVector maxp = CadVector.Max(p0, p1);

            DB.WalkEditable(
                (layer, fig) =>
                {
                    SelectIfContactRect(minp, maxp, layer.ID, fig, mSelList);
                });

            CollectSelList(mSelList);
        }

        public void CollectSelList(SelectList selList)
        {
            foreach (CadLayer layer in DB.LayerList)
            {
                if (layer.Locked || layer.Visible == false)
                {
                    continue;
                }

                foreach (CadFigure fig in layer.FigureList)
                {
                    for (int i = 0; i < fig.PointCount; i++)
                    {
                        if (fig.PointList[i].Selected)
                        {
                            selList.add(layer.ID, fig, i);
                        }
                    }
                }
            }
        }

        public void SelectIfContactRect(CadVector minp, CadVector maxp, uint layerID, CadFigure fig, SelectList selList)
        {
            for (int i = 0; i < fig.PointCount; i++)
            {
                CadVector p = CurrentDC.CadPointToUnitPoint(fig.PointList[i]);

                if (CadUtil.IsInRect2D(minp, maxp, p))
                {
                    fig.SelectPointAt(i, true);
                }
            }
            return;
        }

        private void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            //Log.d("LUp");
            switch (State)
            {
                case States.SELECT:
                    break;

                case States.RUBBER_BAND_SELECT:
                    RubberBandSelect(RubberBandScrnPoint0, RubberBandScrnPoint1);

                    State = States.SELECT;
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
            //mOffsetWorld = default(CadVector);
        }

        private void RButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
        }

        private void PointerMoved(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if ((Control.MouseButtons & MouseButtons.Middle) != 0)
            {
                MDrag(pointer, dc, x, y);
                return;
            }

            if (CursorLocked)
            {
                x = CrossCursor.Pos.x;
                y = CrossCursor.Pos.y;
            }

            if (State == States.START_DRAGING_POINTS)
            {
                State = States.DRAGING_POINTS;
                StartEdit();
            }

            CadVector pixp = CadVector.Create(x, y, 0);
            CadVector cp = dc.UnitPointToCadPoint(pixp);
            CadVector tp = default(CadVector);


            RubberBandScrnPoint1 = pixp;

            bool xmatch = false;
            bool ymatch = false;
            bool segmatch = false;

            double dist = CadConst.MaxValue;

            mSnapScrnPoint = pixp - mOffsetScreen;
            mSnapPoint = cp;

            CrossCursor.Pos = mSnapScrnPoint;

            //mSnapScrnPoint.dump(DebugOut.Std);

            Clear(dc);

            if (SnapToPoint)
            {
                mPointSearcher.CleanMatches();
                mPointSearcher.SetRangePixel(dc, PointSnapRange);

                mPointSearcher.SetTargetPoint(CrossCursor);

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

                    #region New snap
                    CadVector distanceX = CrossCursor.DistanceX(tp);

                    CrossCursor.Pos += distanceX;

                    mSnapScrnPoint = CrossCursor.Pos;
                    #endregion

                    mSnapPoint = dc.UnitPointToCadPoint(mSnapScrnPoint);

                    dist = (tp - pixp).Norm();

                    xmatch = true;
                }

                if ((my.Flag & MarkPoint.Y_MATCH) != 0)
                {
                    dc.Drawing.DrawHighlightPoint(my.Point);

                    tp = dc.CadPointToUnitPoint(my.Point);

                    #region New snap
                    CadVector distanceY = CrossCursor.DistanceY(tp);

                    CrossCursor.Pos += distanceY;

                    mSnapScrnPoint = CrossCursor.Pos;
                    #endregion

                    mSnapPoint = dc.UnitPointToCadPoint(mSnapScrnPoint);

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

                mSegSearcher.SetTargetPoint(CrossCursor);

                mSegSearcher.SearchAllLayer(dc, mDB);

                MarkSeg markSeg = mSegSearcher.GetMatch();

                if (markSeg.FigureID != 0)
                {
                    if (markSeg.Distance < dist)
                    {
                        CadFigure fig = mDB.GetFigure(markSeg.FigureID);
                        fig.DrawSeg(dc, DrawTools.PEN_MATCH_SEG, markSeg.PtIndexA, markSeg.PtIndexB);

                        CadVector center = markSeg.CenterPoint;

                        CadVector t = dc.CadPointToUnitPoint(center);

                        if ((t - pixp).Norm() < LineSnapRange)
                        {
                            dc.Drawing.DrawHighlightPoint(center);

                            mSnapPoint = center;
                            mSnapScrnPoint = t;
                            mSnapScrnPoint.z = 0;
                        }
                        else
                        {
                            mSnapPoint = markSeg.CrossPoint;
                            mSnapScrnPoint = markSeg.CrossViewPoint;
                            mSnapScrnPoint.z = 0;
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
                    mSnapScrnPoint.x = mGridding.XMatchU.x;
                }

                if (!ymatch && mGridding.YMatchU.Valid)
                {
                    mSnapScrnPoint.y = mGridding.YMatchU.y;
                }

                mSnapPoint = dc.UnitPointToCadPoint(mSnapScrnPoint);
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
                        mSnapScrnPoint = dc.CadPointToUnitPoint(mSnapPoint);
                    }
                }
            }

            switch (State)
            {
                case States.RUBBER_BAND_SELECT:
                    DrawAll(dc);
                    DrawSelRect(dc);
                    break;

                case States.DRAGING_POINTS:
                    {
                        CadVector p0 = dc.UnitPointToCadPoint(mMoveOrgScrnPoint);
                        CadVector p1 = dc.UnitPointToCadPoint(mSnapScrnPoint);

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
                            CadVector p = dc.UnitPointToCadPoint(mSnapScrnPoint);
                            CreatingFigure.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                        }
                        break;
                    }
                case States.MEASURING:
                    {
                        DrawAll(dc);

                        if (MeasureFigure != null)
                        {
                            CadVector p = dc.UnitPointToCadPoint(mSnapScrnPoint);
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

            //mSnapScreenPoint.dump(DebugOut.Std);
        }

        private void LDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            //Log.d("LDrag");
            PointerMoved(pointer, dc, x, y);
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
                CurrentLayer.AddFigure(CreatingFigure);

                NextState();
            }
            else if (state == CadFigure.States.ENOUGH)
            {
                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, CreatingFigure.ID);
                mHistoryManager.foward(ope);
                CurrentLayer.AddFigure(CreatingFigure);
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

        public void SearchNearestPoint()
        {
            SpPointSearcher sps = new SpPointSearcher();
            CadVector sv = sps.search(this, CrossCursor.Pos);

            if (sv.Invalid)
            {
                return;
            }

            LockCursorScrn(sv);

            //CadVector tv = CurrentDC.UnitPointToCadPoint(sv);
            //CadFigure tfig = new CadFigure(CadFigure.Types.POINT);
            //tfig.AddPoint(tv);
            //TempFigureList.Add(tfig);
        }

        public void LockCursorScrn(CadVector p)
        {
            CursorLocked = true;

            mSnapScrnPoint = p;
            mSnapPoint = CurrentDC.UnitPointToCadPoint(p);
            CrossCursor.Pos = p;
        }
    }
}
