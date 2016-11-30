using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class DrawSettings
    {
        public Pen SelectedPointPen;
        public Pen CursorPen;
        public Pen DefaultFigurePen;
        public Pen TempFigurePen;
        public Pen PointHighlitePen;
        public Pen MatchFigurePen;
        public Pen MatchSegPen;

        public Pen AuxiliaryLinePen;
        public Pen LastPointMarkerPen1;
        public Pen LastPointMarkerPen2;

        public Color BackgroundColor;
        public Brush BackgroundBrush;

        public Pen AxesPen;
        public Pen PageFramePen;

        public Pen RelativePointPen;

        class Original : IDisposable
        {
            public Color DarkBgColor = Color.FromArgb(20, 20, 30);

            public Pen BalckThinPen = new Pen(Brushes.Black, 0);
            public Brush DarkBgBrush = Brushes.Black;

            public Pen DarkAxisPen = new Pen(Color.FromArgb(60,60,92), 0);
            public Pen DarkFramePen = new Pen(Color.FromArgb(92, 92, 92), 0);

            public Original()
            {
                DarkBgBrush = new SolidBrush(DarkBgColor);
            }

            public void Dispose()
            {
                BalckThinPen.Dispose();
                DarkBgBrush.Dispose();

                DarkAxisPen.Dispose();
                DarkFramePen.Dispose();
            }
        }

        private static Original org  = new Original();


        public DrawSettings()
        {
            darkSet();
        }

        ~DrawSettings()
        {
            if (org != null)
            {
                org.Dispose();
            }
        }

        public void setupPrinterSet()
        {
            SelectedPointPen = Pens.Black;
            CursorPen = Pens.Black;
            DefaultFigurePen = org.BalckThinPen;
            TempFigurePen = Pens.Black;
            PointHighlitePen = Pens.Black;
            MatchFigurePen = Pens.Black;
            MatchSegPen = Pens.Black;

            AuxiliaryLinePen = Pens.Black;
            LastPointMarkerPen1 = Pens.Black;
            LastPointMarkerPen2 = Pens.Black;

            BackgroundColor = Color.White;
            BackgroundBrush = Brushes.Transparent;

            AxesPen = null;
            PageFramePen = null;
            RelativePointPen = null;
        }

        public void darkSet()
        {
            SelectedPointPen = Pens.LightGreen;
            CursorPen = Pens.LightBlue;
            DefaultFigurePen = Pens.White;
            TempFigurePen = Pens.Blue;
            PointHighlitePen = Pens.BlueViolet;
            MatchFigurePen = Pens.Red;
            MatchSegPen = Pens.Green;

            AuxiliaryLinePen = Pens.Coral;
            LastPointMarkerPen1 = Pens.Aqua;
            LastPointMarkerPen2 = Pens.YellowGreen;

            BackgroundColor = org.DarkBgColor;
            BackgroundBrush = org.DarkBgBrush;

            AxesPen = org.DarkAxisPen;
            PageFramePen = org.DarkFramePen;

            RelativePointPen = Pens.CornflowerBlue;
        }

        public void whiteSet()
        {
            SelectedPointPen = Pens.Blue;
            CursorPen = Pens.Blue;
            DefaultFigurePen = Pens.Black;
            TempFigurePen = Pens.Blue;
            PointHighlitePen = Pens.BlueViolet;
            MatchFigurePen = Pens.Red;
            MatchSegPen = Pens.Green;

            AuxiliaryLinePen = Pens.Coral;
            LastPointMarkerPen1 = Pens.DarkBlue;
            LastPointMarkerPen2 = Pens.DarkGreen;

            BackgroundColor = Color.White;
            BackgroundBrush = Brushes.White;

            AxesPen = Pens.Gray;
            PageFramePen = Pens.Gray;

            RelativePointPen = Pens.CornflowerBlue;
        }
    }
}
