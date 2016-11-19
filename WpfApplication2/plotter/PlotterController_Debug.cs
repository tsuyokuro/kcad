#define LOG_INFO
#define LOG_DEBUG

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Plotter
{
    public partial class PlotterController
    {
        private void test001(DrawContext dc, String arg)
        {
            draw(dc);

            MarkSeg seg = mSelectedSegs.LastSel;

            if (seg.FigureID == 0)
            {
                return;
            }

            CadFigure fig = mDB.getFigure(seg.FigureID);

            CadPoint a = fig.getPointAt(seg.PtIndexA);
            CadPoint b = fig.getPointAt(seg.PtIndexB);

            CrossInfo ret = CadUtil.getNormCross2D(a, b, mRawPos);

            if (ret.isCross)
            {
                Drawer.drawLine(dc, dc.Tools.AuxiliaryLinePen, mRawPos, ret.CrossPoint);
            }
        }

        public void debugCommand(DrawContext dc, string s)
        {
            if (s == "test")
            {
                JObject root = new JObject();

                JArray array = new JArray();

                root.Add("version", "1.0");

                JObject item = new JObject();
                item.Add("ID", 11);
                item.Add("Type", 1);

                array.Add(item);

                item = new JObject();
                item.Add("ID", 12);
                item.Add("Type", 2);

                array.Add(item);

                root.Add("list", array);

                String js = root.ToString();

                Log.dr(js + "\n");
            }
            else if (s == "test2")
            {
                JObject jo = mDB.ToJson();

                String js = jo.ToString();

                Log.dr(js + "\n");

                CadObjectDB db = new CadObjectDB();

                db.FromJson(jo);

                mDB = db;

                draw(dc);
            }
            else if (s == "test3")
            {
                Log.d("" + Drawer.Factorial(0));
                Log.d("" + Drawer.Factorial(1));
                Log.d("" + Drawer.Factorial(2));
                Log.d("" + Drawer.Factorial(3));
                Log.d("" + Drawer.Factorial(4));
            }
            else if (s == "fig_list")
            {
                List<CadFigure> figList = CurrentLayer.FigureList;

                Log.d("Figure List");

                foreach (CadFigure fig in figList)
                {
                    Log.d("ID={0:d} Type={1:s} class={2:s}", fig.ID, fig.Type.ToString(), fig.getBehaviorType().Name);
                }
            }
            else if (s == "sel_list")
            {
                DebugOut dout = new DebugOut();
                mSelList.dump(dout);
            }
            else if (s == "selseg_list")
            {
                DebugOut dout = new DebugOut();
                mSelList.dump(dout);
            }
            else if (s == "dump")
            {
                DebugOut dst = new DebugOut();
                dump(dst);
            }
            else if (s == "save")
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("f:\\work\\test.kcd", FileMode.Create, FileAccess.Write, FileShare.None);

                formatter.Serialize(stream, mDB);

                stream.Close();
            }
            else if (s == "load")
            {
                IFormatter formatter = new BinaryFormatter();

                Stream stream = new FileStream("f:\\work\\test.kcd", FileMode.Open, FileAccess.Read, FileShare.Read);

                CadObjectDB db = (CadObjectDB)formatter.Deserialize(stream);

                stream.Close();

                DebugOut dout = new DebugOut();
                db.dump(dout);

                mDB = db;
            }
            else if (s == "savej")
            {
                StreamWriter writer = new StreamWriter("f:\\work\\test.txt");

                Stopwatch sw = new Stopwatch();

                sw.Start();
                JObject jo = mDB.ToJson();
                sw.Stop();

                DebugOut dout = new DebugOut();
                dout.println("ToJson time:" + sw.Elapsed);

                writer.Write(jo.ToString());

                writer.Close();
            }
            else if (s == "loadj")
            {
                StreamReader reader = new StreamReader("f:\\work\\test.txt");

                String js = reader.ReadToEnd();

                reader.Close();

                JObject jo = JObject.Parse(js);


                CadObjectDB db = new CadObjectDB();


                Stopwatch sw = new Stopwatch();
                sw.Start();
                db.FromJson(jo);
                sw.Stop();

                DebugOut dout = new DebugOut();
                dout.println("FromJson time:" + sw.Elapsed);


                db.dump(dout);

                mDB = db;

                clear(dc);
                draw(dc);
            }
            else if (s == "copy")
            {
                copyFigures();
            }
            else if (s == "paste")
            {
                pasteFigure();
            }
        }
        public void dump(DebugOut dst)
        {
            DebugOut dout = new DebugOut();
            mDB.dump(dout);
        }
    }
}
