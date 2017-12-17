using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plotter
{
    public class BitmapUtil
    {
        public static void BitmapToClipboardAsPNG(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();

            bmp.Save(ms, ImageFormat.Png);

            IDataObject dataObject = new DataObject();

            dataObject.SetData("PNG", false, ms);

            Clipboard.SetDataObject(dataObject);
        }


        public static Bitmap CreateAABitmap2x2(Bitmap src, Color color)
        {
            int dw = (int)src.Width / 2;
            int dh = (int)src.Height / 2;

            Bitmap dest = new Bitmap(dw, dh);

            BitmapData dstBits = dest.LockBits(
                    new System.Drawing.Rectangle(0, 0, dest.Width, dest.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, dest.PixelFormat);

            BitmapData srcBits = src.LockBits(
                    new System.Drawing.Rectangle(0, 0, src.Width, src.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, src.PixelFormat);

            byte r = color.R;
            byte g = color.G;
            byte b = color.B;

            unsafe
            {
                byte* s0;
                byte* s1;
                byte* s2;
                byte* s3;
                int spcnt = src.Width;
                int spcnt2 = spcnt * 2;

                uint* srcPixels = (uint*)srcBits.Scan0;
                uint* dstPixels = (uint*)dstBits.Scan0;

                int sline0 = 0;
                int si = 0;

                int x;
                int y = 0;

                uint* psrcLine = srcPixels;
                uint* pdstLine = dstPixels;

                byte* dst;
                int dpcnt = dest.Width;

                for (; y < dh; y++)
                {
                    x = 0;
                    int x2 = 0;
                    for (; x < dw; x++)
                    {
                        s0 = (byte*)(psrcLine + x2 + 0);
                        s1 = (byte*)(psrcLine + x2 + 1);
                        s2 = (byte*)(psrcLine + x2 + spcnt + 0);
                        s3 = (byte*)(psrcLine + x2 + spcnt + 1);

                        int a = (int)(s0[3] + s1[3] + s2[3] + s3[3]) / 4;

                        if (a != 0)
                        {
                            dst = (byte*)(pdstLine + x);
                            dst[0] = b;
                            dst[1] = g;
                            dst[2] = r;
                            dst[3] = (byte)a;
                        }

                        x2 += 2;
                    }

                    psrcLine += spcnt2;
                    pdstLine += dpcnt;
                }
            }

            return dest;
        }

    }
}
