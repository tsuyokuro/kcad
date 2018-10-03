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
            AddChildren(layer);
        }

        public CadLayerTreeItem()
        {
        }

        public void AddChildren(CadLayer layer)
        {
            Layer = layer;

            for (int i = 0; i < Layer.FigureList.Count; i++)
            {
                CadFigure fig = Layer.FigureList[i];

                CadObjTreeItem item = new CadFigTreeItem(fig);
                Add(item);
            }
        }

        public void AddChildren(CadLayer layer, Func<CadFigure, bool> filterFunc)
        {
            Layer = layer;

            for (int i = 0; i < Layer.FigureList.Count; i++)
            {
                CadFigure fig = Layer.FigureList[i];

                if (!filterFunc(fig))
                {
                    continue;
                }

                CadObjTreeItem item = new CadFigTreeItem(fig);
                Add(item);
            }
        }
    }
}
