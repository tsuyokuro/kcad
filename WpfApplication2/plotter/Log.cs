
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Plotter
{
    public class Log
    {
        [Conditional("LOG_INFO")]
        public static void i(String s, params Object[] vals)
        {
            Console.WriteLine(s, vals);
        }

        [Conditional("LOG_INFO")]
        public static void ir(String s)
        {
            Console.Write(s);
        }

        [Conditional("LOG_DEBUG")]
        public static void d(String s, params Object[] vals)
        {
            Console.WriteLine(s, vals);
        }

        [Conditional("LOG_DEBUG")]
        public static void dr(String s)
        {
            Console.Write(s);
        }

        public static void e(String s, params Object[] vals)
        {
            Console.WriteLine(s, vals);
        }

        public static void er(String s)
        {
            Console.Write(s);
        }

        public static void tmp(String s, params Object[] vals)
        {
            Console.WriteLine(s, vals);
        }
    }
}
