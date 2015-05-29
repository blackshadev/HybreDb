using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    [JsonConverter(typeof(HybreErrorJsonSerialiser))]
    public class HybreError : HybreResult {

        public Exception Error;

        public HybreError(Exception e) {
            Error = e;
        }

    }

    public class HybreErrorJsonSerialiser : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            var err = value as HybreError;

            writer.WriteStartObject();

            writer.WritePropertyName("elapsedTime");
            writer.WriteValue(err.ElapsedTime);


            writer.WritePropertyName("error");
            
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(err.Error.GetType().Name);

            writer.WritePropertyName("message");
            writer.WriteValue(err.Error.Message);

            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreErrorJsonSerialiser).IsAssignableFrom(objectType);
        }
    }
}
