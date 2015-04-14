using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb.Tables {
    public static class DataType {
        public enum Types {
            Number = 0,
            Text = 1,
            DateTime = 2
        };

        public static IDataType CreateType(Types t) {
            switch (t) {
                case Types.Number: return new Number();
                case Types.Text: return new Text();
                case Types.DateTime: return new HybreDb.BPlusTree.DataTypes.DateTime();
                default:
                    return null;
            }
        }

        public static IDataType CreateType(Types t, BinaryReader rdr) {
            switch (t) {
                case Types.Number: return new Number(rdr);
                case Types.Text: return new Text(rdr);
                case Types.DateTime: return new BPlusTree.DataTypes.DateTime(rdr);
                default:
                    return null;
            }
        }

        public static IDataType CreateType(Types t, object d) {
            switch (t) {
                case Types.Number: return new Number((int)d);
                case Types.Text: return new Text((string)d);
                case Types.DateTime: return new BPlusTree.DataTypes.DateTime((System.DateTime)d);
                default:
                    return null;
            }
        }

        public static Type GetType(Types t) {
            switch (t) {
                case Types.Number: return typeof(Number);
                case Types.Text  : return typeof(Text);
                case Types.DateTime: return typeof(BPlusTree.DataTypes.DateTime);
                default: return null;
            }
        }
    }
}
