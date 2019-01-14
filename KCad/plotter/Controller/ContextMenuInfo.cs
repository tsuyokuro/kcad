using System.Collections.Generic;

namespace Plotter.Controller
{
    public class MenuInfo
    {
        public enum Commands
        {
            CREATING_FIGURE_QUIT,
            CREATING_FIGURE_END,
            CREATING_FIGURE_CLOSE,
            COPY,
            PASTE,
            INSERT_POINT,
        }

        public class Item
        {
            public Commands Command;
            public string DefaultText;
            public object Tag;

            public Item(Commands cmd, string defaultText, object tag)
            {
                Command = cmd;
                DefaultText = defaultText;
                Tag = tag;
            }

            public Item(Commands cmd, string defaultText) : this(cmd, defaultText, null)
            {
            }
        }

        public List<Item> Items = new List<Item>(20);

        public static Item CreatingFigureQuit = new Item(Commands.CREATING_FIGURE_QUIT, "Quit create");
        public static Item CreatingFigureEnd = new Item(Commands.CREATING_FIGURE_END, "End create");
        public static Item CreatingFigureClose = new Item(Commands.CREATING_FIGURE_CLOSE, "To loop");
        public static Item Copy = new Item(Commands.COPY, "Copy");
        public static Item Paste = new Item(Commands.PASTE, "Paste");
        public static Item InsertPoint = new Item(Commands.INSERT_POINT, "Insert point");
    }
}