﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public static class JsonExtends
    {
        public static double GetDouble(this JObject jo, string key, double defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            return (double)jt;
        }

        public static bool GetBool(this JObject jo, string key, bool defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            return (bool)jt;
        }
    }
}
