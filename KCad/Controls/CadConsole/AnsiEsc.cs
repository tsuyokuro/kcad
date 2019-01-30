using System.Windows.Media;

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


        public const string BalckBG = ESC + "40m";
        public const string RedBG = ESC + "41m";
        public const string GreenBG = ESC + "42m";
        public const string YellowBG = ESC + "43m";
        public const string BlueBG = ESC + "44m";
        public const string MagentaBG = ESC + "45m";
        public const string CyanBG = ESC + "46m";
        public const string WhiteBG = ESC + "47m";

        public const string BBalckBG = ESC + "100m";
        public const string BRedBG = ESC + "101m";
        public const string BGreenBG = ESC + "102m";
        public const string BYellowBG = ESC + "103m";
        public const string BBlueBG = ESC + "104m";
        public const string BMagentaBG = ESC + "105m";
        public const string BCyanBG = ESC + "106m";
        public const string BWhiteBG = ESC + "107m";


        public Brush[] Palette;
        public Brush[] BrightPalette;

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
