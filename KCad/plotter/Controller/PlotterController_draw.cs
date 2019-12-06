﻿
using CadDataTypes;
using OpenTK;
using Plotter.Settings;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        public void PushDraw()
        {
            DC.PushDraw();
        }

        public void Redraw(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = DC;
            }

            dc.StartDraw();
            Clear(dc);
            DrawAll(dc);
            dc.EndDraw();
            dc.PushDraw();
        }

        public void Clear(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = DC;
            }

            dc.Drawing.Clear(dc.GetBrush(DrawTools.BRUSH_BACKGROUND));
        }

        public void DrawAll(DrawContext dc = null)
        {
            if (dc == null)
            {
                dc = DC;
            }

            DrawBase(dc);

            DrawDragLine(dc);

            DrawCrossCursor(dc);

            DrawFigures(dc);

            DrawSelectedItems(dc);

            DrawLastPoint(dc);

            DrawHighlightPoint(dc);

            DrawExtendSnapPoint(dc);

            DrawAccordingState(dc);
        }

        public void DrawAllFigures(DrawContext dc)
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                if (!layer.Visible) continue;

                foreach (CadFigure fig in layer.FigureList)
                {
                    fig.DrawEach(dc);
                }
            }
        }


        protected void DrawBase(DrawContext dc)
        {
            if (SettingsHolder.Settings.DrawAxis)
            {
                dc.Drawing.DrawAxis();
            }
            else
            {
                dc.Drawing.DrawCrossScrn(dc.GetPen(DrawTools.PEN_AXIS), dc.WorldPointToDevPoint(Vector3d.Zero), 8);
            }

            dc.Drawing.DrawPageFrame(PageSize.Width, PageSize.Height, Vector3d.Zero);
            DrawGrid(dc);
        }

        protected void DrawFigures(DrawContext dc)
        {
            if (dc == null) return;

            DrawParams pale_dp = default;
            DrawParams test_dp = default;
            DrawParams current_dp = default;
            DrawParams measure_dp = default;

            DrawParams empty_dp = default;
            empty_dp.Empty = true;

            pale_dp.LinePen = dc.GetPen(DrawTools.PEN_PALE_FIGURE);
            pale_dp.EdgePen = dc.GetPen(DrawTools.PEN_PALE_FIGURE);
            pale_dp.FillBrush = DrawBrush.NullBrush;
            pale_dp.TextBrush = dc.GetBrush(DrawTools.BRUSH_PALE_TEXT);

            test_dp.LinePen = dc.GetPen(DrawTools.PEN_TEST_FIGURE);
            test_dp.EdgePen = dc.GetPen(DrawTools.PEN_TEST_FIGURE);
            test_dp.FillBrush = DrawBrush.NullBrush;
            test_dp.TextBrush = dc.GetBrush(DrawTools.BRUSH_TEXT);

            current_dp.LinePen = dc.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);
            current_dp.EdgePen = dc.GetPen(DrawTools.PEN_FIGURE_HIGHLIGHT);
            current_dp.FillBrush = DrawBrush.NullBrush;
            current_dp.TextBrush = dc.GetBrush(DrawTools.BRUSH_TEXT);

            measure_dp.LinePen = dc.GetPen(DrawTools.PEN_MEASURE_FIGURE);
            measure_dp.EdgePen = dc.GetPen(DrawTools.PEN_MEASURE_FIGURE);
            measure_dp.FillBrush = DrawBrush.NullBrush;
            measure_dp.TextBrush = dc.GetBrush(DrawTools.BRUSH_TEXT);

            lock (DB)
            {
                foreach (CadLayer layer in mDB.LayerList)
                {
                    if (!layer.Visible) continue;

                    // Skip current layer.
                    // It will be drawn at the end of this loop.
                    if (layer == CurrentLayer) { continue; }

                    foreach (CadFigure fig in layer.FigureList)
                    {
                        if (fig.Current)
                        {
                            fig.DrawEach(dc, current_dp);
                        }
                        else
                        {
                            fig.DrawEach(dc, pale_dp);
                        }
                    }
                }

                // Draw current layer at last
                if (CurrentLayer != null && CurrentLayer.Visible)
                {
                    foreach (CadFigure fig in CurrentLayer.FigureList)
                    {
                        if (fig.Current)
                        {
                            fig.DrawEach(dc, current_dp);
                        }
                        else
                        {
                            fig.DrawEach(dc);
                        }
                    }
                }

                foreach (CadFigure fig in TempFigureList)
                {
                    fig.DrawEach(dc, test_dp);
                }

                if (MeasureFigureCreator != null)
                {
                    MeasureFigureCreator.Figure.Draw(dc, measure_dp);
                }
            }
        }

        protected void DrawGrid(DrawContext dc)
        {
            if (SettingsHolder.Settings.SnapToGrid)
            {
                dc.Drawing.DrawGrid(mGridding);
            }
        }

        protected void DrawSelectedItems(DrawContext dc)
        {
            foreach (CadLayer layer in mDB.LayerList)
            {
                dc.Drawing.DrawSelected(layer.FigureList);
            }
        }

        protected void DrawLastPoint(DrawContext dc)
        {
            dc.Drawing.DrawMarkCursor(
                dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER),
                LastDownPoint,
                ControllerConst.MARK_CURSOR_SIZE);

            if (ObjDownPoint.IsValid())
            {
                dc.Drawing.DrawMarkCursor(
                    dc.GetPen(DrawTools.PEN_LAST_POINT_MARKER2),
                    ObjDownPoint,
                    ControllerConst.MARK_CURSOR_SIZE);
            }
        }

        protected void DrawDragLine(DrawContext dc)
        {
            if (State != States.DRAGING_POINTS)
            {
                return;
            }

            dc.Drawing.DrawLine(dc.GetPen(DrawTools.PEN_DRAG_LINE),
                LastDownPoint, dc.DevPointToWorldPoint(CrossCursor.Pos));
        }

        protected void DrawCrossCursor(DrawContext dc)
        {
            dc.Drawing.DrawCrossCursorScrn(CrossCursor, dc.GetPen(DrawTools.PEN_CURSOR2));

            if (CursorLocked)
            {
                dc.Drawing.DrawCrossScrn(
                    dc.GetPen(DrawTools.PEN_POINT_HIGHLIGHT),
                    CrossCursor.Pos,
                    ControllerConst.CURSOR_LOCK_MARK_SIZE);
            }
        }

        protected void DrawSelRect(DrawContext dc)
        {
            dc.Drawing.DrawRectScrn(
                dc.GetPen(DrawTools.PEN_TEMP_FIGURE),
                RubberBandScrnPoint0,
                RubberBandScrnPoint1);
        }

        protected void DrawAccordingState(DrawContext dc)
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
                        Vector3d p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        FigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
                    }
                    break;

                case States.MEASURING:
                    if (MeasureFigureCreator != null)
                    {
                        Vector3d p = dc.DevPointToWorldPoint(CrossCursor.Pos);
                        MeasureFigureCreator.DrawTemp(dc, (CadVertex)p, dc.GetPen(DrawTools.PEN_TEMP_FIGURE));
                    }
                    break;
            }

            if (mInteractCtrl.IsActive)
            {
                mInteractCtrl.Draw(dc, SnapPoint);
            }
        }

        protected void DrawHighlightPoint(DrawContext dc)
        {
            dc.Drawing.DrawHighlightPoints(HighlightPointList);
        }

        protected void DrawExtendSnapPoint(DrawContext dc)
        {
            dc.Drawing.DrawExtSnapPoints(ExtendSnapPointList, dc.GetPen(DrawTools.PEN_EXT_SNAP));
        }
    }
}