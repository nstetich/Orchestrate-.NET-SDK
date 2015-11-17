using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orchestrate.Io.Extensibility;

namespace Orchestrate.Io.Utility
{
    class DefaultJsonSerializer : IJsonSerializer
    {
        public string SerializeObject(object item)
        {
            return JsonConvert.SerializeObject(item);
        }

        public T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
