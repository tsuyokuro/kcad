﻿using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class PaperSizes
    {
        public static Dictionary<PaperKind, (double Width, double Height)> SizeMap =
            new Dictionary<PaperKind, (double Width, double Height)>();

        public static (double Width, double Height) GetSize(PaperKind kind, bool landscape)
        {
            (double w, double h) = SizeMap[kind];

            if (landscape)
            {
                return (h, w);
            }

            return (w, h);
        }

        static PaperSizes()
        {
            SizeMap[PaperKind.A4] = (210.0, 297.0);
            SizeMap[PaperKind.A5] = (148.0, 210.0);
            SizeMap[PaperKind.A6] = (105.0, 148.0);

            SizeMap[PaperKind.B5] = (182.0, 257.0);
        }
    }


    public class PaperPageSize
    {
        // デフォルト A4縦
        public double Width = 210.0;
        public double Height = 297.0;

        public PaperKind mPaperKind = PaperKind.A4;

        public bool mLandscape = false;

        public PaperPageSize()
        {
        }

        public PaperPageSize(PaperKind papaerKind, bool landscape)
        {
            mPaperKind = papaerKind;
            mLandscape = landscape;

            (Width, Height) = PaperSizes.GetSize(papaerKind, landscape);
        }

        public void Setup(PageSettings settings)
        {
            mPaperKind = settings.PaperSize.Kind;

            mLandscape = settings.Landscape;


            // PageSettingsは、1/100 Inch単位で設定されているのでmmに変換

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
