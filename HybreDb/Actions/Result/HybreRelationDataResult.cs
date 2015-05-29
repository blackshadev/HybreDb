using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Relational;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {

    /// <summary>
    /// Wraps the dataset of a singlular relation between tables
    /// </summary>
    [JsonConverter(typeof(HybreRelationDataSerialiser))]
    public class HybreRelationDataResult : HybreResult {

        public Relation Relation;

        /// <summary>
        /// Data rows of the relation
        /// </summary>
        public IEnumerable<DataRow> Data;


    }

    /// <summary>
    /// Serialises the Relational dataset by adding the relation as property and keeping track of the rows which are needed from the tables.
    /// After that add the source and destination dataset on the found keys.
    /// </summary>
    public class HybreRelationDataSerialiser : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var rel = value as HybreRelationDataResult;
            var srcNums = new Numbers();
            var dstNums = new Numbers();
            HybreDataResult res;
            
            writer.WriteStartObject();

            writer.WritePropertyName("relation");
            res = new HybreDataResult(rel.Relation.Table, rel.Data.Select(e => {
                srcNums.Add((Number)e[1]);
                dstNums.Add((Number)e[2]);

                return e;
            }));
            serializer.Serialize(writer, res);

            writer.WritePropertyName("sourceTable");
            serializer.Serialize(writer, (new HybreDataResult(rel.Relation.Source, rel.Relation.Source.GetData(srcNums))));

            writer.WritePropertyName("destinationTable");
            serializer.Serialize(writer, new HybreDataResult(rel.Relation.Destination, rel.Relation.Destination.GetData(dstNums)));


            writer.WriteEndObject();
        }

        /// <summary>
        /// Results are not ment to be interpret from json
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return typeof (HybreRelationDataResult).IsAssignableFrom(objectType);
        }
    }

}
