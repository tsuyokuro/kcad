using CadDataTypes;
using System;

namespace Plotter.Controller
{
    public class PlotterObserver
    {
        public Action<PlotterController, PlotterStateInfo> StateChanged = (controller, state) => { };

        public Action<PlotterController, MenuInfo, int, int> RequestContextMenu = (controller, state, x, y) => { };

        public Action<PlotterController, LayerListInfo> LayerListChanged = (controller, layerListInfo) => { };

        public Action<PlotterController, bool> DataChanged = (controller, redraw) => { };

        public Action<PlotterController, CadVector, CursorType> CursorPosChanged = (controller, pos, cursorType) => { };

        public Action<bool> UpdateTreeView = (remakeTree) => { };

        public Action<int> SetTreeViewPos = (index) => { };

        public Func<uint, int> FindTreeViewItem = (id) => { return 0; /* idx */ };
    }
}