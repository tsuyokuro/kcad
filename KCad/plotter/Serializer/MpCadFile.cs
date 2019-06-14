using MessagePack;
using Plotter.Serializer.v1001;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Plotter.Serializer
{
    public struct CadData
    {
        public CadObjectDB DB;
        public double WorldScale;
        public PaperPageSize PageSize;

        public CadData(CadObjectDB db, double worldScale, PaperPageSize pageSize)
        {
            DB = db;
            WorldScale = worldScale;
            PageSize = pageSize;
        }
    }

    public class MpCadFile
    {
        private static byte[] Sign;
        private static byte[] Version = { 1, 0, 0, 1 };
        private static string JsonSign = "KCAD_JSON";
        private static string JsonVersion = "1001";

        static MpCadFile()
        {
            Sign = Encoding.ASCII.GetBytes("KCAD_BIN");
        }

        public static CadData? Load(string fname)
        {
            FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

            byte[] sign = new byte[Sign.Length];

            fs.Read(sign, 0, Sign.Length);

            if (!Sign.SequenceEqual<byte>(sign))
            {
                fs.Close();
                return null;
            }

            byte[] version = new byte[Version.Length];

            fs.Read(version, 0, Version.Length);

            byte[] data = new byte[fs.Length - Sign.Length - Version.Length];

            fs.Read(data, 0, data.Length);

            fs.Close();

            if (version[0] == 1 && version[1] == 0 && version[2] == 0 && version[3] == 0)
            {
                return null;
            }
            else if (version[0] == 1 && version[1] == 0 && version[2] == 0 && version[3] == 1)
            {
                MpCadData_v1001 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1001>(data);
                return MpUtil_v1001.CreateCadData_v1001(mpdata);
            }

            return null;
        }

        public static void Save(string fname, CadData cd)
        {
            MpCadData_v1001 mpcd = MpUtil_v1001.CreateMpCadData_v1001(cd);

            mpcd.MpDB.GarbageCollect();

            byte[] data = MessagePackSerializer.Serialize(mpcd);

            FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);

            fs.Write(Sign, 0, Sign.Length);
            fs.Write(Version, 0, Version.Length);
            fs.Write(data, 0, data.Length);

            fs.Close();
        }

        public static void SaveAsJson(string fname, CadData cd)
        {
            MpCadData_v1001 data = MpUtil_v1001.CreateMpCadData_v1001(cd);
            string s = MessagePackSerializer.ToJson<MpCadData_v1001>(data);

            s = s.Trim();

            s = s.Substring(1, s.Length - 2);

            string ss = @"{" + "\n" +
                        @"""header"":""" + "type=" + JsonSign + "," + "version=" + JsonVersion + @"""," + "\n" +
                        s + "\n" +
                        @"}";

            StreamWriter writer = new StreamWriter(fname);

            writer.Write(ss);

            writer.Close();
        }

        public static CadData? LoadJson(string fname)
        {
            StreamReader reader = new StreamReader(fname);

            reader.ReadLine(); // skip "{\n"
            string header = reader.ReadLine();
            Regex headerPtn = new Regex(@"version=([0-9a-fA-F]+)");

            Match m = headerPtn.Match(header);

            string version = "";

            if (m.Groups.Count >= 1)
            {
                version = m.Groups[1].Value;
            }

            string js = reader.ReadToEnd();
            reader.Close();

            js = js.Trim();
            js = js.Substring(0, js.Length - 1);
            js = "{" + js + "}";

            byte[] bin = MessagePackSerializer.FromJson(js);

            if (version == "1001")
            {
                MpCadData_v1001 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1001>(bin);

                CadData cd = new CadData(
                    mpcd.GetDB(),
                    mpcd.ViewInfo.WorldScale,
                    mpcd.ViewInfo.PaperSettings.GetPaperPageSize()
                    );

                return cd;
            }

            return null;
        }
    }
}
