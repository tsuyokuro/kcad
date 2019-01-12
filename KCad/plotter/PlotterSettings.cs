using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using CadDataTypes;
using System.ComponentModel;
using Plotter.Controller;

namespace Plotter
{
    public static class SettingsHolder
    {
        public static PlotterSettings Settings = new PlotterSettings();
    }

    public class SettingsVeiwModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PlotterController Controller;

        public bool SnapToGrid
        {
            set
            {
                SettingsHolder.Settings.SnapToGrid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToGrid;
            }
        }

        public bool SnapToPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToPoint;
            }
        }

        public bool SnapToSegment
        {
            set
            {
                SettingsHolder.Settings.SnapToSegment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToSegment;
            }
        }

        public bool SnapToLine
        {
            set
            {
                SettingsHolder.Settings.SnapToLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToLine;
            }
        }

        public bool FilterTreeView
        {
            set
            {
                SettingsHolder.Settings.FilterTreeView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTreeView)));

                if (Controller != null)
                {
                    Controller.UpdateTreeView(true);
                }
            }

            get
            {
                return SettingsHolder.Settings.FilterTreeView;
            }
        }

        public bool DrawFaceOutline
        {
            set
            {
                SettingsHolder.Settings.DrawFaceOutline = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawFaceOutline)));

                Redraw();
            }

            get
            {
                return SettingsHolder.Settings.DrawFaceOutline;
            }
        }

        public bool FillFace
        {
            set
            {
                SettingsHolder.Settings.FillFace = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillFace)));

                Redraw();
            }

            get
            {
                return SettingsHolder.Settings.FillFace;
            }
        }

        public double InitialMoveLimit
        {
            set
            {
                SettingsHolder.Settings.InitialMoveLimit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialMoveLimit)));
            }

            get
            {
                return SettingsHolder.Settings.InitialMoveLimit;
            }
        }

        public bool SnapToZero
        {
            set
            {
                SettingsHolder.Settings.SnapToZero = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToZero)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToZero;
            }
        }

        public bool SnapToLastDownPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToLastDownPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLastDownPoint)));
            }

            get
            {
                return SettingsHolder.Settings.SnapToLastDownPoint;
            }
        }

        public CadVector GridSize
        {
            set
            {
                SettingsHolder.Settings.GridSize = value;
                Controller.Grid.GridSize = value;
            }

            get
            {
                return SettingsHolder.Settings.GridSize;
            }
        }

        public double PointSnapRange
        {
            set
            {
                SettingsHolder.Settings.PointSnapRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointSnapRange)));
            }

            get
            {
                return SettingsHolder.Settings.PointSnapRange;
            }
        }

        public double LineSnapRange
        {
            set
            {
                SettingsHolder.Settings.LineSnapRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineSnapRange)));
            }

            get
            {
                return SettingsHolder.Settings.LineSnapRange;
            }
        }

        public SettingsVeiwModel(PlotterController controller)
        {
            Controller = controller;
        }

        private void Redraw()
        {
            Controller.Redraw(Controller.CurrentDC);
        }

        public void Load()
        {
            SettingsHolder.Settings.Load();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawFaceOutline)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillFace)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTreeView)));

            Controller.Grid.GridSize = SettingsHolder.Settings.GridSize;
        }

        public void Save()
        {
            PlotterSettings settings = SettingsHolder.Settings;

            settings.GridSize = Controller.Grid.GridSize;

            settings.Save();
        }
    }

    public class PlotterSettings
    {
        public bool SnapToPoint = true;

        public bool SnapToSegment = true;

        public bool SnapToLine = true;

        public bool SnapToGrid = false;

        public CadVector GridSize;

        public double PointSnapRange = 6;

        public double LineSnapRange = 8;


        public bool DrawFaceOutline = true;

        public bool FillFace = true;

        public bool FilterTreeView = false;


        public double InitialMoveLimit = 6.0;

        public bool SnapToZero = true;

        public bool SnapToLastDownPoint = true;

        public PlotterSettings()
        {
            GridSize = CadVector.Create(10, 10, 10);
        }

        private String FileName()
        {
            Assembly asm = Assembly.GetEntryAssembly();

            string exePath = asm.Location;

            String dir = Path.GetDirectoryName(exePath);

            string fileName = dir + @"\settings.json";

            return fileName;
        }

        public bool Save()
        {
            string fileName = FileName();

            JObject root = new JObject();

            JObject jo;

            jo = new JObject();
            jo.Add("enable", SnapToPoint);
            jo.Add("range", PointSnapRange);
            root.Add("PointSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToSegment);
            root.Add("SegmentSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToLine);
            jo.Add("range", LineSnapRange);
            root.Add("LineSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToGrid);
            jo.Add("size_x", GridSize.x);
            jo.Add("size_y", GridSize.z);
            jo.Add("size_z", GridSize.x);
            root.Add("GridInfo", jo);


            jo = new JObject();
            jo.Add("DrawFaceOutline", DrawFaceOutline);
            jo.Add("FillFace", FillFace);
            root.Add("DrawSettings", jo);

            StreamWriter writer = new StreamWriter(fileName);

            writer.Write(root.ToString());
            writer.Close();

            return true;
        }

        public bool Load()
        {
            string fileName = FileName();

            if (!File.Exists(fileName))
            {
                return true;
            }

            StreamReader reader = new StreamReader(fileName);

            var js = reader.ReadToEnd();

            reader.Close();

            JObject root = JObject.Parse(js);

            JObject jo;

            jo = (JObject)root["PointSnap"];
            SnapToPoint = jo.GetBool("enable", SnapToPoint);
            PointSnapRange = jo.GetDouble("range", PointSnapRange);

            jo = (JObject)root["SegmentSnap"];
            SnapToSegment = jo.GetBool("enable", SnapToSegment);

            jo = (JObject)root["LineSnap"];
            SnapToLine = jo.GetBool("enable", SnapToLine);
            LineSnapRange = jo.GetDouble("range", LineSnapRange);

            jo = (JObject)root["DrawSettings"];
            DrawFaceOutline = jo.GetBool("DrawFaceOutline", DrawFaceOutline);
            FillFace = jo.GetBool("FillFace", FillFace);

            return true;
        }
    }
}