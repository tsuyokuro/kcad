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
        private void splitToTriangle()
        {
            TempFigureList.Clear();

            CadFigureBonder fa = new CadFigureBonder(mDB, CurrentLayer);

            var res = fa.bond(mSelList.List);

            if (!res.isValid())
            {
                return;
            }

            if (res.AddList.Count == 0)
            {
                return;
            }

            CadFigureAssembler.ResultItem ri = res.AddList[0];

            TempFigureList.Add(ri.Figure);

            splitToTriangle(ri.Figure);
        }

        private List<CadFigure> splitToTriangle(CadFigure fig)
        {
            CadPoint p0 = default(CadPoint);

            CadFigure tfig = new CadFigure();

            tfig.copyPoints(fig);

            var triangles = new List<CadFigure>();

            int i1 = -1;

            int state = 0;

            CadFigure triangle;

            i1 = CadUtil.findMaxDistantPointIndex(p0, tfig.PointList);

            if (i1 == -1)
            {
                return triangles;
            }

            triangle = getTriangleWithCenterPoint(tfig, i1);

            CadPoint tp0 = triangle.PointList[0];
            CadPoint tp1 = triangle.PointList[1];
            CadPoint tp2 = triangle.PointList[2];

            double dir = CadUtil.crossProduct2D(tp1, tp0, tp2);
            double currentDir = 0;

            while (tfig.PointCount > 3)
            {
                if (state == 0)
                {
                    i1 = CadUtil.findMaxDistantPointIndex(p0, tfig.PointList);
                    if (i1 == -1)
                    {
                        return triangles;
                    }
                }

                triangle = getTriangleWithCenterPoint(tfig, i1);

                tp0 = triangle.PointList[0];
                tp1 = triangle.PointList[1];
                tp2 = triangle.PointList[2];

                currentDir = CadUtil.crossProduct2D(tp1, tp0, tp2);

                bool hasIn = isFigPointInTriangle(tfig, triangle);
                if (!hasIn && (Math.Sign(dir) == Math.Sign(currentDir)))
                {
                    triangles.Add(triangle);
                    tfig.PointList.RemoveAt(i1);
                    state = 0;
                    continue;
                }

                if (state == 0)
                {
                    state = 1;
                    i1 = 0;
                }
                else if (state == 1)
                {
                    i1++;
                    if (i1 >= tfig.PointCount)
                    {
                        break;
                    }
                }
            }

            if (tfig.PointCount == 3)
            {
                triangle = new CadFigure(CadFigure.Types.POLY_LINES);

                triangle.copyPoints(tfig);
                triangle.Closed = true;

                triangles.Add(triangle);
            }

            return triangles;
        }

        private CadFigure getTriangleWithCenterPoint(CadFigure fig, int cpIndex)
        {
            int i1 = cpIndex;
            int endi = fig.PointCount - 1;

            int i0 = i1 - 1;
            int i2 = i1 + 1;

            if (i0 < 0) { i0 = endi; }
            if (i2 > endi) { i2 = 0; }

            var triangle = new CadFigure(CadFigure.Types.POLY_LINES);

            CadPoint tp0 = fig.PointList[i0];
            CadPoint tp1 = fig.PointList[i1];
            CadPoint tp2 = fig.PointList[i2];

            triangle.addPoint(tp0);
            triangle.addPoint(tp1);
            triangle.addPoint(tp2);

            triangle.Closed = true;

            return triangle;
        }

        private bool isFigPointInTriangle(CadFigure check, CadFigure triangle)
        {
            var tps = triangle.PointList;

            foreach (CadPoint cp in check.PointList)
            {
                if　(
                    cp.coordEquals(tps[0]) ||
                    cp.coordEquals(tps[1]) ||
                    cp.coordEquals(tps[2])
                    )
                {
                    continue;
                }

                bool ret = CadUtil.isPointInTriangle(cp, triangle.PointList);
                if (ret)
                {
                    return true;
                }
            }

            return false;
        }


        private void calcTest(DrawContext dc)
        {
            TempFigureList.Clear();

            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];

            CadFigure fig = si.Figure;

            CadPoint pt = mFreeDownPoint.Value;

            DebugOut dout = new DebugOut();

            double res = CadUtil.getTriangleArea(fig.PointList);

            dout.println("res=" + res);

            CadPoint gp = CadUtil.getTriangleCentroid(fig.PointList);

            CadFigure tfig = new CadFigure(CadFigure.Types.POLY_LINES);
            tfig.addPoint(gp);

            TempFigureList.Add(tfig);

            clear(dc);
            draw(dc);
        }

        private void splitTriangleTest(DrawContext dc)
        {
            TempFigureList.Clear();

            if (mSelList.List.Count == 0)
            {
                return;
            }

            SelectItem si = mSelList.List[0];
            CadFigure fig = si.Figure;

            List<CadFigure> triangles = splitToTriangle(fig);

            triangles.ForEach(a=>TempFigureList.Add(a));

            draw(dc);

            Centroid gp = CadUtil.getTriangleListCentroid(triangles);

            CadFigure tfig = new CadFigure(CadFigure.Types.POLY_LINES);
            tfig.addPoint(gp.Point);

            TempFigureList.Add(tfig);
        }

        private void areaCollectorTest()
        {
            TempFigureList.Clear();

            AreaCollecter ac = new AreaCollecter(mDB);
            List<CadFigure> figs = ac.collect(mSelList.List);

            figs.ForEach(a => { TempFigureList.Add(a); });

            RequestRedraw();
        }

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
            else if (s == "dump_temp")
            {
                DebugOut dout = new DebugOut();

                dout.println("dump_temp {");
                dout.Indent++;
                foreach (CadFigure fig in TempFigureList)
                {
                    fig.dump(dout);
                }
                dout.Indent--;
                dout.println("}");
            }

            else if (s == "calct")
            {
                calcTest(dc);
            }

            else if (s == "stt")
            {
                splitTriangleTest(dc);
            }

            else if (s == "act")
            {
                areaCollectorTest();
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
            else if (s == "tri")
            {
                splitToTriangle();
            }
            else if (s == "clean temp")
            {
                TempFigureList.Clear();
            }
            else if (s == "dump figs")
            {
                DebugOut dout = new DebugOut();

                dout.println("fig_list {");
                dout.Indent++;
                foreach (CadLayer layer in mDB.LayerList)
                {
                    List<CadFigure> figList = layer.FigureList;

                    dout.println("Layer:" + layer.Name + "{");
                    dout.Indent++;

                    foreach (CadFigure fig in figList)
                    {
                        Log.d("ID={0:d} Type={1:s} class={2:s}", fig.ID, fig.Type.ToString(), fig.getBehaviorType().Name);
                    }

                    dout.Indent--;
                    dout.println("}");
                }

                dout.Indent--;
                dout.println("}");
            }
            else if (s == "dump sels")
            {
                DebugOut dout = new DebugOut();
                mSelList.dump(dout);
            }
            else if (s == "dump selsegs")
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
