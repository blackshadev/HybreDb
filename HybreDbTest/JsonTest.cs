using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using HybreDb;
using HybreDb.Actions;
using HybreDb.Tables;
using Newtonsoft.Json;
using NUnit.Framework;

namespace HybreDbTest {
    public class JsonTest {
        protected const string strJson1 = "{ " +
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

        protected const string strJson2 = "{ " +
                                          "\"method\": \"get\", " +
                                          "\"params\": {" +
                                          "\"table\": \"People\"," +
                                          "\"key\": 0" +
                                          "}" +
                                          "}";

        protected const string strJson3 = "{ " +
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

        protected const string strJson4 = "{ " +
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

        protected const string strJson5 = "{ " +
                                          "\"method\": \"delete\", " +
                                          "\"params\": {" +
                                          "\"table\": \"People\"," +
                                          "\"key\": 0" +
                                          "}" +
                                          "}";

        protected const string strJson6 = "{" +
                                          "\"method\": \"list\"," +
                                          "\"params\": {" +
                                          "\"table\": \"People\"" +
                                          "}" +
                                          "}";

        protected const string strJson7 = "{" +
                                          "\"method\": \"listRelation\"," +
                                          "\"params\": {" +
                                          "\"table\": \"People\"," +
                                          "\"relation\": \"Knows\"" +
                                          "}" +
                                          "}";

        protected const string strJson8 = "{" +
                                          "\"method\": \"createTable\"," +
                                          "\"params\": {" +
                                          "\"table\": \"People2\"," +
                                          "\"columns\": [" +
                                          "{ \"name\": \"Name\", \"dataType\": \"Text\", \"hasIndex\": true }," +
                                          "{ \"name\": \"Age\", \"dataType\": \"Number\", \"hasIndex\": true }," +
                                          "]," +
                                          "rows: []" +
                                          "}" +
                                          "}";

        protected const string strJson9 = "{" +
                                          "\"method\": \"listTables\"," +
                                          "\"params\": {}" +
                                          "}";

        private readonly Database db;


        public JsonTest() {
            db = new Database("JsonTest", true);
            TestData(db);
        }

        private static void TestData(Database db) {
            Table t = DummyData.TestTable(db, "People");
            db.NewRelation("Knows", "People", "People", new[] {
                new DataColumn("From", DataTypes.Types.Text)
            });
            db.Write();

            DummyData.TestRows(t);
            DummyData.TestRelations(t);
            t.Commit();
        }

        [TestCase]
        public void Creation() {
            Time("Initial Parse", () => HybreAction.Parse(strJson4));

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

        [TestCase]
        public void SocketServer() {
            var hybre = new HybreDb.HybreDb("SocketServerTest", 4242, true);
            TestData(hybre.Database);
            hybre.Start();

            string resp = SendAndReceive(strJson7);
            Console.WriteLine(resp);
        }

        private static string SendAndReceive(string str) {
            var c = new TcpClient("127.0.0.1", 4242);
            byte[] data = Encoding.Unicode.GetBytes(str);
            int read;
            NetworkStream strm = c.GetStream();

            strm.Write(BitConverter.GetBytes(data.Length), 0, 4);
            strm.Write(data, 0, data.Length);
            strm.Flush();

            int l;
            data = new byte[4];
            read = 0;
            while ((read += strm.Read(data, read, 4)) < 4) {
            }
            l = BitConverter.ToInt32(data, 0);

            data = new byte[l];
            read = 0;
            while ((read += strm.Read(data, read, l)) < l) {
            }

            strm.Close();

            return Encoding.Unicode.GetString(data);
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