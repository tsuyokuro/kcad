#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CadDataTypes;
using KCad;
using OpenTK;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public struct SnapInfo
        {
            public CadCursor Cursor;
            public CadVertex SnapPoint;
            public double Distance;

            public SnapInfo(CadCursor cursor, CadVertex snapPoint, double dist = Double.MaxValue)
            {
                Cursor = cursor;
                SnapPoint = snapPoint;
                Distance = dist;
            }
        }

        public InteractCtrl mInteractCtrl = new InteractCtrl();

        public CadMouse Mouse { get; } = new CadMouse();

        public CadCursor CrossCursor = CadCursor.Create();

        private PointSearcher mPointSearcher = new PointSearcher();

        private SegSearcher mSegSearcher = new SegSearcher();

        private ItemCursor<NearPointSearcher.Result> mSpPointList = null;

        private CadRulerSet RulerSet = new CadRulerSet();


        private Vector3d StoreViewOrg = default;

        private CadVertex SnapPoint;

        private CadVertex MoveOrgScrnPoint;

        // 生のL button down point (デバイス座標系)
        private CadVertex RawDownPoint = default;

        // Snap等で補正された L button down point (World座標系)
        public CadVertex LastDownPoint = default;

        // 選択したObjectの点の座標 (World座標系)
        private CadVertex ObjDownPoint = default;
        private CadVertex SObjDownPoint = default;

        // 実際のMouse座標からCross cursorへのOffset
        private CadVertex OffsetScreen = default;

        public CadVertex RubberBandScrnPoint0 = CadVertex.InvalidValue;

        public CadVertex RubberBandScrnPoint1 = default;

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

        public bool SelectNearest(DrawContext dc, Vector3d pixp)
        {
            SelectContext sc = default;

            ObjDownPoint = CadVertex.InvalidValue;

            RulerSet.Clear();

            sc.DC = dc;
            sc.CursorWorldPt = (CadVertex)dc.DevPointToWorldPoint(pixp);
            sc.PointSelected = false;
            sc.SegmentSelected = false;

            sc.CursorScrPt = (CadVertex)pixp;
            sc.Cursor = CadCursor.Create((CadVertex)pixp);

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
                    Vector3d p = pixp;

                    bool match = false;

                    mGridding.Clear();
                    mGridding.Check(dc, pixp);

                    if (mGridding.XMatchU.IsValid())
                    {
                        p.X = mGridding.XMatchU.X;
                        match = true;
                    }

                    if (mGridding.YMatchU.IsValid())
                    {
                        p.Y = mGridding.YMatchU.Y;
                        match = true;
                    }

                    if (match)
                    {
                        LastDownPoint = (CadVertex)dc.DevPointToWorldPoint(p);
                    }
                }
            }

            return sc.PointSelected || sc.SegmentSelected;
        }

        private SelectContext PointSelectNearest(SelectContext sc)
        {
            mPointSearcher.Clean();
            mPointSearcher.SetRangePixel(sc.DC, SettingsHolder.Settings.PointSnapRange);
            mPointSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;

            if (CurrentFigure != null)
            {
                mPointSearcher.CheckFigure(sc.DC, CurrentLayer, CurrentFigure);
            }

            //sc.Cursor.Pos.dump("CursorPos");

            mPointSearcher.SetTargetPoint(sc.Cursor);

            mPointSearcher.SearchAllLayer(sc.DC, mDB);

            sc.MarkPt = mPointSearcher.GetXYMatch();

            //sc.MarkPt.dump();

            if (sc.MarkPt.FigureID == 0)
            {
                return sc;
            }

            ObjDownPoint = sc.MarkPt.Point;

            MoveOrgScrnPoint = sc.DC.WorldPointToDevPoint(sc.MarkPt.Point);

            MoveOrgScrnPoint.Z = 0;

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

            CadVertex center = sc.MarkSeg.CenterPoint;

            CadVertex t = sc.DC.WorldPointToDevPoint(center);

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
                x = CrossCursor.Pos.X;
                y = CrossCursor.Pos.Y;
            }

            CadVertex pixp = CadVertex.Create(x, y, 0);
            CadVertex cp = dc.DevPointToWorldPoint(pixp);

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
                    if (SelectNearest(dc, (Vector3d)CrossCursor.Pos))
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


                        CadVertex p = dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, p);
                    }
                    break;

                case States.CREATING:
                    {
                        LastDownPoint = SnapPoint;

                        CadVertex p = dc.DevPointToWorldPoint(CrossCursor.Pos);

                        SetPointInCreating(dc, p);
                    }
                    break;

                case States.MEASURING:
                    {
                        LastDownPoint = SnapPoint;
                        CadVertex p = dc.DevPointToWorldPoint(CrossCursor.Pos);

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

                CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
                CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

                currentD = (p1 - p0).Norm();
                currentD = Math.Round(currentD, 4);
            }

            double a = 0;

            if (pcnt > 2)
            {
                CadVertex p0 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 2);
                CadVertex p1 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 3);
                CadVertex p2 = MeasureFigureCreator.Figure.GetPointAt(pcnt - 1);

                Vector3d v1 = p1.vector - p0.vector;
                Vector3d v2 = p2.vector - p0.vector;

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
            mBackState = State;

            State = States.DRAGING_VIEW_ORG;

            StoreViewOrg = dc.ViewOrg;
            CursorLocked = false;

            CrossCursor.Store();

            Observer.ChangeMouseCursor(PlotterObserver.MouseCursorType.HAND);
        }

        private void MButtonUp(CadMouse pointer, DrawContext dc, double x, double y)
        {
            if (pointer.MDownPoint.X == x && pointer.MDownPoint.Y == y)
            {
                ViewCtrl.AdjustOrigin(dc, x, y, (int)dc.ViewWidth, (int)dc.ViewHeight);
                //Redraw();
            }

            State = mBackState;

            CrossCursor.Pos = CadVertex.Create(x, y, 0);

            Observer.ChangeMouseCursor(PlotterObserver.MouseCursorType.CROSS);
        }

        private void ViewOrgDrag(CadMouse pointer, DrawContext dc, double x, double y)
        {
            //DOut.tpl("ViewOrgDrag");

            CadVertex cp = default;
            cp.Set(x, y, 0);

            CadVertex d = cp - pointer.MDownPoint;

            CadVertex op = StoreViewOrg + d;

            ViewCtrl.SetOrigin(dc, (int)op.X, (int)op.Y);

            CrossCursor.Pos = CrossCursor.StorePos + d;

            //Redraw();
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
                //Redraw();
            }
        }

        private void RButtonDown(CadMouse pointer, DrawContext dc, double x, double y)
        {
            DrawAll(dc);

            RequestContextMenu(x, y);
        }

        #region RubberBand
        public void RubberBandSelect(CadVertex p0, CadVertex p1)
        {
            LastSelPoint = null;
            LastSelSegment = null;

            CadVertex minp = CadVertex.Min(p0, p1);
            CadVertex maxp = CadVertex.Max(p0, p1);

            DB.WalkEditable(
                (layer, fig) =>
                {
                    SelectIfContactRect(minp, maxp, layer, fig);
                });
        }

        public void SelectIfContactRect(CadVertex minp, CadVertex maxp, CadLayer layer, CadFigure fig)
        {
            for (int i = 0; i < fig.PointCount; i++)
            {
                CadVertex p = CurrentDC.WorldPointToDevPoint(fig.PointList[i]);

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
                foreach (CadVertex v in mInteractCtrl.PointList)
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

        private SnapInfo EvalPointSearcher(DrawContext dc, SnapInfo si)
        {
            MarkPoint mxy = mPointSearcher.GetXYMatch();
            MarkPoint mx = mPointSearcher.GetXMatch();
            MarkPoint my = mPointSearcher.GetYMatch();

            CadVertex tp = default;

            if (mx.IsValid)
            {
                HighlightPointList.Add(
                    new HighlightPointListItem(mx.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

                tp = dc.WorldPointToDevPoint(mx.Point);

                CadVertex distanceX = si.Cursor.DistanceX(tp);

                si.Cursor.Pos += distanceX;

                si.SnapPoint = dc.DevPointToWorldPoint(si.Cursor.Pos);
            }

            if (my.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(my.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

                tp = dc.WorldPointToDevPoint(my.Point);

                CadVertex distanceY = si.Cursor.DistanceY(tp);

                si.Cursor.Pos += distanceY;

                si.SnapPoint = dc.DevPointToWorldPoint(si.Cursor.Pos);
            }

            if (mxy.IsValid)
            {
                HighlightPointList.Add(new HighlightPointListItem(mxy.Point, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT2)));
                tp = dc.WorldPointToDevPoint(mx.Point);

                si.Cursor.Pos = dc.WorldPointToDevPoint(si.SnapPoint);

                si.SnapPoint = mxy.Point;
            }

            return si;
        }

        private void SegSnap(DrawContext dc)
        {
            mSegSearcher.SearchAllLayer(dc, mDB);
        }

        private SnapInfo EvalSegSeracher(DrawContext dc, SnapInfo si)
        {
            MarkSegment markSeg = mSegSearcher.GetMatch();

            if (mSegSearcher.IsMatch)
            {
                if (markSeg.Distance < si.Distance)
                {
                    CadFigure fig = mDB.GetFigure(markSeg.FigureID);
                    fig.DrawSeg(dc, dc.GetPen(DrawTools.PEN_MATCH_SEG), markSeg.PtIndexA, markSeg.PtIndexB);

                    CadVertex center = markSeg.CenterPoint;

                    CadVertex t = dc.WorldPointToDevPoint(center);

                    if ((t - si.Cursor.Pos).Norm() < SettingsHolder.Settings.LineSnapRange)
                    {
                        HighlightPointList.Add(new HighlightPointListItem(center, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));

                        si.SnapPoint = center;

                        si.Cursor.Pos = t;
                        si.Cursor.Pos.Z = 0;
                    }
                    else
                    {
                        si.SnapPoint = markSeg.CrossPoint;

                        si.Cursor.Pos = markSeg.CrossPointScrn;
                        si.Cursor.Pos.Z = 0;

                        HighlightPointList.Add(new HighlightPointListItem(SnapPoint, dc.GetPen(DrawTools.PEN_LINE_SNAP)));
                    }
                }
                else
                {
                    mSegSearcher.Clean();
                }
            }

            return si;
        }

        private SnapInfo SnapGrid(DrawContext dc, SnapInfo si)
        {
            mGridding.Clear();
            mGridding.Check(dc, (Vector3d)si.Cursor.Pos);

            bool snapx = false;
            bool snapy = false;

            if (!mPointSearcher.IsXMatch && mGridding.XMatchU.IsValid())
            {
                si.Cursor.Pos.X = mGridding.XMatchU.X;
                snapx = true;
            }

            if (!mPointSearcher.IsYMatch && mGridding.YMatchU.IsValid())
            {
                si.Cursor.Pos.Y = mGridding.YMatchU.Y;
                snapy = true;
            }

            si.SnapPoint = dc.DevPointToWorldPoint(si.Cursor.Pos);

            if (snapx && snapy)
            {
                HighlightPointList.Add(new HighlightPointListItem(si.SnapPoint, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));
            }

            return si;
        }

        private SnapInfo SnapLine(DrawContext dc, SnapInfo si)
        {
            if (mPointSearcher.IsXMatch)
            {
                si.Cursor.Pos.X = mPointSearcher.GetXMatch().PointScrn.X;
            }

            if (mPointSearcher.IsYMatch)
            {
                si.Cursor.Pos.Y = mPointSearcher.GetYMatch().PointScrn.Y;
            }

            RulerInfo ri = RulerSet.Capture(dc, si.Cursor, SettingsHolder.Settings.LineSnapRange);

            if (ri.IsValid)
            {
                si.SnapPoint = ri.CrossPoint;
                si.Cursor.Pos = dc.WorldPointToDevPoint(si.SnapPoint);

                if (mSegSearcher.IsMatch)
                {
                    MarkSegment ms = mSegSearcher.GetMatch();

                    if (ms.FigureID != ri.Ruler.Fig.ID)
                    {
                        CadVertex cp = PlotterUtil.CrossOnScreen(dc, ri.Ruler.P0, ri.Ruler.P1, ms.FigSeg.Point0, ms.FigSeg.Point1);

                        if (cp.Valid)
                        {
                            si.SnapPoint = dc.DevPointToWorldPoint(cp);
                            si.Cursor.Pos = cp;
                        }
                    }
                }

                HighlightPointList.Add(new HighlightPointListItem(ri.Ruler.P1, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));
                HighlightPointList.Add(new HighlightPointListItem(ri.CrossPoint, dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT)));
            }

            return si;
        }

        private void SnapCursor(DrawContext dc)
        {
            HighlightPointList.Clear();

            SnapInfo si =
                new SnapInfo(
                    CrossCursor,
                    SnapPoint,
                    mPointSearcher.Distance()
                    );

            #region Point search

            mPointSearcher.Clean();
            mPointSearcher.SetRangePixel(dc, SettingsHolder.Settings.PointSnapRange);
            mPointSearcher.CheckStorePoint = SettingsHolder.Settings.SnapToSelfPoint;
            mPointSearcher.SetTargetPoint(CrossCursor);

            //if (!SettingsHolder.Settings.SnapToSelfPoint)
            //{
            //    DOut.pl("SnapToSelf");

            //    if (CurrentFigure != null)
            //    {
            //        mPointSearcher.AddIgnore(CurrentFigure);
            //    }
            //}

            // (0, 0, 0)にスナップするようにする
            if (SettingsHolder.Settings.SnapToZero)
            {
                mPointSearcher.Check(dc, CadVertex.Zero);
            }

            // 最後にマウスダウンしたポイントにスナップする
            if (SettingsHolder.Settings.SnapToLastDownPoint)
            {
                mPointSearcher.Check(dc, LastDownPoint);
            }

            if (SettingsHolder.Settings.SnapToPoint)
            {
                PointSnap(dc);
                si = EvalPointSearcher(dc, si);
            }

            #endregion

            #region Segment search

            mSegSearcher.Clean();
            mSegSearcher.SetRangePixel(dc, SettingsHolder.Settings.LineSnapRange);
            mSegSearcher.SetTargetPoint(CrossCursor);

            if (SettingsHolder.Settings.SnapToSegment)
            {
                if (!mPointSearcher.IsXYMatch)
                {
                    SegSnap(dc);
                    si = EvalSegSeracher(dc, si);
                }
            }

            #endregion

            if (SettingsHolder.Settings.SnapToGrid)
            {
                if (!mPointSearcher.IsXYMatch && !mSegSearcher.IsMatch)
                {
                    si = SnapGrid(dc, si);
                }
            }

            if (SettingsHolder.Settings.SnapToLine)
            {
                if (!mPointSearcher.IsXYMatch)
                {
                    si = SnapLine(dc, si);
                }
            }

            CrossCursor = si.Cursor;
            SnapPoint = si.SnapPoint;
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
                x = CrossCursor.Pos.X;
                y = CrossCursor.Pos.Y;
            }

            //DOut.pl($"MouseMove {x}, {y}");

            CadVertex pixp = CadVertex.Create(x, y, 0) - OffsetScreen;
            CadVertex cp = dc.DevPointToWorldPoint(pixp);


            if (State == States.START_DRAGING_POINTS)
            {
                //
                // 選択時に思わずずらしてしまうことを防ぐため、
                // 最初だけある程度ずらさないと移動しないようにする
                //
                CadVertex v = CadVertex.Create(x, y, 0);
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
                CadVertex p0 = dc.DevPointToWorldPoint(MoveOrgScrnPoint);
                CadVertex p1 = dc.DevPointToWorldPoint(CrossCursor.Pos);

                CadVertex delta = p1 - p0;

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
                        CadVertex p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        FigureCreator.DrawTemp(dc, p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
                    }
                    break;

                case States.MEASURING:
                    if (MeasureFigureCreator != null)
                    {
                        CadVertex p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        MeasureFigureCreator.DrawTemp(dc, p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
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


        private void SetPointInCreating(DrawContext dc, CadVertex p)
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

        private void SetPointInMeasuring(DrawContext dc, CadVertex p)
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

            CadVertex sv = CurrentDC.WorldPointToDevPoint(res.WoldPoint);

            LockCursorScrn(sv);

            Mouse.MouseMove(dc, sv.X, sv.Y);
        }

        public void LockCursorScrn(CadVertex p)
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

        public CadVertex GetCursorPos()
        {
            return SnapPoint;
        }

        public void SetCursorWoldPos(CadVertex v)
        {
            SnapPoint = v;
            CrossCursor.Pos = CurrentDC.WorldPointToDevPoint(SnapPoint);

            Observer.CursorPosChanged(this, SnapPoint, CursorType.TRACKING);
        }


        public CadVertex GetLastDownPoint()
        {
            return LastDownPoint;
        }

        public void SetLastDownPoint(CadVertex v)
        {
            LastDownPoint = v;
            Observer.CursorPosChanged(this, LastDownPoint, CursorType.LAST_DOWN);
        }
    }
}
