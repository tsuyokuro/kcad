using MessagePack;
using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Plotter
{
    public partial class PlotterViewModel : INotifyPropertyChanged
    {
        private void SaveFile(string fname)
        {
            if (fname.EndsWith(".txt"))
            {
                SaveToJsonFile(fname);
            }
            else
            {
                SaveToMsgPackFile(fname);
            }
        }

        private void LoadFile(string fname)
        {
            if (fname.EndsWith(".txt"))
            {
                LoadFromJsonFile(fname);
                //mController.LoadFromJsonFileAsync(fname);
            }
            else
            {
                LoadFromMsgPackFile(fname);
            }
        }


        #region "Json file access"
        public void SaveToJsonFile(string fname)
        {
            StreamWriter writer = new StreamWriter(fname);

            JObject jroot = new JObject();

            jroot.Add("WorldScale", mController.CurrentDC.WorldScale);

            JObject jo = CadJson.ToJson.DbToJson(mController.DB);

            jroot.Add("DB", jo);

            //writer.Write(jo.ToString(Newtonsoft.Json.Formatting.None));
            writer.Write(jroot.ToString());
            writer.Close();
        }

        public void LoadFromJsonFile(string fname)
        {
            ItConsole.println("Loading file: " + fname);

            StreamReader reader = new StreamReader(fname);

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jroot = JObject.Parse(js);

            JObject jdb = (JObject)jroot["DB"];

            double worldScale = 1.0;

            if (jdb == null)
            {
                jdb = jroot;
            }
            else
            {
                worldScale = (double)jroot["WorldScale"];
                if (worldScale == 0)
                {
                    worldScale = 1.0;
                }
            }

            CadObjectDB db = CadJson.FromJson.DbFromJson(jdb);

            mController.SetDB(db);
            SetWorldScale(worldScale);
        }

        public CadObjectDB DBFromJsonFile(string fname)
        {
            ItConsole.println("Loading file: " + fname);

            StreamReader reader = new StreamReader(fname);

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jo = JObject.Parse(js);

            CadObjectDB db = CadJson.FromJson.DbFromJson(jo);
            return db;
        }
        #endregion




        #region "MessagePack file access"
        public void SaveToMsgPackFile(string fname)
        {
            MpCadData data = MpCadData.Create(mController.DB);

            data.ViewInfo.WorldScale = mController.CurrentDC.WorldScale;

            data.ViewInfo.PaperSettings.Set(mController.PageSize);

            byte[] bin_data = MessagePackSerializer.Serialize(data);

            MpCadFile.Save(fname, bin_data);
        }

        public void LoadFromMsgPackFile(string fname)
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

            MpViewInfo viewInfo = mpdata.ViewInfo;

            double worldScale = 0;

            PaperPageSize pps = null;

            if (viewInfo != null)
            {
                worldScale = viewInfo.WorldScale;

                if (viewInfo.PaperSettings != null)
                {
                    pps = viewInfo.PaperSettings.GetPaperPageSize();
                }
            }


            if (worldScale == 0)
            {
                worldScale = 1.0;
            }

            SetWorldScale(worldScale);


            if (pps == null)
            {
                pps = new PaperPageSize();
            }

            mController.PageSize = pps;


            mController.SetDB(mpdata.GetDB());
        }
        #endregion
    }
}