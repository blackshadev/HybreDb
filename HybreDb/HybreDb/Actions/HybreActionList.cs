﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.Actions.Result;
using Newtonsoft.Json;

namespace HybreDb.Actions {
    public class HybreActionList : IHybreAction {
        [JsonProperty("table")]
        public string TableName;

        public HybreResult Execute(Database db) {
            return new HybreDataResult(db[TableName], db[TableName].Rows.Select(e => e.Value));
        }
    }
}
