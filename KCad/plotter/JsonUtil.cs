using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Plotter
{
    public static class JsonUtil
    {
        public static JArray DictToJsonList<Tkey, TValue>(Dictionary<Tkey, TValue> map, uint version)
        {
            JArray ja = new JArray();

            List<Tkey> ids = new List<Tkey>(map.Keys);

            foreach (Tkey id in ids)
            {
                dynamic x = map[id];
                ja.Add(x.ToJson(version));
            }

            return ja;
        }

        public static JArray ListToJsonList<T>(IReadOnlyList<T> list, uint version)
        {
            JArray ja = new JArray();

            foreach (T item in list)
            {
                dynamic x = item;
                ja.Add(x.ToJson(version));
            }

            return ja;
        }

        public static JArray ListToJsonIdList<T>(List<T> list, uint version)
        {
            JArray ja = new JArray();

            foreach (T item in list)
            {
                dynamic x = item;
                ja.Add(x.ID);
            }

            return ja;
        }

        public static List<uint> JsonIdListToList(JArray ja)
        {
            List<uint> list = new List<uint>();

            foreach (uint id in ja)
            {
                list.Add(id);
            }

            return list;
        }

        public static List<T> JsonListToObjectList<T>(JArray ja, uint version) where T : new()
        {
            List<T> list = new List<T>();

            if (ja == null)
            {
                return list;
            }

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(jo, version);

                obj = d;

                list.Add(obj);
            }

            return list;
        }

        public static List<T> JsonListToObjectList<T>(CadObjectDB db, JArray ja, uint version) where T : new()
        {
            List<T> list = new List<T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(db, jo, version);

                list.Add(obj);
            }

            return list;
        }


        public static Dictionary<uint, T> JsonListToDictionary<T>(JArray ja, uint version) where T : new()
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(jo, version);

                dict.Add(d.ID, obj);
            }

            return dict;
        }

        public static Dictionary<uint, T> JsonListToDictionary<T>(CadObjectDB db, JArray ja, uint version) where T : new()
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            foreach (JObject jo in ja)
            {
                T obj = new T();
                dynamic d = obj;

                d.FromJson(db, jo, version);

                dict.Add(d.ID, obj);
            }

            return dict;
        }
    }
}
