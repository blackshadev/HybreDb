using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using HybreDb;
using HybreDb.Relational;
using HybreDb.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class Relations {
        Database Db = new Database("RelationTest", true);

        [TestMethod]
        public void Basic() {
            Table t;
            if (!Db.Any()) {
                t = DummyData.TestTable(Db, "Test");
                DummyData.TestRows(t);

                var attrs = new[] {
                    new DataColumn("From", DataTypes.Types.Text), 
                };

                Db.NewRelation("Knows", "Test", "Test", attrs);

                t.Write();

                DummyData.TestRelations(t);

                t.Commit();
            }

            t = Db["Test"];
            foreach (var rel in t.Relations["Knows"].Data) {
                var str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3, rel.Item2["Name"]);
                Console.WriteLine(str);
            }

            t = Db.Reopen(t);
            
            Console.WriteLine("\nAfter reopen");
            foreach (var rel in t.Relations["Knows"].Data) {
                var str = String.Format("{0} -[{1}:{2}]-> {3}", rel.Item1["Name"], "Knows", rel.Item3, rel.Item2["Name"]);
                Console.WriteLine(str);
            }


        }
    }
}
