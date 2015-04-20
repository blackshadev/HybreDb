using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;

namespace HybreDb.Tables {

    /// <summary>
    /// DataType structure containing datatype information within the database
    /// </summary>
    public static class DataType {

        /// <summary>
        /// Enum of Types, uses the byte values to distinguish between data on serialisation 
        /// </summary>
        public enum Types {
            Number = 0,
            Text = 1,
            DateTime = 2
        };

        /// <summary>
        /// Create the object implementation corresponding to the DataType
        /// </summary>
        public static IDataType CreateType(this Types t) {
            switch (t) {
                case Types.Number: return new Number();
                case Types.Text: return new Text();
                case Types.DateTime: return new HybreDb.BPlusTree.DataTypes.DateTime();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create the object implementation corresponding to the DataType, with a given BinaryReader holding the actual data of that type
        /// </summary>
        public static IDataType CreateType(this Types t, BinaryReader rdr) {
            switch (t) {
                case Types.Number: return new Number(rdr);
                case Types.Text: return new Text(rdr);
                case Types.DateTime: return new BPlusTree.DataTypes.DateTime(rdr);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create the object implementation corresponding to the DataType, with a given object as value
        /// </summary>
        public static IDataType CreateType(this Types t, object d) {
            switch (t) {
                case Types.Number: return new Number((int)d);
                case Types.Text: return new Text((string)d);
                case Types.DateTime: return new BPlusTree.DataTypes.DateTime((System.DateTime)d);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gives the System.Type of the DataType
        /// </summary>
        public static Type GetSystemType(this Types t) {
            switch (t) {
                case Types.Number: return typeof(Number);
                case Types.Text  : return typeof(Text);
                case Types.DateTime: return typeof(BPlusTree.DataTypes.DateTime);
                default: return null;
            }
        }
    }
}
