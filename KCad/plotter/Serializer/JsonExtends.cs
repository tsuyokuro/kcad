using Newtonsoft.Json.Linq;

namespace Plotter.Serializer
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
