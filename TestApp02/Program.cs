using MessagePack;
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
        static CadLayer getTestData_Layer()
        {
            StreamReader reader = new StreamReader(@"..\..\..\TestData\TestData.txt");

            var js = reader.ReadToEnd();

            reader.Close();

            JObject jo = JObject.Parse(js);

            CadObjectDB db = CadJson.FromJson.DbFromJson(jo);

            CadLayer layer = db.CurrentLayer;

            return layer;
        }

        static void test001()
        {
            CadLayer layer = getTestData_Layer();
            CadFigure fig = layer.FigureList[0];

            MpFigure mpFig = MpFigure.Create(fig);


            byte[] bfig = LZ4MessagePackSerializer.Serialize(mpFig);

            string js = LZ4MessagePackSerializer.ToJson(bfig);

            //printJson(js);
            Console.WriteLine(js);

            MpFigure rmpFig = LZ4MessagePackSerializer.Deserialize<MpFigure>(bfig);


            CadFigure rfig = rmpFig.Restore();

        }

        static void printJson(string js)
        {
            JObject jo = JObject.Parse(js);

            Console.WriteLine(jo.ToString());
        }



        static void Main(string[] args)
        {
            test001();

            Console.ReadLine();
        }
    }
}
