using MyCollections;
using Newtonsoft.Json.Linq;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp02
{
    class Program
    {
        static void test001()
        {
            StreamReader reader = new StreamReader(@"..\..\..\TestData\TestData.txt");

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jo = JObject.Parse(js);

            CadObjectDB db = CadJson.FromJson.DbFromJson(jo);

            CadLayer layer = db.CurrentLayer;
        }

        static void Main(string[] args)
        {
            test001();
        }
    }
}
