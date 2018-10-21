using KCad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCad.CadObjectTreeView;
using CadDataTypes;
using System.Windows.Media;

namespace Plotter
{

    class CadPointTreeItem : CadObjTreeItem
    {
        public CadFigure Fig;
        public int Index;

        private static SolidColorBrush CheckedBackColor = new SolidColorBrush(Color.FromRgb(0x11, 0x46, 0x11));

        public override bool IsChecked
        {
            get
            {
                if (Index >=0 && Index < Fig.PointCount)
                {
                    return Fig.GetPointAt(Index).Selected;
                }

                return false;
            }

            set
            {
                CadVector v = Fig.GetPointAt(Index);
                v.Selected = value;
                Fig.SetPointAt(Index, v);
            }
        }

        public override string Text
        {
            get
            {
                CadVector v;

                if (Index >= 0 && Index < Fig.PointCount)
                {
                    v = Fig.GetPointAt(Index);

                    //return String.Format("{0, 7:F2},{1, 7:F2},{2, 7:F2}", v.x, v.y, v.z);

                    return v.x.ToString("F2") + ", " + v.y.ToString("F2") + ", " + v.z.ToString("F2");
                }

                return "removed";
            }
        }

        public override SolidColorBrush getForeColor()
        {
            return null;
        }

        public override SolidColorBrush getBackColor()
        {
            if (!IsChecked)
            {
                return null;
            }

            return CheckedBackColor;
        }

        public CadPointTreeItem(CadFigure fig, int idx)
        {
            Fig = fig;
            Index = idx;
        }
    }
}
