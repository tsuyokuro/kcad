﻿using CadDataTypes;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Plotter.Controller
{
    public class PlotterObserver
    {
        public enum MessageType
        {
            INFO,
            INPUT,
            ERROR,
        }

        public enum MouseCursorType
        {
            NORMAL_ARROW,
            CROSS,
            HAND,
        }

        public Action<PlotterController, PlotterStateInfo> StateChanged = (controller, state) => { };

        public Action<PlotterController, MenuInfo, int, int> RequestContextMenu = (controller, state, x, y) => { };

        public Action<PlotterController, LayerListInfo> LayerListChanged = (controller, layerListInfo) => { };

        //public Action<PlotterController, bool> DataChanged = (controller, redraw) => { };

        public Action<PlotterController, Vector3d, CursorType> CursorPosChanged = (controller, pos, cursorType) => { };

        public Action<bool> UpdateObjectTree = (remakeTree) => { };

        public Action<int> SetObjectTreePos = (index) => { };

        public Func<uint, int> FindObjectTreeItem = (id) => { return 0; /* index. It can be used with FindObjectTreeItem */ };

        public Action<string, MessageType> OpenPopupMessage = (text, messageType) => { };

        public Action ClosePopupMessage = () => { };

        public Action<bool> CursorLocked = (locked) => {}; 

        public Action<MouseCursorType> ChangeMouseCursor = (cursorType) => {};

        public Func<string, List<string>> HelpOfKey = (keyword) => { return null; };
    }
}