using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Quokka.Public.Tools
{
    public static class QuokkaJson
    {
        public static JsonSerializerSettings TypedSettings = new JsonSerializerSettings() {
            TypeNameHandling = TypeNameHandling.Objects
        };

        public static T Copy<T>(T value)
        {
            return (T)DeserializeObject(SerializeObject(value));
        }

        public static IEnumerable<T> CopyArray<T>(IEnumerable<T> value)
        {
            return (IEnumerable<T>)DeserializeArray<T>(SerializeArray(value));
        }

        public static string SerializeObject(object value)
        {
            var json = JsonConvert.SerializeObject(value, TypedSettings);

            return json;
        }

        public static object DeserializeObject(string value)
        {
            var result = JsonConvert.DeserializeObject(value, TypedSettings);

            if (result is JArray array)
            {
                return array.Select(s => DeserializeObject(s.ToString()));
            }

            return result;
        }

        public static string SerializeArray<T>(IEnumerable<T> value)
        {
            var json = SerializeObject(value);

            return json;
        }

        public static IEnumerable<T> DeserializeArray<T>(string jsonArray)
        {
            return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonArray, TypedSettings);
        }

        public static string ToEnvelope(object payload)
        {
            return SerializeArray(new[] { payload });
        }

        public static string ToEnvelope(IEnumerable<object> payload)
        {
            return SerializeArray(payload);
        }

        public static IEnumerable<object> FromEnvelope(string json)
        {
            return DeserializeArray<object>(json);
        }
    }
}
