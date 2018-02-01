using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    class CadDxfLoader
    {
        public enum States
        {
            ON_GOING,
            COMPLETE,
            ERROR,
        }

        public delegate void Progress(States state, int percent, CadObjectDB db);

        public async void AsyncLoad(string fname, double scale, Progress progress)
        {
            CadObjectDB db = await Task.Run(() => Load(fname, scale));

            progress(States.COMPLETE, 100, db);
        }

        private enum DxfState
        {
            STATE_NONE,
            STATE_3DFACE,
        }

        public int TotalPointCount;

        public int TotalFaceCount;

        public CadObjectDB Load(string fname, double scale)
        {
            TotalPointCount = 0;
            TotalFaceCount = 0;

            CadObjectDB db = new CadObjectDB();

            CadLayer layer = db.NewLayer();
            db.LayerList.Add(layer);

            db.CurrentLayer = layer;

            StreamReader reader = new StreamReader(fname);

            string L1;
            string L2;

            DxfState state = DxfState.STATE_NONE;
            int valCnt = 0;


            double[] val = new double[3];

            int code;

            VectorList pointList = new VectorList();

            while (!reader.EndOfStream)
            {
                L1 = reader.ReadLine();
                L2 = reader.ReadLine();

                code = Int32.Parse(L1);
                L2 = L2.Trim();


                if (code == 0)
                {
                    if (L2 == "3DFACE")
                    {
                        state = DxfState.STATE_3DFACE;
                        valCnt = 0;
                    }

                    if (pointList.Count > 0)
                    {
                        AddFace(db, pointList);
                        TotalFaceCount++;

                        pointList.Clear();
                    }

                    if (valCnt != 0)
                    {
                        state = 0;
                    }
                }

                if (state == DxfState.STATE_3DFACE)
                {
                    if (code < 10)
                    {
                        continue;
                    }

                    val[valCnt] = Double.Parse(L2) * scale;
                    valCnt++;

                    if (valCnt >= 3)
                    {
                        pointList.Add(CadVector.Create(val[0], val[1], val[2]));
                        valCnt = 0;

                        TotalPointCount++;
                    }

                    continue;
                }
            }

            return db;
        }

        private void AddFace(CadObjectDB db, VectorList plist)
        {
            if (plist.Count == 0)
            {
                return;
            }

            CadFigure fig = db.NewFigure(CadFigure.Types.POLY_LINES);

            foreach (CadVector v in plist)
            {
                fig.AddPoint(v);
            }

            fig.IsLoop = true;

            CadUtil.SetNormal(fig);

            db.CurrentLayer.AddFigure(fig);
        }
    }
}
