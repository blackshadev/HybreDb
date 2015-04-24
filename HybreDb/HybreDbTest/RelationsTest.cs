using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HybreDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class Relations {
        Database Db = new Database("RelationTest", true);

        [TestMethod]
        public void Basic() {

            var t = DummyData.TestTable(Db, "Test");
            DummyData.TestRows(t);

            Db.AddRelation("Knows", "Test", "Test");

            t.Write();

            DummyData.TestRelations(t);

            t.Commit();

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
