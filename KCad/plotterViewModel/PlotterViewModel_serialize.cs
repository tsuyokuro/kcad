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

            JObject jo = CadJson.ToJson.DbToJson(mController.DB);

            //writer.Write(jo.ToString(Newtonsoft.Json.Formatting.None));
            writer.Write(jo.ToString());
            writer.Close();
        }

        public async void LoadFromJsonFileAsync(string fname)
        {
            CadObjectDB db = await Task<CadObjectDB>.Run(() =>
            {
                return DBFromJsonFile(fname);
            });

            if (db == null)
            {
                return;
            }

            mController.SetDB(db);

        }

        public void LoadFromJsonFile(string fname)
        {
            CadObjectDB db = DBFromJsonFile(fname);

            if (db == null)
            {
                return;
            }

            mController.SetDB(db);
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
        public void SaveToMsgPackFile(string fname)
        {
            MpCadData data = MpCadData.Create(mController.DB);

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

            mController.SetDB(mpdata.GetDB());
        }
        #endregion
    }
}