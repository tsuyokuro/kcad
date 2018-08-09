//#define COPY_AS_JSON

using KCad;
using MessagePack;
using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CadDataTypes;

namespace Plotter.Controller
{
    public partial class PlotterController
    {
        #region "Copy and paste"

#if COPY_AS_JSON
        public void Copy()
        {
            CopyFiguresAsJson();
        }

        public void Paste()
        {
            PasteFiguresAsJson();
        }

        public void CopyFiguresAsJson()
        {
            var temp = GetSelectedFigureList();

            var figList = new List<CadFigure>();

            temp.ForEach(fig =>
            {
                if (fig.Parent == null)
                {
                    figList.Add(fig);
                }
            });

            if (figList.Count == 0)
            {
                return;
            }

            JObject jo = CadJson.ToJson.FigListToJsonForClipboard(figList);

            string s = jo.ToString();

            Clipboard.SetData(CadClipBoard.TypeNameJson, s);
            //Clipboard.SetText(s);
        }

        public void PasteFiguresAsJson()
        {
            if (!Clipboard.ContainsData("List.CadFiguer"))
            {
                return;
            }

            CadVector pp = LastDownPoint;
                                
            string s = (string)Clipboard.GetData(CadClipBoard.TypeNameJson);

            JObject jo = JObject.Parse(s);

            List<CadFigure> figList = CadJson.FromJson.FigListFromJsonForClipboard(jo);

            MinMax3D mm3d = CadUtil.GetFigureMinMaxIncludeChild(figList);

            CadVector d = pp - mm3d.GetMinAsVector();

            //d.z = 0;

            CadOpeList opeRoot = CadOpe.CreateListOpe();

            foreach (CadFigure fig in figList)
            {
                PasteFigure(fig, d);
                CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            HistoryMan.foward(opeRoot);

            UpdateTreeView(true);
        }

#else // Not defined COPY_AS_JSON

        public void Copy()
        {
            CopyFiguresAsBin();
        }

        public void Paste()
        {
            PasteFiguresAsBin();
        }

        public void CopyFiguresAsBin()
        {
            var temp = GetSelectedFigureList();

            var figList = CadFigure.Util.GetRootFigList(temp);

            if (figList.Count == 0)
            {
                return;
            }

            List<MpFigure> mpfigList = MpUtil.FigureListToMp(figList, true);

            byte[] bin = MessagePackSerializer.Serialize(mpfigList);

            Clipboard.SetData(CadClipBoard.TypeNameBin, bin);

            // For debug
            //string js = MessagePackSerializer.ToJson(bin);
            //Clipboard.SetText(js);
        }

        public void PasteFiguresAsBin()
        {
            if (!Clipboard.ContainsData(CadClipBoard.TypeNameBin))
            {
                return;
            }
            byte[] bin = (byte[])Clipboard.GetData(CadClipBoard.TypeNameBin);

            List<MpFigure> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure>>(bin);

            List<CadFigure> figList = MpUtil.FigureListFromMp(mpfigList);


            // Pase figures in fig list
            CadVector pp = LastDownPoint;

            MinMax3D mm3d = CadUtil.GetFigureMinMaxIncludeChild(figList);

            CadVector d = pp - mm3d.GetMinAsVector();

            CadOpeList opeRoot = CadOpe.CreateListOpe();

            foreach (CadFigure fig in figList)
            {
                PasteFigure(fig, d);
                CurrentLayer.AddFigure(fig);    // 子ObjectはLayerに追加しない

                CadOpe ope = CadOpe.CreateAddFigureOpe(CurrentLayer.ID, fig.ID);
                opeRoot.OpeList.Add(ope);
            }

            HistoryMan.foward(opeRoot);

            UpdateTreeView(true);
        }
#endif

        private void PasteFigure(CadFigure fig, CadVector delta)
        {
            fig.MoveAllPoints(CurrentDC, delta);
            mDB.AddFigure(fig);

            if (fig.ChildList != null)
            {
                foreach (CadFigure child in fig.ChildList)
                {
                    PasteFigure(child, delta);
                }
            }
        }

        #endregion

        #region "Json file access"
        public void SaveToJsonFile(String fname)
        {
            StreamWriter writer = new StreamWriter(fname);

            JObject jo = CadJson.ToJson.DbToJson(mDB);

            //writer.Write(jo.ToString(Newtonsoft.Json.Formatting.None));
            writer.Write(jo.ToString());
            writer.Close();
        }

        public async void LoadFromJsonFileAsync(String fname)
        {
            CadObjectDB db = await Task<CadObjectDB>.Run(() =>
            {
                return DBFromJsonFile(fname);
            });

            if (db == null)
            {
                return;
            }

            mDB = db;

            HistoryMan = new HistoryManager(mDB);

            NotifyLayerInfo();

            UpdateTreeView(true);

            Redraw(CurrentDC);
        }

        public void LoadFromJsonFile(String fname)
        {
            CadObjectDB db = DBFromJsonFile(fname);

            if (db == null)
            {
                return;
            }

            mDB = db;

            HistoryMan = new HistoryManager(mDB);

            NotifyLayerInfo();

            UpdateTreeView(true);

            Redraw(CurrentDC);
        }

        public CadObjectDB DBFromJsonFile(string fname)
        {
            ItConsole.println("Loading file: " + fname);

            StreamReader reader = new StreamReader(fname);

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jo = JObject.Parse(js);

            string version = CadJson.FromJson.VersionStringFromJson(jo);

            if (version == null)
            {
                ItConsole.println("Bat file format");
                return null;
            }

            ItConsole.println("Format version: " + version);

            CadObjectDB db = CadJson.FromJson.DbFromJson(jo);
            return db;
        }
        #endregion

        #region "MessagePack file access"
        public void SaveToMsgPackFile(String fname)
        {
            MpCadData data = MpCadData.Create(DB);

            byte[] bin_data = MessagePackSerializer.Serialize(data);

            MpCadFile.Save(fname, bin_data);
        }

        public void LoadFromMsgPackFile(String fname)
        {
            byte[] bin = MpCadFile.Load(fname);

            if (bin == null)
            {
                return;
            }

            MpCadData mpdata = MessagePackSerializer.Deserialize<MpCadData>(bin);

            if (mpdata == null)
            {
                return;
            }

            mDB = mpdata.GetDB();

            HistoryMan = new HistoryManager(mDB);

            NotifyLayerInfo();

            UpdateTreeView(true);

            Redraw(CurrentDC);
        }
        #endregion
    }
}