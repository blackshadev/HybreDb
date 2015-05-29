﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HybreDb.Actions.Result {
    /// <summary>
    /// A simple wrapper which only holds the affected rows
    /// </summary>
    public class HybreUpdateResult : HybreResult {
        [JsonProperty("affected")]
        public int Affected;

    }
}
