using MessagePack;
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
                MpCadData_v1000 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1000>(data);
                return CreateCadData(mpdata);
            }
            else
            {
                MpCadData_Latest mpdata = MessagePackSerializer.Deserialize<MpCadData_Latest>(data);
                return CreateCadData(mpdata);
            }
        }

        public static void Save(string fname, CadData cd)
        {
            MpCadData_Latest mpcd = CreateMpCadData_Latest(cd);

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
            MpCadData_Latest data = CreateMpCadData_Latest(cd);
            string s = MessagePackSerializer.ToJson<MpCadData_Latest>(data);

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
                MpCadData_Latest mpcd = MessagePackSerializer.Deserialize<MpCadData_Latest>(bin);

                CadData cd = new CadData(
                    mpcd.GetDB(),
                    mpcd.ViewInfo.WorldScale,
                    mpcd.ViewInfo.PaperSettings.GetPaperPageSize()
                    );

                return cd;
            }

            return null;
        }

        private static MpCadData_Latest CreateMpCadData_Latest(CadData cd)
        {
            MpCadData_Latest data = MpCadData_Latest.Create(cd.DB);

            data.ViewInfo.WorldScale = cd.WorldScale;

            data.ViewInfo.PaperSettings.Set(cd.PageSize);

            return data;
        }

        private static CadData CreateCadData(MpCadData_v1000 mpcd)
        {
            CadData cd = new CadData();

            MpViewInfo viewInfo = mpcd.ViewInfo;

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

            cd.WorldScale = worldScale;


            if (pps == null)
            {
                pps = new PaperPageSize();
            }

            cd.PageSize = pps;

            cd.DB = mpcd.GetDB();

            return cd;
        }

        private static CadData CreateCadData(MpCadData_Latest mpcd)
        {
            CadData cd = new CadData();

            MpViewInfo viewInfo = mpcd.ViewInfo;

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

            cd.WorldScale = worldScale;


            if (pps == null)
            {
                pps = new PaperPageSize();
            }

            cd.PageSize = pps;

            cd.DB = mpcd.GetDB();

            return cd;
        }
    }
}
