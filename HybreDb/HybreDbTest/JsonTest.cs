using System;
using System.Diagnostics;
using HybreDb;
using HybreDb.Actions;
using HybreDb.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HybreDbTest {

    [TestClass]
    public class JsonTest {
        private Database db;
        public JsonTest() {
            db = new Database("JsonTest", true);
            var t = DummyData.TestTable(db, "People");
            db.NewRelation("Knows", "People", "People", new[] {
                new DataColumn("From", DataTypes.Types.Text) 
            });
            db.Write();

            DummyData.TestRows(t);
            DummyData.TestRelations(t);
            t.Commit();
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
                                   "\"key\": 0" +
                                "}" +
                          "}";
            var strJson3 = "{ " +
                              "\"method\": \"insert\", " +
                              "\"params\": {" +
                                   "\"table\": \"People\"," +
                                   "\"Data\": {" +
                                        "\"Name\": \"Tester\"," +
                                        "\"Age\": 22," +
                                        "\"Unindexed_Age\": 22," +
                                        "\"Inserted\": \"now\"" +
                                    "}" +
                                "}" +
                          "}";
            var strJson4 = "{ " +
                              "\"method\": \"update\", " +
                              "\"params\": {" +
                                   "\"table\": \"People\"," +
                                   "\"key\": 0," +
                                   "\"Data\": {" +
                                        "\"Name\": \"Vincent Hagen\"," +
                                        "\"Inserted\": \"now\"" +
                                    "}" +
                                "}" +
                          "}";

            var strJson5 = "{ " +
                            "\"method\": \"delete\", " +
                            "\"params\": {" +
                                "\"table\": \"People\"," +
                                "\"key\": 0" +
                            "}" +
                        "}";
            var strJson6 = "{" +
                           "\"method\": \"list\"," +
                           "\"params\": {" +
                                "\"table\": \"People\"" +
                           "}" +
                        "}";
            var strJson7 = "{" +
                           "\"method\": \"relationList\"," +
                           "\"params\": {" +
                                "\"table\": \"People\"," +
                                "\"relation\": \"Knows\"" +
                           "}" +
                        "}";




            Time("Initial Parse" , () => HybreAction.Parse(strJson4));

            IHybreAction act = null;
            string str = "";
            Time("Parsing", () => act = HybreAction.Parse(strJson7));
            object res = null;
            Time("Execution", () => res = HybreAction.Execute(db, act));
            
            //Time("\nParsing", () => act = HybreAction.Parse(strJson2));
            //Time("Execution", () => res = HybreAction.Execute(db, act));
            

            Time("\nSerialisation", () => str = JsonConvert.SerializeObject(res, Formatting.Indented));
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
