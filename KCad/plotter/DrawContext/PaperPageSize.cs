using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class PaperPageSize
    {
        // A4
        public double Width = 210.0;
        public double Height = 297.0;

        public PaperKind mPaperKind = PaperKind.A4;

        public bool mLandscape = false;

        public PaperPageSize()
        {
            //SetPageSettings(new PageSettings());
        }

        public void Setup(PageSettings settings)
        {
            mPaperKind = settings.PaperSize.Kind;

            mLandscape = settings.Landscape;

            Width =
                Math.Round(settings.Bounds.Width * 25.4 / 100.0, MidpointRounding.AwayFromZero);

            Height =
                Math.Round(settings.Bounds.Height * 25.4 / 100.0, MidpointRounding.AwayFromZero);
        }

        public bool IsLandscape()
        {
            return mLandscape;
        }

        public PaperSize GetPaperSize()
        {
            PrintDocument pd = new PrintDocument();
            int cnt = pd.PrinterSettings.PaperSizes.Count;
            int i;

            PaperSize matchSize = null;

            for (i = 0; i < cnt; i++)
            {
                PaperSize ps = pd.PrinterSettings.PaperSizes[i];
                if (ps.Kind == mPaperKind)
                {
                    return ps;
                }
            }

            return null;
        }

        public double MilliToInch(double mm)
        {
            return mm / 25.4;
        }

        public double InchToMilli(double inchi)
        {
            return inchi * 25.4;
        }
    }
}
