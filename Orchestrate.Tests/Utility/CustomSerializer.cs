using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Orchestrate.Io.Extensibility;
using Orchestrate.Io.Utility;

namespace Orchestrate.Tests.Utility
{
    public class CustomSerializer : IJsonSerializer
    {
        private JsonSerializer jsonSerializer;

        public CustomSerializer()
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new StringEnumConverter());
            jsonSerializer = JsonSerializer.Create(jsonSettings);
        }

        public static IJsonSerializer Create()
        {
            return new CustomSerializer();
        }

        public string SerializeObject(object item)
        {
            using (var stringWriter = new StringWriter())
            {
                jsonSerializer.Serialize(stringWriter, item);
                return stringWriter.ToString();
            }
        }

        public T DeserializeObject<T>(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var jsonTextReader = new JsonTextReader(stringReader))
            {
                return jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
