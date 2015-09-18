using System;
using System.Collections.Generic;
using HybreDb;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables;
using HybreDb.Tables.Types;
using DateTime = System.DateTime;

namespace HybreDbTest {
    public static class DummyData {
        public const int Seed = 42;
        public const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";

        public static Table TestTable(Database db, string n) {
            var cols = new[] {
                new DataColumn("Name", DataTypes.Types.Text, true),
                new DataColumn("Age", DataTypes.Types.Number, true),
                new DataColumn("Unindexed_Age", DataTypes.Types.Number),
                new DataColumn("Inserted", DataTypes.Types.DateTime)
            };
            return db.NewTable(n, cols);
        }

        public static void TestRows(Table tab) {
            tab.Insert(new IDataType[] {
                new Text("Vincent"),
                new Number(22),
                new Number(22),
                new HybreDb.Tables.Types.DateTime(DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Wouter"),
                new Number(22),
                new Number(22),
                new HybreDb.Tables.Types.DateTime(DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Tessa"),
                new Number(20),
                new Number(20),
                new HybreDb.Tables.Types.DateTime(DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Tessa"),
                new Number(26),
                new Number(26),
                new HybreDb.Tables.Types.DateTime(DateTime.Now)
            });
            tab.Insert(new IDataType[] {
                new Text("Stefan"),
                new Number(21),
                new Number(21),
                new HybreDb.Tables.Types.DateTime(DateTime.Now)
            });
        }

        public static void TestRelations(Table tab) {
            tab.Relations["Knows"].Add(0, 1, new IDataType[] {new Text("UvA")});
            tab.Relations["Knows"].Add(0, 2, new IDataType[] {new Text("Baken")});
            tab.Relations["Knows"].Add(0, 3, new IDataType[] {new Text("Zus")});
            tab.Relations["Knows"].Add(0, 4, new IDataType[] {new Text("UvA")});

            tab.Relations["Knows"].Add(2, 0, new IDataType[] {new Text("Baken")});
            tab.Relations["Knows"].Add(1, 2, new IDataType[] {new Text("UvA")});
            tab.Relations["Knows"].Add(2, 1, new IDataType[] {new Text("UvA")});
            tab.Relations["Knows"].Add(2, 0, new IDataType[] {new Text("UvA")});
            tab.Relations["Knows"].Add(4, 0, new IDataType[] {new Text("UvA")});
        }

        public static IDataType[][] RandomDataset(int N) {
            var rnd = new Random(Seed);
            var o = new IDataType[N][];


            for (int i = 0; i < N; i++) {
                var a = new Number(rnd.Next(0, 100));
                o[i] = new IDataType[] {
                    new Text(RandomString(rnd.Next(0, 8), rnd)),
                    a,
                    a,
                    new HybreDb.Tables.Types.DateTime(DateTime.Now)
                };
            }

            return o;
        }

        public static Tuple<NumberPair, IDataType[]>[] RandomRelations(int dat_len, int N) {
            var rnd = new Random(Seed);

            var rels = new Tuple<NumberPair, IDataType[]>[N];
            var hs = new HashSet<NumberPair>();
            int iX = 0;

            NumberPair num;
            IDataType[] dat;
            while (iX < N) {
                num = new NumberPair(rnd.Next(0, dat_len), rnd.Next(0, dat_len));

                if (hs.Contains(num)) continue;

                dat = new IDataType[] {
                    new Text(RandomString(5, rnd))
                };

                hs.Add(num);
                rels[iX++] = new Tuple<NumberPair, IDataType[]>(num, dat);
            }

            return rels;
        }


        public static string RandomString(int n, Random r) {
            var c = new char[n];

            for (int i = 0; i < n; i++)
                c[i] = Chars[r.Next(0, Chars.Length)];

            return new string(c);
        }
    }
}