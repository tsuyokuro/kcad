using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.IO;
using System.Reflection;
using CadDataTypes;
using System.ComponentModel;
using Plotter.Controller;
using OpenTK;

namespace Plotter.Settings
{
    public static class SettingsHolder
    {
        public static PlotterSettings Settings = new PlotterSettings();
    }

    public class UserSettingDataAttribute : System.Attribute
    {
    }



    public class SettingsVeiwModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PlotterController Controller;

        [UserSettingData]
        public bool SnapToGrid
        {
            set
            {
                if (SettingsHolder.Settings.SnapToGrid != value)
                {
                    SettingsHolder.Settings.SnapToGrid = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));

                    Redraw();
                }
            }

            get => SettingsHolder.Settings.SnapToGrid;
        }

        [UserSettingData]
        public bool SnapToPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            }

            get => SettingsHolder.Settings.SnapToPoint;
        }

        [UserSettingData]
        public bool SnapToSegment
        {
            set
            {
                SettingsHolder.Settings.SnapToSegment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            }

            get => SettingsHolder.Settings.SnapToSegment;
        }

        [UserSettingData]
        public bool SnapToLine
        {
            set
            {
                SettingsHolder.Settings.SnapToLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            }

            get => SettingsHolder.Settings.SnapToLine;
        }

        [UserSettingData]
        public bool FilterObjectTree
        {
            set
            {
                SettingsHolder.Settings.FilterObjectTree = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterObjectTree)));

                if (Controller != null)
                {
                    Controller.UpdateObjectTree(true);
                }
            }

            get => SettingsHolder.Settings.FilterObjectTree;
        }

        [UserSettingData]
        public bool DrawMeshEdge
        {
            set
            {
                if (SettingsHolder.Settings.DrawMeshEdge != value && value == false)
                {
                    if (FillMesh == false)
                    {
                        FillMesh = true;
                    }
                }

                SettingsHolder.Settings.DrawMeshEdge = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawMeshEdge)));

                Redraw();
            }

            get => SettingsHolder.Settings.DrawMeshEdge;
        }

        [UserSettingData]
        public bool FillMesh
        {
            set
            {
                if (SettingsHolder.Settings.FillMesh != value && value == false)
                {
                    if (DrawMeshEdge == false)
                    {
                        DrawMeshEdge = true;
                    }
                }

                SettingsHolder.Settings.FillMesh = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillMesh)));

                Redraw();
            }

            get => SettingsHolder.Settings.FillMesh;
        }

        [UserSettingData]
        public bool DrawNormal
        {
            set
            {
                SettingsHolder.Settings.DrawNormal = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawNormal)));

                Redraw();
            }

            get => SettingsHolder.Settings.DrawNormal;
        }

        [UserSettingData]
        public bool DrawAxis
        {
            set
            {
                SettingsHolder.Settings.DrawAxis = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawAxis)));

                Redraw();
            }

            get => SettingsHolder.Settings.DrawAxis;
        }

        [UserSettingData]
        public double InitialMoveLimit
        {
            set
            {
                SettingsHolder.Settings.InitialMoveLimit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialMoveLimit)));
            }

            get => SettingsHolder.Settings.InitialMoveLimit;
        }

        [UserSettingData]
        public bool SnapToZero
        {
            set
            {
                SettingsHolder.Settings.SnapToZero = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToZero)));
            }

            get => SettingsHolder.Settings.SnapToZero;
        }

        [UserSettingData]
        public bool SnapToLastDownPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToLastDownPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLastDownPoint)));
            }

            get => SettingsHolder.Settings.SnapToLastDownPoint;
        }

        [UserSettingData]
        public bool SnapToSelfPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToSelfPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSelfPoint)));
            }

            get => SettingsHolder.Settings.SnapToSelfPoint;
        }

        [UserSettingData]
        public Vector3d GridSize
        {
            set
            {
                SettingsHolder.Settings.GridSize = value;
                Controller.Grid.GridSize = value;
            }

            get => SettingsHolder.Settings.GridSize;
        }

        [UserSettingData]
        public double PointSnapRange
        {
            set
            {
                SettingsHolder.Settings.PointSnapRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointSnapRange)));
            }

            get => SettingsHolder.Settings.PointSnapRange;
        }

        [UserSettingData]
        public double LineSnapRange
        {
            set
            {
                SettingsHolder.Settings.LineSnapRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineSnapRange)));
            }

            get => SettingsHolder.Settings.LineSnapRange;
        }

        [UserSettingData]
        public double KeyMoveUnit
        {
            set
            {
                SettingsHolder.Settings.KeyMoveUnit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KeyMoveUnit)));
            }

            get => SettingsHolder.Settings.KeyMoveUnit;
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

            MemberInfo[] memberes = typeof(SettingsVeiwModel).GetMembers();

            foreach (MemberInfo member in memberes)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    UserSettingDataAttribute userSetting =
                        (UserSettingDataAttribute)member.GetCustomAttribute(typeof(UserSettingDataAttribute));

                    if (userSetting != null)
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(member.Name));
                    }
                }
            }

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

        public Vector3d GridSize;

        public double PointSnapRange = 6;

        public double LineSnapRange = 8;

        public double KeyMoveUnit = 1.0;

        public bool FilterObjectTree = false;

        public double InitialMoveLimit = 6.0;

        public bool SnapToZero = true;

        public bool SnapToLastDownPoint = true;

        public bool SnapToSelfPoint = true;

        #region Draw settings
        public bool DrawMeshEdge = true;

        public bool FillMesh = true;

        public bool DrawNormal = false;

        public bool DrawAxis = true;       
        #endregion

        public PlotterSettings()
        {
            GridSize = new Vector3d(10, 10, 10);
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
            jo.Add("unit", KeyMoveUnit);
            root.Add("KeyMove", jo);

            jo = new JObject();
            jo.Add("enable", SnapToZero);
            root.Add("ZeroSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToLastDownPoint);
            root.Add("LastDownSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToSelfPoint);
            root.Add("SelfPointSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToGrid);
            jo.Add("size_x", GridSize.X);
            jo.Add("size_y", GridSize.Y);
            jo.Add("size_z", GridSize.Z);
            root.Add("GridInfo", jo);


            jo = new JObject();
            jo.Add("DrawFaceOutline", DrawMeshEdge);
            jo.Add("FillFace", FillMesh);
            jo.Add("DrawNormal", DrawNormal);
            jo.Add("DrawAxis", DrawAxis);
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

            jo = (JObject)root["KeyMove"];
            if (jo != null)
            {
                KeyMoveUnit = jo.GetDouble("unit", KeyMoveUnit);
            }

            jo = (JObject)root["ZeroSnap"];
            if (jo != null)
            {
                SnapToZero = jo.GetBool("enable", SnapToZero);
            }

            jo = (JObject)root["LastDownSnap"];
            if (jo != null)
            {
                SnapToLastDownPoint = jo.GetBool("enable", SnapToLastDownPoint);
            }

            jo = (JObject)root["SelfPointSnap"];
            if (jo != null)
            {
                SnapToSelfPoint = jo.GetBool("enable", SnapToSelfPoint);
            }

            jo = (JObject)root["DrawSettings"];
            DrawMeshEdge = jo.GetBool("DrawFaceOutline", DrawMeshEdge);
            FillMesh = jo.GetBool("FillFace", FillMesh);
            DrawNormal = jo.GetBool("DrawNormal", DrawNormal);
            DrawAxis = jo.GetBool("DrawAxis", DrawAxis);

            jo = (JObject)root["GridInfo"];
            if (jo != null)
            {
                SnapToGrid = jo.GetBool("enable", SnapToSelfPoint);
                GridSize.X = jo.GetDouble("size_x", 10);
                GridSize.Y = jo.GetDouble("size_y", 10);
                GridSize.Z = jo.GetDouble("size_z", 10);
            }

            return true;
        }
    }
}