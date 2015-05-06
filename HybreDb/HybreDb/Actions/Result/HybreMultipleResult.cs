using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    [JsonConverter(typeof(HybreMultiResultSerializer))]
    public class HybreMultipleResult : HybreResult {
        
        public HybreResult[] Results;
    }

    public class HybreMultiResultSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var r = value as HybreMultipleResult;
            serializer.Serialize(writer, r.Results);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreMultipleResult).IsAssignableFrom(objectType);
        }
    }
}
