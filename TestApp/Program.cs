using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            XDocument doc = new XDocument();

            XDocumentType docType = new XDocumentType(
                @"svg", @" -//W3C//DTD SVG 1.1//EN",
                @"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd",
                null);


            string pd = "M 100 100 L 300 100 L 200 300 z";

            XElement fig = new XElement("path", new XAttribute("d", pd));

            doc.Add(docType, fig);


            Console.WriteLine(doc.ToString());

            Console.WriteLine("end");

            Console.ReadLine();
        }
    }
}
