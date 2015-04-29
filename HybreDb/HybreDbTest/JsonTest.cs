using System;
using System.Diagnostics;
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
            var strJson1 = "{ " +
                              "\"method\": \"match\", " +
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
            var strJson2 = "{ " +
                              "\"method\": \"get\", " +
                              "\"params\": {" +
                                   "\"table\": \"People\"," +
                                   "\"key\": 1" +
                                "}" +
                          "}";
            var strJson3 = "{ " +
                              "\"method\": \"insert\", " +
                              "\"params\": {" +
                                   "\"table\": \"People\"," +
                                   "\"data\": {" +
                                        "\"Name\": \"Tester\"," +
                                        "\"Age\": 22," +
                                        "\"Unindexed_Age\": 22," +
                                        "\"Inserted\": \"now\"" +
                                    "}" +
                                "}" +
                          "}";


            
            var act = HybreAction.Parse(strJson1);
            object res = null;
            Time("execution" ,() => res = act.Execute(db) );
            var str = JsonConvert.SerializeObject(res, Formatting.Indented);
            Console.WriteLine(str);
        }

        private static void Time(string tag, Action act) {
            var sw = new Stopwatch();
            sw.Start();
            act();
            sw.Stop();
            Trace.WriteLine(tag + " took " + sw.ElapsedMilliseconds + "ms");
        }
    }
}
