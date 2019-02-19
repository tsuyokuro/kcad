using Newtonsoft.Json.Linq;
using Plotter.Serializer;
using System.ComponentModel;
using System.IO;

namespace Plotter
{
    public partial class PlotterViewModel : INotifyPropertyChanged
    {
        private string mCurrentFileName = null;

        public string CurrentFileName
        {
            get => mCurrentFileName;

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

        private void SaveToMsgPackFile(string fname)
        {
            CadData cd = new CadData(
                                mController.DB,
                                mController.CurrentDC.WorldScale,
                                mController.PageSize
                                );

            MpCadFile.Save(fname, cd);

            CurrentFileName = fname;
        }

        private void LoadFromMsgPackFile(string fname)
        {
            CadData? cd = MpCadFile.Load(fname);

            if (cd == null)
            {
                return;
            }

            CadData rcd = cd.Value;


            SetWorldScale(rcd.WorldScale);

            mController.PageSize = rcd.PageSize;

            mController.SetDB(rcd.DB);

            CurrentFileName = fname;
        }


        private void SaveToMsgPackJsonFile(string fname)
        {
            CadData cd = new CadData(
                mController.DB,
                mController.CurrentDC.WorldScale,
                mController.PageSize);


            MpCadFile.SaveAsJson(fname, cd);

            CurrentFileName = fname;
        }

        private void LoadFromMsgPackJsonFile(string fname)
        {
            CadData? cd = MpCadFile.LoadJson(fname);

            if (cd == null)
            {
                return;
            }

            CadData rcd = cd.Value;

            SetWorldScale(rcd.WorldScale);

            mController.PageSize = rcd.PageSize;

            mController.SetDB(rcd.DB);

            CurrentFileName = fname;
        }
        #endregion
    }
}