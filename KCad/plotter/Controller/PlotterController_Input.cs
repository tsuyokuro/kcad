#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using CadDataTypes;
using KCad;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public GideLineManager GideLines = new GideLineManager();

        public InteractCtrl mInteractCtrl = new InteractCtrl();

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


        private CadVector StoreViewOrg = default;

        private CadVector mSnapPoint;

        private CadVector MoveOrgScrnPoint;

        // 生のL button down point (デバイス座標系)
        private CadVector RawDownPoint = default;

        // Snap等で補正された L button down point (Wold座標系)
        public CadVector LastDownPoint = default;

        // 選択したObjectの点の座標 (Wold座標系)
        private CadVector ObjDownPoint = default;

        // 実際のMouse座標からCross cursorへのOffset
        private CadVector mOffsetScreen = default;

        public CadVector RubberBandScrnPoint0 = CadVector.InvalidValue;

        public CadVector RubberBandScrnPoint1 = default;

        private CadFigure mCurrentFigure = null;

        public CadFigure CurrentFigure
        {
            set
            {
                if (mCurrentFigure != null)
                {
                    mCurrentFigure.Current = false;
                }

                mCurrentFigure = value;

                if (mCurrentFigure != null)
                {
                    mCurrentFigure.Current = true;
                }
            }

            get
            {
                return mCurrentFigure;
            }
        }

        private bool CursorLocked = false;

        private List<HighlightPointListItem> HighlightPointList = new List<HighlightPointListItem>();


        private Gridding mGridding = new Gridding();

        public Gridding Grid
        {
            get
            {
                return mGridding;
            }
        }


        private void InitHid()
        {
            Mouse.LButtonDown = LButtonDown;
            Mouse.LButtonUp = LButtonUp;

            Mouse.RButtonDown = RButtonDown;
            Mouse.RButtonUp = RButtonUp;

            Mouse.MButtonDown = MButtonDown;
            Mouse.MButtonUp = MButtonUp;

            Mouse.PointerMoved = MouseMove;

            Mouse.Wheel = Wheel;
        }

        private bool IsSelected(MarkPoint mp)
        {
            if (SelectMode == SelectModes.POINT)
            {
                return SelList.isSelected(mp);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                return SelList.isSelectedFigure(mp.FigureID);
            }

            return false;
        }

        private bool IsSelectedSeg(MarkSeg ms)
        {
            if (SelectMode == SelectModes.POINT)
            {
                return SelSegList.isSelected(ms);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                return SelSegList.isSelectedFigure(ms.FigureID);
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

        private struct SelectContext
        {
            public DrawContext DC;
            public CadVector CursorScrPt;
            public CadVector CursorWoldPt;
            public CadCursor Cursor;

            public bool PointSelected;
            public MarkPoint MarkPt;

            public bool SegmentSelected;
            public MarkSeg mseg;
        }

        public bool SelectNearest(DrawContext dc, CadVector pixp)
        {
            SelectContext sc = default;

            ObjDownPoint = CadVector.InvalidValue;

            sc.DC = dc;
            sc.CursorWoldPt = dc.UnitPointToCadPoint(pixp);
            sc.PointSelected = false;
            sc.SegmentSelected = false;

            sc.CursorScrPt = pixp;
            sc.Cursor = CadCursor.Create(pixp);

            sc = PointSelectNearest(sc);

            if (!sc.PointSelected)
            {
                sc = SegSelectNearest(sc);

                if (!sc.SegmentSelected)
                {
                    if (!CadKeyboard.IsCtrlKeyDown())
                    {
                        ClearSelection();
                    }
                }
            }

            if (ObjDownPoint.Valid)
            {
                LastDownPoint = ObjDownPoint;

                CrossCursor.Pos = dc.CadPointToUnitPoint(ObjDownPoint);

                //DebugOut.println("SelectNearest ObjDownPoint.Valid");

                // LastDownPointを投影面上にしたい場合は、こちら
                //LastDownPoint = mSnapPoint;
            }
            else
            {
                LastDownPoint = mSnapPoint;

                if (SettingsHolder.Settings.SnapToGrid)
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
            }

            return sc.PointSelected || sc.SegmentSelected;
        }

        private SelectContext PointSelectNearest(SelectContext sc)
        {
            mPointSearcher.Clean();
            mPointSearcher.SetRangePixel(sc.DC, PointSnapRange);

            if (CurrentFigure != null)
            {
                mPointSearcher.CheckFigure(sc.DC, CurrentLayer, CurrentFigure);
            }

            mPointSearcher.SetTargetPoint(sc.Cursor);

            mPointSearcher.SearchAllLayer(sc.DC, mDB);

            sc.MarkPt = mPointSearcher.GetXYMatch();

            if (sc.MarkPt.FigureID == 0)
            {
                return sc;
            }

            ObjDownPoint = sc.MarkPt.Point;

            MoveOrgScrnPoint = sc.DC.CadPointToUnitPoint(sc.MarkPt.Point);

            MoveOrgScrnPoint.z = 0;

            State = States.START_DRAGING_POINTS;
            CadFigure fig = mDB.GetFigure(sc.MarkPt.FigureID);

            CadLayer layer = mDB.GetLayer(sc.MarkPt.LayerID);

            if (layer.Locked)
            {
                sc.MarkPt.reset();
                return sc;
            }

            ClearSelListConditional(sc.MarkPt);

            if (SelectMode == SelectModes.POINT)
            {
                SelList.add(sc.MarkPt);
                sc.PointSelected = true;
                fig.SelectPointAt(sc.MarkPt.PointIndex, true);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                SelList.add(sc.MarkPt.LayerID, mDB.GetFigure(sc.MarkPt.FigureID));
                sc.PointSelected = true;
                fig.SelectWithGroup();
            }

            // Set ignore list for snap cursor
            mPointSearcher.SetIgnoreList(SelList.List);
            mSegSearcher.SetIgnoreList(SelList.List);

            mRulerSet.Set(fig.PointList, sc.MarkPt.PointIndex, sc.CursorWoldPt);

            CurrentFigure = fig;

            return sc;
        }

        private SelectContext SegSelectNearest(SelectContext sc)
        {
            mSegSearcher.Clean();
            mSegSearcher.SetRangePixel(sc.DC, LineSnapRange);
            mSegSearcher.SetTargetPoint(sc.Cursor);

            mSegSearcher.SearchAllLayer(sc.DC, mDB);

            sc.mseg = mSegSearcher.GetMatch();

            if (sc.mseg.FigureID == 0)
            {
                return sc;
            }

            CadLayer layer = mDB.GetLayer(sc.mseg.LayerID);

            if (layer.Locked)
            {
                sc.mseg.FSegment.Figure = null;
                return sc;
            }

            CadVector center = sc.mseg.CenterPoint;

            CadVector t = sc.DC.CadPointToUnitPoint(center);

            if ((t - sc.CursorScrPt).Norm() < LineSnapRange)
            {
                ObjDownPoint = center;
            }
            else
            {
                ObjDownPoint = sc.mseg.CrossPoint;
            }


            CadFigure fig = mDB.GetFigure(sc.mseg.FigureID);

            ClearSelListConditional(sc.mseg);

            if (SelectMode == SelectModes.POINT)
            {
                SelList.add(sc.mseg.LayerID, mDB.GetFigure(sc.mseg.FigureID), sc.mseg.PtIndexA, sc.mseg.PtIndexB);
                sc.SegmentSelected = true;

                fig.SelectPointAt(sc.mseg.PtIndexA, true);
                fig.SelectPointAt(sc.mseg.PtIndexB, true);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                SelList.add(sc.mseg.LayerID, mDB.GetFigure(sc.mseg.FigureID));
                sc.SegmentSelected = true;

                fig.SelectWithGroup();
            }

            SelSegList.Add(sc.mseg);

            MoveOrgScrnPoint = sc.DC.CadPointToUnitPoint(ObjDownPoint);

            State = States.START_DRAGING_POINTS;

            // Set ignore liset for snap cursor
            mPointSearcher.SetIgnoreList(SelList.List);
            mSegSearcher.SetIgnoreList(SelList.List);
            mSegSearcher.SetIgnoreSeg(SelSegList.List);

            CurrentFigure = fig;

            return sc;
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

            RawDownPoint = pixp;

            RubberBandScrnPoint1 = pixp;
            RubberBandScrnPoint0 = pixp;

            mOffsetScreen = pixp - CrossCursor.Pos;

            if (mInteractCtrl.CurrentMode != InteractCtrl.Mode.NONE)
            {
                mInteractCtrl.Draw(dc, mSnapPoint);
                mInteractCtrl.SetPoint(mSnapPoint);
                return;
            }

            switch (State)
            {
                case States.SELECT:
                    if (SelectNearest(dc, CrossCursor.Pos))
                    {
                        mOffsetScreen = pixp - CrossCursor.Pos;
                    }
                    else
                    {
                        State = States.RUBBER_BAND_SELECT;
                    }

                    return;

                case States.RUBBER_BAND_SELECT:

                    return;

                case States.START_CREATE:
                    {
                        LastDownPoint = mSnapPoint;

                        CadFigure fig = mDB.NewFigure(CreatingFigType);

                        FigureCreator = CadFigure.Creator.Get(CreatingFigType, fig);

                        State = States.CREATING;

                        FigureCreator.StartCreate(dc);


                        CadVector p = dc.UnitPointToCadPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, p);
                    }
                    break;

                case States.CREATING:
                    {
                        LastDownPoint = mSnapPoint;

                        CadVector p = dc.UnitPointToCadPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, p);
                    }
                    break;

                case States.MEASURING:
                    {
                        LastDownPoint = mSnapPoint;
                        CadVector p = dc.UnitPointToCadPoint(CrossCursor.Pos);

                        SetPointInMeasuring(dc, p);
                        PutMeasure();
                    }
                    break;

                default:
                    break;

            }

            Observer.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }

        private void PutMeasure()
        {
            int pcnt = MeasureFigureCreator.Figure.PointCount;

            double currentD = 0;
                       
            if (pcnt > 1)
            {
                int idx0 = pcnt - 1;
                int idx1 = pcnt;

                CadVector p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
                CadVector p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

                currentD = (p1 - p0).Norm();
                currentD = Math.Round(currentD, 4);
            }

            double a = 0;

            if (pcnt > 2)
            {
                CadVector p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
                CadVector p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 3);
                CadVector p2 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

                CadVector v1 = p1 - p0;
                CadVector v2 = p2 - p0;

                double t = CadMath.AngleOfVector(v1, v2);
                a = CadMath.Rad2Deg(t);
                a = Math.Round(a, 4);
            }

            double totalD = CadUtil.AroundLength(MeasureFigureCreator.Figure);

            totalD = Math.Round(totalD, 4);

            int cnt = MeasureFigureCreator.Figure.PointCount;

            InteractOut.println("[" + cnt.ToString() + "]" +
                AnsiEsc.Reset + " LEN:" + AnsiEsc.BGreen + currentD.ToString() +
                AnsiEsc.Reset + " ANGLE:" + AnsiEsc.BBlue + a.ToString() +
                AnsiEsc.Reset + " TOTAL:" + totalD.ToString());
        }

        private void MButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            StoreViewOrg = dc.ViewOrg;
        }

        private void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (pointer.MDownPoint.x == x && pointer.MDownPoint.y == y)
            {
                ViewCtrl.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
                Redraw();
            }
        }

        private void MDrag(CadMouse pointer, DrawContext dc, double x, double y)
        {
            CadVector cp = default;
            cp.Set(x, y, 0);

            CadVector d = cp - pointer.MDownPoint;

            CadVector op = StoreViewOrg + d;

            ViewCtrl.SetOrigin(dc, (int)op.x, (int)op.y);
            Redraw();
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

                ViewCtrl.DpiUpDown(dc, f);
                Redraw();
            }
        }

        private void RButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            DrawAll(dc);

            RequestContextMenu(x, y);
        }

        #region RubberBand
        public void RubberBandSelect(CadVector p0, CadVector p1)
        {
            SelList.clear();

            CadVector minp = CadVector.Min(p0, p1);
            CadVector maxp = CadVector.Max(p0, p1);

            DB.WalkEditable(
                (layer, fig) =>
                {
                    SelectIfContactRect(minp, maxp, layer.ID, fig, SelList);
                });
        }

        public void SelectIfContactRect(CadVector minp, CadVector maxp, uint layerID, CadFigure fig, SelectList selList)
        {
            for (int i = 0; i < fig.PointCount; i++)
            {
                CadVector p = CurrentDC.CadPointToUnitPoint(fig.PointList[i]);

                if (CadUtil.IsInRect2D(minp, maxp, p))
                {
                    fig.SelectPointAt(i, true);

                    if (selList != null)
                    {
                        selList.add(layerID, fig, i);
                    }
                }
            }
            return;
        }
        #endregion

        private void LButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
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

            UpdateTreeView(false);

            mOffsetScreen = default;
        }

        private void RButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
        }

        private void PointSnap(DrawContext dc)
        {
            mPointSearcher.Clean();
            mPointSearcher.SetRangePixel(dc, PointSnapRange);

            mPointSearcher.SetTargetPoint(CrossCursor);

            // (0, 0, 0)にスナップするようにする
            if (SettingsHolder.Settings.SnapToZero)
            {
                mPointSearcher.Check(dc, CadVector.Zero);
            }

            // 複数の点が必要な図形を作成中、最初の点が入力された状態では、
            // オブジェクトがまだ作成されていない。このため、別途チェックする
            if (FigureCreator != null)
            {
                if (FigureCreator.Figure.PointCount == 1)
                {
                    mPointSearcher.Check(dc, FigureCreator.Figure.GetPointAt(0));
                }
            }

            // 計測用オブジェクトの点のチェック
            if (MeasureFigureCreator != null)
            {
                mPointSearcher.Check(dc, MeasureFigureCreator.Figure.PointList);
            }

            // Search point
            mPointSearcher.SearchAllLayer(dc, mDB);

            MarkPoint mxy = mPointSearcher.GetXYMatch();
            MarkPoint mx = mPointSearcher.GetXMatch();
            MarkPoint my = mPointSearcher.GetYMatch();

            CadVector tp = default;

            if (mx.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mx.Point));

                tp = dc.CadPointToUnitPoint(mx.Point);

                CadVector distanceX = CrossCursor.DistanceX(tp);

                CrossCursor.Pos += distanceX;

                mSnapPoint = dc.UnitPointToCadPoint(CrossCursor.Pos);
            }

            if (my.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(my.Point));

                tp = dc.CadPointToUnitPoint(my.Point);

                CadVector distanceY = CrossCursor.DistanceY(tp);

                CrossCursor.Pos += distanceY;

                mSnapPoint = dc.UnitPointToCadPoint(CrossCursor.Pos);
            }

            if (mxy.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mxy.Point, DrawTools.PEN_POINT_HIGHTLITE2));
                tp = dc.CadPointToUnitPoint(mx.Point);

                mSnapPoint = mxy.Point;
                CrossCursor.Pos = dc.CadPointToUnitPoint(mSnapPoint);
            }
        }

        private void SegSnap(DrawContext dc, double dist)
        {
            mSegSearcher.Clean();

            mSegSearcher.SetRangePixel(dc, LineSnapRange);

            mSegSearcher.SetTargetPoint(CrossCursor);

            mSegSearcher.SearchAllLayer(dc, mDB);

            MarkSeg markSeg = mSegSearcher.GetMatch();

            if (mSegSearcher.IsMatch)
            {
                if (markSeg.Distance < dist)
                {
                    CadFigure fig = mDB.GetFigure(markSeg.FigureID);
                    fig.DrawSeg(dc, DrawTools.PEN_MATCH_SEG, markSeg.PtIndexA, markSeg.PtIndexB);

                    CadVector center = markSeg.CenterPoint;

                    CadVector t = dc.CadPointToUnitPoint(center);

                    if ((t - CrossCursor.Pos).Norm() < LineSnapRange)
                    {
                        HighlightPointList.Add(new HighlightPointListItem(center));

                        mSnapPoint = center;

                        CrossCursor.Pos = t;
                        CrossCursor.Pos.z = 0;
                    }
                    else
                    {
                        mSnapPoint = markSeg.CrossPoint;

                        CrossCursor.Pos = markSeg.CrossPointScrn;
                        CrossCursor.Pos.z = 0;
                    }
                }
                else
                {
                    mSegSearcher.Clean();
                }
            }
        }

        private void SnapGrid(DrawContext dc, CadVector pixp)
        {
            mGridding.Clear();
            mGridding.Check(dc, pixp);

            bool snapx = false;
            bool snapy = false;

            if (!mPointSearcher.IsXMatch && mGridding.XMatchU.Valid)
            {
                CrossCursor.Pos.x = mGridding.XMatchU.x;
                snapx = true;
            }

            if (!mPointSearcher.IsYMatch && mGridding.YMatchU.Valid)
            {
                CrossCursor.Pos.y = mGridding.YMatchU.y;
                snapy = true;
            }

            mSnapPoint = dc.UnitPointToCadPoint(CrossCursor.Pos);

            if (snapx && snapy)
            {
                HighlightPointList.Add(new HighlightPointListItem(mSnapPoint));
            }
        }

        private void SnapLine(DrawContext dc, CadVector cp)
        {
            if (!mPointSearcher.IsXMatch && !mPointSearcher.IsYMatch)
            {
                RulerInfo ri = mRulerSet.Capture(dc, cp, LineSnapRange);

                if (ri.IsValid)
                {
                    mSnapPoint = ri.CrossPoint;
                    CrossCursor.Pos = dc.CadPointToUnitPoint(mSnapPoint);
                    HighlightPointList.Add(new HighlightPointListItem(ri.Ruler.P1));
                    HighlightPointList.Add(new HighlightPointListItem(ri.CrossPoint));
                }
            }
        }

        private void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
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
                //
                // 選択時に思わずずらしてしまうことを防ぐため、
                // 最初だけある程度ずらさないと移動しないようにする
                //
                CadVector v = CadVector.Create(x, y, 0);
                double d = (RawDownPoint - v).Norm();

                if (d > SettingsHolder.Settings.InitialMoveLimit)
                {
                    State = States.DRAGING_POINTS;
                    StartEdit();
                }
            }

            CadVector pixp = CadVector.Create(x, y, 0) - mOffsetScreen;
            CadVector cp = dc.UnitPointToCadPoint(pixp);

            if (State == States.DRAGING_POINTS)
            {
                //mOffsetScreen.dump("Offset");

                if (GideLines.Enabled)
                {
                    cp = GideLines.GetOnGideLine(LastDownPoint, cp);
                    pixp = dc.CadPointToUnitPoint(cp);

                    mOffsetScreen = CadVector.Zero;
                }
            }

            RubberBandScrnPoint1 = pixp;

            CrossCursor.Pos = pixp;
            mSnapPoint = cp;

            HighlightPointList.Clear();

            if (SettingsHolder.Settings.SnapToPoint)
            {
                PointSnap(dc);
            }

            if (SettingsHolder.Settings.SnapToSegment)
            {
                SegSnap(dc, mPointSearcher.Distance());
            }

            if (SettingsHolder.Settings.SnapToGrid)
            {
                if (!mPointSearcher.IsXYMatch && !mSegSearcher.IsMatch)
                {
                    SnapGrid(dc, CrossCursor.Pos);
                }
            }

            if (SettingsHolder.Settings.SnapToLine)
            {
                SnapLine(dc, cp);
            }

            switch (State)
            {
                case States.DRAGING_POINTS:
                    {
                        CadVector p0 = dc.UnitPointToCadPoint(MoveOrgScrnPoint);
                        CadVector p1 = dc.UnitPointToCadPoint(CrossCursor.Pos);

                        CadVector delta = p1 - p0;

                        MoveSelectedPoints(dc, delta);

                        break;
                    }
            }

            Observer.CursorPosChanged(this, mSnapPoint, CursorType.TRACKING);
            Observer.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }

        private void LDrag(CadMouse pointer, DrawContext dc, int x, int y)
        {
            MouseMove(pointer, dc, x, y);
        }

        public void DrawAccordingState(DrawContext dc)
        {
            switch (State)
            {
                case States.SELECT:
                    break;

                case States.START_DRAGING_POINTS:
                    break;

                case States.RUBBER_BAND_SELECT:
                    DrawSelRect(dc);
                    break;

                case States.DRAGING_POINTS:
                    break;

                case States.START_CREATE:
                    break;

                case States.CREATING:
                    if (FigureCreator != null)
                    {
                        CadVector p = dc.UnitPointToCadPoint(CrossCursor.Pos);
                        FigureCreator.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                    }
                    break;

                case States.MEASURING:
                    if (MeasureFigureCreator != null)
                    {
                        CadVector p = dc.UnitPointToCadPoint(CrossCursor.Pos);
                        MeasureFigureCreator.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                    }
                    break;
            }

            if (mInteractCtrl.CurrentMode != InteractCtrl.Mode.NONE)
            {
                mInteractCtrl.Draw(dc, mSnapPoint);
            }
        }

        public void DrawHighlightPoint(DrawContext dc)
        {
            HighlightPointList.ForEach(item =>
            {
                dc.Drawing.DrawHighlightPoint(item.Point, item.Pen);
            });
        }


        private void SetPointInCreating(DrawContext dc, CadVector p)
        {
            FigureCreator.AddPointInCreating(dc, p);

            CadFigure.Creator.State state = FigureCreator.GetCreateState();

            if (state == CadFigure.Creator.State.FULL)
            {
                FigureCreator.EndCreate(dc);

                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, FigureCreator.Figure.ID);
                HistoryMan.foward(ope);
                CurrentLayer.AddFigure(FigureCreator.Figure);

                NextState();
            }
            else if (state == CadFigure.Creator.State.ENOUGH)
            {
                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, FigureCreator.Figure.ID);
                HistoryMan.foward(ope);
                CurrentLayer.AddFigure(FigureCreator.Figure);
            }
            else if (state == CadFigure.Creator.State.WAIT_NEXT_POINT)
            {
                CadOpe ope = CadOpe.CreateAddPointOpe(
                    CurrentLayer.ID,
                    FigureCreator.Figure.ID,
                    FigureCreator.Figure.PointCount - 1,
                    ref p
                    );

                HistoryMan.foward(ope);
            }
        }

        private void SetPointInMeasuring(DrawContext dc, CadVector p)
        {
            MeasureFigureCreator.AddPointInCreating(dc, p);
        }

        public void MoveCursorNearestPoint(DrawContext dc)
        {
            SpPointSearcher sps = new SpPointSearcher();
            CadVector sv = sps.Search(this, CrossCursor.Pos);

            if (sv.Invalid)
            {
                return;
            }

            LockCursorScrn(sv);

            Mouse.MouseMove(dc, sv.x, sv.y);
        }

        public void LockCursorScrn(CadVector p)
        {
            CursorLocked = true;

            CrossCursor.Pos = p;
            mSnapPoint = CurrentDC.UnitPointToCadPoint(p);
            CrossCursor.Pos = p;
        }
    }
}
