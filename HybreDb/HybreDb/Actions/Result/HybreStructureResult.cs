using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Tables;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    /// <summary>
    /// Represents the structure of a table
    /// </summary>
    
    [JsonConverter(typeof(HybreStructureResultSerializer))]
    public class HybreStructureResult : HybreResult {

        public Table Table;

    }

    public class HybreStructureResultSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

            var res = value as HybreStructureResult;

            writer.WriteStartObject();

            writer.WritePropertyName("elapsedTime");
            writer.WriteValue(res.ElapsedTime);

            writer.WritePropertyName("tableName");
            writer.WriteValue(res.Table.Name);

            writer.WritePropertyName("columns");
            HybreResult.SerializeColumns(writer, res.Table.Columns.Where(e => !e.Hidden));

            writer.WritePropertyName("relations");
            HybreResult.SerializeRelations(writer, res.Table.Relations);

            writer.WriteEndObject();

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreStructureResult).IsAssignableFrom(objectType);
        }
    }

}
