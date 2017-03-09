using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public class PaperPageSize
    {
        public double Width;
        public double Height;

        public double widthInch
        {
            set
            {
                Width = value * 25.4;
            }

            get
            {
                return Width / 25.4;
            }
        }

        public double heightInch
        {
            set
            {
                Height = value * 25.4;
            }

            get
            {
                return Height / 25.4;
            }
        }

        public PaperPageSize()
        {
            A4Land();
        }

        public void A4()
        {
            Width = 210.0;
            Height = 297.0;
        }

        public void A4Land()
        {
            Width = 297.0;
            Height = 210.0;
        }

        public PaperPageSize clone()
        {
            return (PaperPageSize)MemberwiseClone();
        }

        public bool IsLandscape()
        {
            return (Width > Height);
        }
    }
}
