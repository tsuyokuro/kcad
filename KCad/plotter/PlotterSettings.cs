using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Plotter
{
    class PlotterSettings
    {
        public bool SnapToPoint = true;

        public bool SnapToSegment = true;

        public bool SnapToLine = true;

        public bool SnapToGrid = false;

        public CadPoint GridSize;

        public double PointSnapRange = 6;

        public double LineSnapRange = 8;

        public PlotterSettings()
        {
            GridSize = CadPoint.Create(10, 10, 10);
        }

        private String FileName()
        {
            Assembly asm = Assembly.GetEntryAssembly();

            string exePath = asm.Location;

            String dir = Path.GetDirectoryName(exePath);

            string fileName = dir + @"\settings.xml";

            return fileName;
        }


        public bool Save()
        {
            string fileName = FileName();

            XmlDocument document = new XmlDocument();

            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = document.CreateElement("settings");

            document.AppendChild(declaration);
            document.AppendChild(root);

            XmlElement item;

            item = document.CreateElement("EnableSnap");

            item.SetAttribute("point", SnapToPoint.ToString());
            item.SetAttribute("segment", SnapToSegment.ToString());
            item.SetAttribute("line", SnapToLine.ToString());
            item.SetAttribute("grid", SnapToGrid.ToString());

            root.AppendChild(item);


            item = document.CreateElement("GridSize");

            item.SetAttribute("x", GridSize.x.ToString());
            item.SetAttribute("y", GridSize.y.ToString());
            item.SetAttribute("z", GridSize.z.ToString());

            root.AppendChild(item);


            item = document.CreateElement("PointSnapRange");
            item.SetAttribute("range", PointSnapRange.ToString());
            root.AppendChild(item);

            item = document.CreateElement("LineSnapRange");
            item.SetAttribute("range", LineSnapRange.ToString());
            root.AppendChild(item);

            try
            {
                document.Save(fileName);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool Load()
        {
            string fileName = FileName();

            if (!File.Exists(fileName))
            {
                return true;
            }

            XmlDocument document = new XmlDocument();

            try
            {
                document.Load(fileName);
            }
            catch (Exception e)
            {
                return false;
            }

            XmlNode node = document.DocumentElement;

            foreach (XmlElement item in node.ChildNodes)
            {
                if (item.Name == "EnableSnap")
                {
                    SnapToPoint = GetAttributeBool(item, "point", true);
                    SnapToSegment = GetAttributeBool(item, "segment", true);
                    SnapToLine = GetAttributeBool(item, "line", true);
                    SnapToGrid = GetAttributeBool(item, "grid", false);
                }
                else if (item.Name == "GridSize")
                {
                    GridSize.x = GetAttributeDouble(item, "x", 1);
                    GridSize.y = GetAttributeDouble(item, "y", 1);
                    GridSize.z = GetAttributeDouble(item, "z", 1);
                }
                else if (item.Name == "PointSnapRange")
                {
                    PointSnapRange = GetAttributeDouble(item, "range", 8);
                }
                else if (item.Name == "LineSnapRange")
                {
                    LineSnapRange = GetAttributeDouble(item, "range", 8);
                }
            }

            return true;
        }

        public void Set(PlotterSettings settings)
        {
            GridSize = settings.GridSize;

            PointSnapRange = settings.PointSnapRange;

            LineSnapRange = settings.LineSnapRange; ;
        }

        private double GetAttributeDouble(XmlElement item, string name, double defaultValue)
        {
            double ret;

            if (!Double.TryParse(item.GetAttribute(name),out ret))
            {
                ret = defaultValue;
            }

            return ret;
        }

        private bool GetAttributeBool(XmlElement item, string name, bool defaultValue)
        {
            bool ret;

            if (!Boolean.TryParse(item.GetAttribute(name), out ret))
            {
                ret = defaultValue;
            }

            return ret;
        }
    }
}