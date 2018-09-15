using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KCad
{
    public class AnsiEsc
    {
        public const string ESC = "\x1b[";

        public const string Reset = ESC + "0m";

        public const string Balck = ESC + "30m";
        public const string Red = ESC + "31m";
        public const string Green = ESC + "32m";
        public const string Yellow = ESC + "33m";
        public const string Blue = ESC + "34m";
        public const string Magenta = ESC + "35m";
        public const string Cyan = ESC + "36m";
        public const string White = ESC + "37m";

        public const string BBalck = ESC + "90m";
        public const string BRed = ESC + "91m";
        public const string BGreen = ESC + "92m";
        public const string BYellow = ESC + "93m";
        public const string BBlue = ESC + "94m";
        public const string BMagenta = ESC + "95m";
        public const string BCyan = ESC + "96m";
        public const string BWhite = ESC + "97m";


        public const string BalckB = ESC + "40m";
        public const string RedB = ESC + "41m";
        public const string GreenB = ESC + "42m";
        public const string YellowB = ESC + "43m";
        public const string BlueB = ESC + "44m";
        public const string MagentaB = ESC + "45m";
        public const string CyanB = ESC + "46m";
        public const string WhiteB = ESC + "47m";

        public const string BBalckB = ESC + "100m";
        public const string BRedB = ESC + "101m";
        public const string BGreenB = ESC + "102m";
        public const string BYellowB = ESC + "103m";
        public const string BBlueB = ESC + "104m";
        public const string BMagentaB = ESC + "105m";
        public const string BCyanB = ESC + "106m";
        public const string BWhiteB = ESC + "107m";


        public Brush[] Palette;
        public Brush[] SelPalette;

        public byte DefaultFColor = 7;
        public byte DefaultBColor = 0;

        public AnsiEsc()
        {
            Palette = new Brush[16];

            Palette[0] = Brushes.Black;
            Palette[1] = Brushes.MediumVioletRed;
            Palette[2] = Brushes.SeaGreen;
            Palette[3] = Brushes.Goldenrod;
            Palette[4] = Brushes.SteelBlue;
            Palette[5] = Brushes.DarkMagenta;
            Palette[6] = Brushes.DarkCyan;
            Palette[7] = Brushes.LightGray;

            Palette[8] = Brushes.Black;
            Palette[9] = Brushes.LightCoral;
            Palette[10] = Brushes.SpringGreen;
            Palette[11] = Brushes.Yellow;
            Palette[12] = Brushes.CornflowerBlue;
            Palette[13] = Brushes.MediumOrchid;
            Palette[14] = Brushes.Turquoise;
            Palette[15] = Brushes.White;



            SelPalette = new Brush[16];

            float b = 1.4f;

            SelPalette[0] = Brigahtness(Palette[0], b);
            SelPalette[1] = Brigahtness(Palette[1], b);
            SelPalette[2] = Brigahtness(Palette[2], b);
            SelPalette[3] = Brigahtness(Palette[3], b);
            SelPalette[4] = Brigahtness(Palette[4], b);
            SelPalette[5] = Brigahtness(Palette[5], b);
            SelPalette[6] = Brigahtness(Palette[6], b);
            SelPalette[7] = Brigahtness(Palette[7], b);

            SelPalette[8] = Brigahtness(Palette[8], b);
            SelPalette[9] = Brigahtness(Palette[9], b);
            SelPalette[10] = Brigahtness(Palette[10], b);
            SelPalette[11] = Brigahtness(Palette[11], b);
            SelPalette[12] = Brigahtness(Palette[12], b);
            SelPalette[13] = Brigahtness(Palette[13], b);
            SelPalette[14] = Brigahtness(Palette[14], b);
            SelPalette[15] = Brigahtness(Palette[15], b);
        }

        public Brush Brigahtness(Brush brush, float a)
        {
            if (!(brush is SolidColorBrush))
            {
                return brush;
            }

            SolidColorBrush src = (SolidColorBrush)brush;
            Color c = src.Color;

            RGB rgb = new RGB(c.R / 255f, c.G / 255f, c.B / 255f);

            rgb = ColorUtil.Brightness(rgb, a);

            Color cc = Color.FromArgb(0xff, (byte)(rgb.R * 255f), (byte)(rgb.G * 255f), (byte)(rgb.B * 255f));

            return new SolidColorBrush(cc);
        }
    }
}
