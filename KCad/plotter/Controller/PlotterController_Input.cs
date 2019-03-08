#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CadDataTypes;
using KCad;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public InteractCtrl mInteractCtrl = new InteractCtrl();

        public CadMouse Mouse { get; } = new CadMouse();

        public CadCursor CrossCursor = CadCursor.Create();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();

        private ItemCursor<NearPointSearcher.Result> mSpPointList = null;

        private CadRulerSet RulerSet = new CadRulerSet();


        private CadVector StoreViewOrg = default;

        private CadVector SnapPoint;

        private CadVector MoveOrgScrnPoint;

        // 生のL button down point (デバイス座標系)
        private CadVector RawDownPoint = default;

        // Snap等で補正された L button down point (World座標系)
        public CadVector LastDownPoint = default;

        // 選択したObjectの点の座標 (World座標系)
        private CadVector ObjDownPoint = default;
        private CadVector SObjDownPoint = default;

        // 実際のMouse座標からCross cursorへのOffset
        private CadVector OffsetScreen = default;

        public CadVector RubberBandScrnPoint0 = CadVector.InvalidValue;

        public CadVector RubberBandScrnPoint1 = default;

        private CadFigure mCurrentFigure = null;

        public MarkSegment? LastSelSegment = null;

        public MarkPoint? LastSelPoint = null;


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

        private bool mCursorLocked = false;
        private bool CursorLocked
        {
            set
            {
                mCursorLocked = value;
                Observer.CursorLocked(mCursorLocked);
                if (!mCursorLocked)
                {
                    mSpPointList = null;
                    Observer.ClosePopupMessage();
                }
                else
                {
                    Observer.OpenPopupMessage("Cursor locked", PlotterObserver.MessageType.INFO);
                }
            }

            get => mCursorLocked;
        }

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

        private void ClearSelectionConditional(MarkPoint newSel)
        {
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                if (!newSel.IsSelected())
                {
                    ClearSelection();
                }
            }
        }

        private void ClearSelectionConditional(MarkSegment newSel)
        {
            if (!CadKeyboard.IsCtrlKeyDown())
            {
                if (!newSel.IsSelected())
                {
                    ClearSelection();
                }
            }
        }

        public bool SelectNearest(DrawContext dc, CadVector pixp)
        {
            SelectContext sc = default;

            ObjDownPoint = CadVector.InvalidValue;

            RulerSet.Clear();

            sc.DC = dc;
            sc.CursorWorldPt = dc.DevPointToWorldPoint(pixp);
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
                else
                {
                    //DbgOut.pf(
                    //    "SegSelected fig id:{0} idx0:{1} idx1:{2}\n",
                    //    sc.MarkSeg.FigureID,
                    //    sc.MarkSeg.FSegment.Index0, sc.MarkSeg.FSegment.Index1);
                }
            }

            if (ObjDownPoint.Valid)
            {
                LastDownPoint = ObjDownPoint;

                CrossCursor.Pos = dc.WorldPointToDevPoint(ObjDownPoint);

                //DebugOut.println("SelectNearest ObjDownPoint.Valid");

                // LastDownPointを投影面上にしたい場合は、こちら
                //LastDownPoint = mSnapPoint;
            }
            else
            {
                LastDownPoint = SnapPoint;

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
                        LastDownPoint = dc.DevPointToWorldPoint(p);
                    }
                }
            }

            return sc.PointSelected || sc.SegmentSelected;
        }

        private SelectContext PointSelectNearest(SelectContext sc)
        {
            mPointSearcher.Clean();
            mPointSearcher.SetRangePixel(sc.DC, SettingsHolder.Settings.PointSnapRange);

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

            MoveOrgScrnPoint = sc.DC.WorldPointToDevPoint(sc.MarkPt.Point);

            MoveOrgScrnPoint.z = 0;

            CadFigure fig = mDB.GetFigure(sc.MarkPt.FigureID);

            CadLayer layer = mDB.GetLayer(sc.MarkPt.LayerID);

            if (layer.Locked)
            {
                sc.MarkPt.reset();
                return sc;
            }

            ClearSelectionConditional(sc.MarkPt);

            if (SelectMode == SelectModes.POINT)
            {
                LastSelPoint = sc.MarkPt;

                sc.PointSelected = true;
                fig.SelectPointAt(sc.MarkPt.PointIndex, true);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                LastSelPoint = sc.MarkPt;

                sc.PointSelected = true;
                fig.SelectWithGroup();
            }

            // Set ignore list for snap cursor
            //mPointSearcher.SetIgnoreList(SelList.List);
            //mSegSearcher.SetIgnoreList(SelList.List);

            if (sc.PointSelected)
            {
                RulerSet.Set(sc.MarkPt);
            }

            CurrentFigure = fig;

            return sc;
        }

        private SelectContext SegSelectNearest(SelectContext sc)
        {
            mSegSearcher.Clean();
            mSegSearcher.SetRangePixel(sc.DC, SettingsHolder.Settings.LineSnapRange);
            mSegSearcher.SetTargetPoint(sc.Cursor);

            mSegSearcher.SearchAllLayer(sc.DC, mDB);

            sc.MarkSeg = mSegSearcher.GetMatch();

            if (sc.MarkSeg.FigureID == 0)
            {
                return sc;
            }

            CadLayer layer = mDB.GetLayer(sc.MarkSeg.LayerID);

            if (layer.Locked)
            {
                sc.MarkSeg.FigSeg.Figure = null;
                return sc;
            }

            CadVector center = sc.MarkSeg.CenterPoint;

            CadVector t = sc.DC.WorldPointToDevPoint(center);

            if ((t - sc.CursorScrPt).Norm() < SettingsHolder.Settings.LineSnapRange)
            {
                ObjDownPoint = center;
            }
            else
            {
                ObjDownPoint = sc.MarkSeg.CrossPoint;
            }


            CadFigure fig = mDB.GetFigure(sc.MarkSeg.FigureID);

            ClearSelectionConditional(sc.MarkSeg);

            if (SelectMode == SelectModes.POINT)
            {
                LastSelPoint = null;
                LastSelSegment = sc.MarkSeg;

                sc.SegmentSelected = true;

                fig.SelectPointAt(sc.MarkSeg.PtIndexA, true);
                fig.SelectPointAt(sc.MarkSeg.PtIndexB, true);
            }
            else if (SelectMode == SelectModes.OBJECT)
            {
                sc.SegmentSelected = true;

                LastSelPoint = null;
                LastSelSegment = sc.MarkSeg;

                fig.SelectWithGroup();
            }

            MoveOrgScrnPoint = sc.DC.WorldPointToDevPoint(ObjDownPoint);

            if (sc.SegmentSelected)
            {
                RulerSet.Set(sc.MarkSeg, sc.DC);
            }

            CurrentFigure = fig;

            return sc;
        }

        private void LButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (CursorLocked)
            {
                x = CrossCursor.Pos.x;
                y = CrossCursor.Pos.y;
            }

            //DOut.pl($"LButtonDown {x}, {y}");

            CadVector pixp = CadVector.Create(x, y, 0);
            CadVector cp = dc.DevPointToWorldPoint(pixp);

            RawDownPoint = pixp;

            RubberBandScrnPoint1 = pixp;
            RubberBandScrnPoint0 = pixp;

            OffsetScreen = pixp - CrossCursor.Pos;

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Draw(dc, SnapPoint);
                mInteractCtrl.SetPoint(SnapPoint);

                LastDownPoint = SnapPoint;

                return;
            }

            switch (State)
            {
                case States.SELECT:
                    if (SelectNearest(dc, CrossCursor.Pos))
                    {
                        if (!CursorLocked)
                        {
                            State = States.START_DRAGING_POINTS;
                        }

                        OffsetScreen = pixp - CrossCursor.Pos;

                        SObjDownPoint = ObjDownPoint;
                    }
                    else
                    {
                        State = States.RUBBER_BAND_SELECT;
                    }

                    break;

                case States.RUBBER_BAND_SELECT:

                    break;

                case States.START_CREATE:
                    {
                        LastDownPoint = SnapPoint;

                        CadFigure fig = mDB.NewFigure(CreatingFigType);

                        FigureCreator = CadFigure.Creator.Get(CreatingFigType, fig);

                        State = States.CREATING;

                        FigureCreator.StartCreate(dc);


                        CadVector p = dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, p);
                    }
                    break;

                case States.CREATING:
                    {
                        LastDownPoint = SnapPoint;

                        CadVector p = dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, p);
                    }
                    break;

                case States.MEASURING:
                    {
                        LastDownPoint = SnapPoint;
                        CadVector p = dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInMeasuring(dc, p);
                        PutMeasure();
                    }
                    break;

                default:
                    break;

            }

            if (CursorLocked)
            {
                CursorLocked = false;
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

            ItConsole.println("[" + cnt.ToString() + "]" +
                AnsiEsc.Reset + " LEN:" + AnsiEsc.BGreen + currentD.ToString() +
                AnsiEsc.Reset + " ANGLE:" + AnsiEsc.BBlue + a.ToString() +
                AnsiEsc.Reset + " TOTAL:" + totalD.ToString());
        }

        private void MButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            State = States.DRAGING_VIEW_ORG;

            StoreViewOrg = dc.ViewOrg;
            CursorLocked = false;

            CrossCursor.Store();
        }

        private void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (pointer.MDownPoint.x == x && pointer.MDownPoint.y == y)
            {
                ViewCtrl.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
                Redraw();
            }

            State = States.SELECT;

            CrossCursor.Pos = CadVector.Create(x, y, 0);
        }

        private void ViewOrgDrag(CadMouse pointer, DrawContext dc, double x, double y)
        {
            CadVector cp = default;
            cp.Set(x, y, 0);

            CadVector d = cp - pointer.MDownPoint;

            CadVector op = StoreViewOrg + d;

            ViewCtrl.SetOrigin(dc, (int)op.x, (int)op.y);

            CrossCursor.Pos = CrossCursor.StorePos + d;

            Redraw();
        }

        private void Wheel(CadMouse pointer, DrawContext dc, double x, double y, int delta)
        {
            if (CadKeyboard.IsCtrlKeyDown())
            {
                CursorLocked = false;

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
            LastSelPoint = null;
            LastSelSegment = null;

            CadVector minp = CadVector.Min(p0, p1);
            CadVector maxp = CadVector.Max(p0, p1);

            DB.WalkEditable(
                (layer, fig) =>
                {
                    SelectIfContactRect(minp, maxp, layer, fig);
                });
        }

        public void SelectIfContactRect(CadVector minp, CadVector maxp, CadLayer layer, CadFigure fig)
        {
            for (int i = 0; i < fig.PointCount; i++)
            {
                CadVector p = CurrentDC.WorldPointToDevPoint(fig.PointList[i]);

                if (CadUtil.IsInRect2D(minp, maxp, p))
                {
                    fig.SelectPointAt(i, true);
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

                    //mPointSearcher.SetIgnoreList(null);
                    //mSegSearcher.SetIgnoreList(null);
                    //mSegSearcher.SetIgnoreSeg(null);

                    if (State == States.DRAGING_POINTS)
                    {
                        EndEdit();
                    }

                    State = States.SELECT;
                    break;
            }

            UpdateTreeView(false);

            OffsetScreen = default;
        }

        private void RButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
        }

        private void PointSnap(DrawContext dc)
        {
            // 複数の点が必要な図形を作成中、最初の点が入力された状態では、
            // オブジェクトがまだ作成されていない。このため、別途チェックする
            if (FigureCreator != null)
            {
                if (FigureCreator.Figure.PointCount == 1)
                {
                    mPointSearcher.Check(dc, FigureCreator.Figure.GetPointAt(0));
                }
            }

            if (mInteractCtrl.IsActive)
            {
                foreach (CadVector v in mInteractCtrl.PointList)
                {
                    mPointSearcher.Check(dc, v);
                }
            }

            // 計測用オブジェクトの点のチェック
                if (MeasureFigureCreator != null)
            {
                mPointSearcher.Check(dc, MeasureFigureCreator.Figure.PointList);
            }

            // Search point
            mPointSearcher.SearchAllLayer(dc, mDB);
        }

        private void EvalPointSearcher(DrawContext dc)
        {
            MarkPoint mxy = mPointSearcher.GetXYMatch();
            MarkPoint mx = mPointSearcher.GetXMatch();
            MarkPoint my = mPointSearcher.GetYMatch();

            CadVector tp = default;

            if (mx.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mx.Point));

                tp = dc.WorldPointToDevPoint(mx.Point);

                CadVector distanceX = CrossCursor.DistanceX(tp);

                CrossCursor.Pos += distanceX;

                SnapPoint = dc.DevPointToWorldPoint(CrossCursor.Pos);
            }

            if (my.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(my.Point));

                tp = dc.WorldPointToDevPoint(my.Point);

                CadVector distanceY = CrossCursor.DistanceY(tp);

                CrossCursor.Pos += distanceY;

                SnapPoint = dc.DevPointToWorldPoint(CrossCursor.Pos);
            }

            if (mxy.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mxy.Point, DrawTools.PEN_POINT_HIGHLIGHT2));
                tp = dc.WorldPointToDevPoint(mx.Point);

                SnapPoint = mxy.Point;
                CrossCursor.Pos = dc.WorldPointToDevPoint(SnapPoint);
            }
        }

        private void SegSnap(DrawContext dc)
        {
            mSegSearcher.SearchAllLayer(dc, mDB);
        }

        private void EvalSegSeracher(DrawContext dc, double dist)
        {
            MarkSegment markSeg = mSegSearcher.GetMatch();

            if (mSegSearcher.IsMatch)
            {
                if (markSeg.Distance < dist)
                {
                    CadFigure fig = mDB.GetFigure(markSeg.FigureID);
                    fig.DrawSeg(dc, DrawTools.PEN_MATCH_SEG, markSeg.PtIndexA, markSeg.PtIndexB);

                    CadVector center = markSeg.CenterPoint;

                    CadVector t = dc.WorldPointToDevPoint(center);

                    if ((t - CrossCursor.Pos).Norm() < SettingsHolder.Settings.LineSnapRange)
                    {
                        HighlightPointList.Add(new HighlightPointListItem(center));

                        SnapPoint = center;

                        CrossCursor.Pos = t;
                        CrossCursor.Pos.z = 0;
                    }
                    else
                    {
                        SnapPoint = markSeg.CrossPoint;

                        CrossCursor.Pos = markSeg.CrossPointScrn;
                        CrossCursor.Pos.z = 0;

                        HighlightPointList.Add(new HighlightPointListItem(SnapPoint, DrawTools.PEN_LINE_SNAP));
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
            if (mPointSearcher.IsXYMatch || mSegSearcher.IsMatch)
            {
                return;
            }


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

            SnapPoint = dc.DevPointToWorldPoint(CrossCursor.Pos);

            if (snapx && snapy)
            {
                HighlightPointList.Add(new HighlightPointListItem(SnapPoint));
            }
        }

        private void SnapLine(DrawContext dc)
        {
            if (mPointSearcher.IsXYMatch)
            {
                return;
            }

            CadCursor cursor = CrossCursor;

            if (mPointSearcher.IsXMatch)
            {
                cursor.Pos.x = mPointSearcher.GetXMatch().PointScrn.x;
            }

            if (mPointSearcher.IsYMatch)
            {
                cursor.Pos.y = mPointSearcher.GetYMatch().PointScrn.y;
            }

            RulerInfo ri = RulerSet.Capture(dc, cursor, SettingsHolder.Settings.LineSnapRange);

            if (ri.IsValid)
            {
                SnapPoint = ri.CrossPoint;
                CrossCursor.Pos = dc.WorldPointToDevPoint(SnapPoint);

                if (mSegSearcher.IsMatch)
                {
                    MarkSegment ms = mSegSearcher.GetMatch();

                    if (ms.FigureID != ri.Ruler.Fig.ID)
                    {
                        CadVector cp = PlotterUtil.CrossOnScreen(dc, ri.Ruler.P0, ri.Ruler.P1, ms.FigSeg.Point0, ms.FigSeg.Point1);

                        if (cp.Valid)
                        {
                            SnapPoint = dc.DevPointToWorldPoint(cp);
                            CrossCursor.Pos = cp;
                        }
                    }
                }

                HighlightPointList.Add(new HighlightPointListItem(ri.Ruler.P1));
                HighlightPointList.Add(new HighlightPointListItem(ri.CrossPoint));
            }
        }

        private void SnapCursor(DrawContext dc)
        {
            HighlightPointList.Clear();

            mPointSearcher.Clean();
            mPointSearcher.SetRangePixel(dc, SettingsHolder.Settings.PointSnapRange);
            mPointSearcher.SetTargetPoint(CrossCursor);

            // (0, 0, 0)にスナップするようにする
            if (SettingsHolder.Settings.SnapToZero)
            {
                mPointSearcher.Check(dc, CadVector.Zero);
            }

            // 最後にマウスダウンしたポイントにスナップする
            if (SettingsHolder.Settings.SnapToLastDownPoint)
            {
                mPointSearcher.Check(dc, LastDownPoint);
            }

            if (SettingsHolder.Settings.SnapToPoint)
            {
                PointSnap(dc);
            }

            EvalPointSearcher(dc);

            mSegSearcher.Clean();
            mSegSearcher.SetRangePixel(dc, SettingsHolder.Settings.LineSnapRange);
            mSegSearcher.SetTargetPoint(CrossCursor);

            if (SettingsHolder.Settings.SnapToSegment)
            {
                SegSnap(dc);
            }

            EvalSegSeracher(dc, mPointSearcher.Distance());

            if (SettingsHolder.Settings.SnapToGrid)
            {
                SnapGrid(dc, CrossCursor.Pos);
            }

            if (SettingsHolder.Settings.SnapToLine)
            {
                SnapLine(dc);
            }
        }

        private void MouseMove(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (State == States.DRAGING_VIEW_ORG)
            {
                ViewOrgDrag(pointer, dc, x, y);
                return;
            }

            if (CursorLocked)
            {
                x = CrossCursor.Pos.x;
                y = CrossCursor.Pos.y;
            }

            //DOut.pl($"MouseMove {x}, {y}");

            CadVector pixp = CadVector.Create(x, y, 0) - OffsetScreen;
            CadVector cp = dc.DevPointToWorldPoint(pixp);


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

            RubberBandScrnPoint1 = pixp;

            CrossCursor.Pos = pixp;
            SnapPoint = cp;

            if (!CursorLocked)
            {
                SnapCursor(dc);
            }

            if (State == States.DRAGING_POINTS)
            {
                CadVector p0 = dc.DevPointToWorldPoint(MoveOrgScrnPoint);
                CadVector p1 = dc.DevPointToWorldPoint(CrossCursor.Pos);

                CadVector delta = p1 - p0;

                MoveSelectedPoints(dc, delta);

                ObjDownPoint = SObjDownPoint + delta;
            }

            Observer.CursorPosChanged(this, SnapPoint, CursorType.TRACKING);
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
                        CadVector p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        FigureCreator.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                    }
                    break;

                case States.MEASURING:
                    if (MeasureFigureCreator != null)
                    {
                        CadVector p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        MeasureFigureCreator.DrawTemp(dc, p, DrawTools.PEN_TEMP_FIGURE);
                    }
                    break;
            }

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Draw(dc, SnapPoint);
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

                CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, FigureCreator.Figure.ID);
                HistoryMan.foward(ope);
                CurrentLayer.AddFigure(FigureCreator.Figure);

                NextState();
            }
            else if (state == CadFigure.Creator.State.ENOUGH)
            {
                CadOpe ope = new CadOpeAddFigure(CurrentLayer.ID, FigureCreator.Figure.ID);
                HistoryMan.foward(ope);
                CurrentLayer.AddFigure(FigureCreator.Figure);
            }
            else if (state == CadFigure.Creator.State.WAIT_NEXT_POINT)
            {
                CadOpe ope = new CadOpeAddPoint(
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

        public void MoveCursorToNearPoint(DrawContext dc)
        {
            if (mSpPointList == null)
            {
                NearPointSearcher searcher = new NearPointSearcher(this);

                var resList = searcher.Search(CrossCursor.Pos, 64);

                if (resList.Count == 0)
                {
                    return;
                }

                mSpPointList = new ItemCursor<NearPointSearcher.Result>(resList);
            }

            NearPointSearcher.Result res = mSpPointList.LoopNext();

            ItConsole.println(res.ToInfoString());

            CadVector sv = CurrentDC.WorldPointToDevPoint(res.WoldPoint);

            LockCursorScrn(sv);

            Mouse.MouseMove(dc, sv.x, sv.y);
        }

        public void LockCursorScrn(CadVector p)
        {
            CursorLocked = true;

            SnapPoint = CurrentDC.DevPointToWorldPoint(p);
            CrossCursor.Pos = p;
        }

        public void CursorLock()
        {
            CursorLocked = true;
        }

        public void CursorUnlock()
        {
            CursorLocked = false;
        }

        public CadVector GetCursorPos()
        {
            return SnapPoint;
        }

        public void SetCursorWoldPos(CadVector v)
        {
            SnapPoint = v;
            CrossCursor.Pos = CurrentDC.WorldPointToDevPoint(SnapPoint);

            Observer.CursorPosChanged(this, SnapPoint, CursorType.TRACKING);

            Redraw();
        }


        public CadVector GetLastDownPoint()
        {
            return LastDownPoint;
        }

        public void SetLastDownPoint(CadVector v)
        {
            LastDownPoint = v;

            Observer.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);

            Redraw();
        }
    }
}
