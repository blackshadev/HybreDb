using HybreDb;
using HybreDb.Relational;
using HybreDb.Tables;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDbTest {
    class UndirectedRelationTest {
        private readonly Database Db = new Database("RelationTest", true);

        [TestCase]
        public void Basic() {
            int c;
            var t = DummyData.TestTable(Db, "Test");
            DummyData.TestRows(t);

            var attrs = new[] {
                    new DataColumn("From", DataTypes.Types.Text)
                };

            Db.NewRelation("Knows", "Test", "Test", attrs, RelationType.UndirectedRelation);

            t.Write();

            for (var i = 0; i < DummyData.UniqueRelationMarker; i++) {
                var fromTo = DummyData.Relations[i].Item1;
                t.Relations["Knows"].Add(fromTo.Item1, fromTo.Item2, DummyData.Relations[i].Item2);
            }

            c = t.Relations["Knows"].Count();
            Assert.IsTrue(c == DummyData.UniqueRelationMarker, "Invalid relation number");

            t.Commit();

            c = t.Relations["Knows"].Count();
            Assert.IsTrue(c == DummyData.UniqueRelationMarker, "Invalid relation number");


            var relDef = DummyData.Relations[DummyData.UniqueRelationMarker];
            var dr_1 = t.Relations["Knows"].Get(2, 0);
            var dr_2 = t.Relations["Knows"].Get(0, 2);
            Assert.AreSame(dr_1, dr_2, "Invalid undirected relation matched");

        }

        [TestCase]
        public void JsonRelation() {
            var createRelation = "{ \"method\": \"CreateRelation\" }";
        }
    }
}
