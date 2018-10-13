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

        public static Item CreatingFigureQuit = new Item(Commands.CREATING_FIGURE_QUIT, "QUIT");
        public static Item CreatingFigureEnd = new Item(Commands.CREATING_FIGURE_END, "END");
        public static Item CreatingFigureClose = new Item(Commands.CREATING_FIGURE_CLOSE, "TO LOOP");
        public static Item Copy = new Item(Commands.COPY, "COPY");
        public static Item Paste = new Item(Commands.PASTE, "PASTE");
    }
}