using OpenTK;
using Plotter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DrawPen pen = DrawPen.New(Color.FromArgb(0x45, 1, 2, 3), 0);

            uint a = (uint)pen.Argb & 0xff000000;

            Console.WriteLine("end");

            Console.ReadLine();
        }
    }
}
