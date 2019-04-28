﻿using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.IO;
using System.Reflection;
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
                if (SettingsHolder.Settings.SnapToGrid != value)
                {
                    SettingsHolder.Settings.SnapToGrid = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));

                    Redraw();
                }
            }

            get => SettingsHolder.Settings.SnapToGrid;
        }

        public bool SnapToPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            }

            get => SettingsHolder.Settings.SnapToPoint;
        }

        public bool SnapToSegment
        {
            set
            {
                SettingsHolder.Settings.SnapToSegment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            }

            get => SettingsHolder.Settings.SnapToSegment;
        }

        public bool SnapToLine
        {
            set
            {
                SettingsHolder.Settings.SnapToLine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            }

            get => SettingsHolder.Settings.SnapToLine;
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

            get => SettingsHolder.Settings.FilterTreeView;
        }

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

        public double InitialMoveLimit
        {
            set
            {
                SettingsHolder.Settings.InitialMoveLimit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialMoveLimit)));
            }

            get => SettingsHolder.Settings.InitialMoveLimit;
        }

        public bool SnapToZero
        {
            set
            {
                SettingsHolder.Settings.SnapToZero = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToZero)));
            }

            get => SettingsHolder.Settings.SnapToZero;
        }

        public bool SnapToLastDownPoint
        {
            set
            {
                SettingsHolder.Settings.SnapToLastDownPoint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLastDownPoint)));
            }

            get => SettingsHolder.Settings.SnapToLastDownPoint;
        }

        public CadVector GridSize
        {
            set
            {
                SettingsHolder.Settings.GridSize = value;
                Controller.Grid.GridSize = value;
            }

            get => SettingsHolder.Settings.GridSize;
        }

        public double PointSnapRange
        {
            set
            {
                SettingsHolder.Settings.PointSnapRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointSnapRange)));
            }

            get => SettingsHolder.Settings.PointSnapRange;
        }

        public double LineSnapRange
        {
            set
            {
                SettingsHolder.Settings.LineSnapRange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineSnapRange)));
            }

            get => SettingsHolder.Settings.LineSnapRange;
        }

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

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawMeshEdge)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillMesh)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterTreeView)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToZero)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLastDownPoint)));

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

        public double KeyMoveUnit = 1.0;

        public bool DrawMeshEdge = true;

        public bool FillMesh = true;

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
            jo.Add("unit", KeyMoveUnit);
            root.Add("KeyMove", jo);

            jo = new JObject();
            jo.Add("enable", SnapToZero);
            root.Add("ZeroSnap", jo);

            jo = new JObject();
            jo.Add("enable", SnapToLastDownPoint);
            root.Add("LastDownSnap", jo);


            jo = new JObject();
            jo.Add("enable", SnapToGrid);
            jo.Add("size_x", GridSize.x);
            jo.Add("size_y", GridSize.z);
            jo.Add("size_z", GridSize.x);
            root.Add("GridInfo", jo);


            jo = new JObject();
            jo.Add("DrawFaceOutline", DrawMeshEdge);
            jo.Add("FillFace", FillMesh);
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

            jo = (JObject)root["DrawSettings"];
            DrawMeshEdge = jo.GetBool("DrawFaceOutline", DrawMeshEdge);
            FillMesh = jo.GetBool("FillFace", FillMesh);

            return true;
        }
    }
}