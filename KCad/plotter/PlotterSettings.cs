using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using CadDataTypes;

namespace Plotter
{
    public static class SettingsHolder
    {
        public static PlotterSettings Settings = new PlotterSettings();
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