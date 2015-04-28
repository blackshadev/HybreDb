using System;
using HybreDb;
using HybreDb.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HybreDbTest {

    [TestClass]
    public class JsonTest {
        private Database db;
        public JsonTest() {
            db = new Database("JsonTest", true);
            var t = DummyData.TestTable(db, "People");
            DummyData.TestRows(t);
        }

        [TestMethod]
        public void Creation() {
            var strJson = "{ " +
                              "\"method\": \"get\", " +
                              "\"params\": {" +
                                   "\"table\": \"People\"," +
                                   "\"condition\": [" +
                                      "{" +
                                        "\"type\": \"and\"," +
                                        "\"field\": \"Name\"," +
                                        "\"value\": \"Tessa\"" +
                                      "}" +
                                  "]" +
                                "}" +
                          "}";
            
            var act = HybreAction.Parse(strJson);
            var res = act.Execute(db);
            var str = JsonConvert.SerializeObject(res);
        }
    }
}
