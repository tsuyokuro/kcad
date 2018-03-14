using KCad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KCad.CadObjectTreeView;

namespace Plotter
{
    class CadLayerTreeItem : CadObjTreeItem
    {
        public CadLayer Layer;

        public override bool IsChecked
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public override string Text
        {
            get
            {
                return "LAYER:" + Layer.Name;
            }
        }

        public CadLayerTreeItem(CadLayer layer)
        {
            Layer = layer;

            for (int i=0;i< Layer.FigureList.Count; i++)
            {
                CadFigure fig = Layer.FigureList[i];

                CadObjTreeItem item = new CadFigTreeItem(fig);
                Add(item);
            }
        }
    }

    class CadFigTreeItem : CadObjTreeItem
    {
        public CadFigure Fig;

        public override bool IsChecked
        {
            get
            {
                return HasSelectedPoint();
            }

            set
            {
                SelectAllPoints(value);
            }
        }

        public override string Text
        {
            get
            {
                return "FIG:" + Fig.ID.ToString();
            }
        }

        public CadFigTreeItem(CadFigure fig)
        {
            Fig = fig;

            int idx = 0;

            for (int i=0; i < Fig.PointList.Count; i++) {
                CadVector p = Fig.PointList[i];
                CadPointTreeItem pi = new CadPointTreeItem(fig, idx);
                Add(pi);
                idx++;
            }

            for (int i=0; i<Fig.ChildList.Count; i++)
            {
                CadFigure c = Fig.ChildList[i];
                CadFigTreeItem pi = new CadFigTreeItem(c);
                Add(pi);
            }
        }

        private void SelectAllPoints(bool sel)
        {
            if (Children == null)
            {
                return;
            }

            for (int i=0; i<Children.Count; i++)
            {
                CadObjTreeItem c = Children[i];
                c.IsChecked = sel;
            }
        }

        private bool HasSelectedPoint()
        {
            bool ret = false;

            if (Children == null)
            {
                return ret;
            }

            int i;
            for (i=0; i<Children.Count;i++)
            {
                CadObjTreeItem c = Children[i];

                if (c.IsChecked)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
    }

    class CadPointTreeItem : CadObjTreeItem
    {
        public CadFigure Fig;
        public int Index;

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

                    return String.Format("{0, 7:F2},{1, 7:F2},{2, 7:F2}", v.x, v.y, v.z);

                    //return v.x.ToString("F2") + ", " + v.y.ToString("F2") + ", " + v.z.ToString("F2");
                }

                return "removed";
            }
        }

        public CadPointTreeItem(CadFigure fig, int idx)
        {
            Fig = fig;
            Index = idx;
        }
    }
}
