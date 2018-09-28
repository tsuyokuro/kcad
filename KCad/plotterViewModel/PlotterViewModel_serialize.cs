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
        private string mCurrentFileName = null;

        public string CurrentFileName
        {
            get
            {
                return mCurrentFileName;
            }

            private set
            {
                mCurrentFileName = value;

                if (mCurrentFileName != null)
                {
                    mMainWindow.Title = "KCad " + mCurrentFileName;
                }
                else
                {
                    mMainWindow.Title = "KCad";
                }
            }
        }

        private void SaveFile(string fname)
        {
            if (fname.EndsWith(".txt"))
            {
                SaveToJsonFile(fname);
            }
            else if (fname.EndsWith(".kjs"))
            {
                SaveToMsgPackJsonFile(fname);
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
            }
            else if (fname.EndsWith(".kjs"))
            {
                LoadFromMsgPackJsonFile(fname);
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

            writer.Write(jroot.ToString());
            writer.Close();

            CurrentFileName = fname;
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

            CurrentFileName = fname;
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
        private MpCadData ToMpCadData()
        {
            MpCadData data = MpCadData.Create(mController.DB);

            data.ViewInfo.WorldScale = mController.CurrentDC.WorldScale;

            data.ViewInfo.PaperSettings.Set(mController.PageSize);

            return data;
        }

        private void FromMpCadData(MpCadData mpdata)
        {
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


        private void SaveToMsgPackFile(string fname)
        {
            MpCadData data = ToMpCadData();

            byte[] bin_data = MessagePackSerializer.Serialize(data);

            MpCadFile.Save(fname, bin_data);

            CurrentFileName = fname;
        }

        private void LoadFromMsgPackFile(string fname)
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

            FromMpCadData(mpdata);

            CurrentFileName = fname;
        }


        private void SaveToMsgPackJsonFile(string fname)
        {
            MpCadData data = ToMpCadData();
            string s = MessagePackSerializer.ToJson<MpCadData>(data);

            StreamWriter writer = new StreamWriter(fname);
            writer.Write(s);

            writer.Close();

            CurrentFileName = fname;
        }

        private void LoadFromMsgPackJsonFile(string fname)
        {
            StreamReader reader = new StreamReader(fname);

            string js = reader.ReadToEnd();

            reader.Close();


            byte[] bin = MessagePackSerializer.FromJson(js);

            MpCadData mpdata = MessagePackSerializer.Deserialize<MpCadData>(bin);

            if (mpdata == null)
            {
                CurrentFileName = "";
                return;
            }

            FromMpCadData(mpdata);

            CurrentFileName = fname;
        }
        #endregion
    }
}