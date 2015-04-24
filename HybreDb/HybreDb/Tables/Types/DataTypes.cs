﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybreDb.BPlusTree.DataTypes;
using HybreDb.Tables.Types;
using DateTime = HybreDb.Tables.Types.DateTime;

namespace HybreDb.Tables {


    /// <summary>
    /// Abstract class for useable datatypes within the tree.
    /// Every DataType can be used as key and as value.
    /// The extended classes of DataType, must have both the empty constructor as well as the constructor with an binary reader
    /// </summary>
    public abstract class DataType : IDataType {

        protected DataType() { }

        /// <summary>
        /// Constructor used to deserialize an object
        /// </summary>
        /// <param name="rdr"></param>
        protected DataType(BinaryReader rdr) { Deserialize(rdr); }

        public abstract void Serialize(BinaryWriter b);

        public abstract void Deserialize(BinaryReader b);

        public abstract int CompareTo(object obj);
    }


    /// <summary>
    /// DataTypes structure containing datatype information within the database
    /// </summary>
    public static class DataTypes {

        /// <summary>
        /// Enum of Types, uses the byte values to distinguish between data on serialisation 
        /// </summary>
        public enum Types {
            Number = 0,
            Text = 1,
            DateTime = 2
        };

        /// <summary>
        /// Create the object implementation corresponding to the DataTypes
        /// </summary>
        public static IDataType CreateType(this Types t) {
            switch (t) {
                case Types.Number: return new Number();
                case Types.Text: return new Text();
                case Types.DateTime: return new DateTime();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create the object implementation corresponding to the DataTypes, with a given BinaryReader holding the actual data of that type
        /// </summary>
        public static IDataType CreateType(this Types t, BinaryReader rdr) {
            switch (t) {
                case Types.Number: return new Number(rdr);
                case Types.Text: return new Text(rdr);
                case Types.DateTime: return new DateTime(rdr);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create the object implementation corresponding to the DataTypes, with a given object as value
        /// </summary>
        public static IDataType CreateType(this Types t, object d) {
            switch (t) {
                case Types.Number: return new Number((int)d);
                case Types.Text: return new Text((string)d);
                case Types.DateTime: return new DateTime((System.DateTime)d);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gives the System.Type of the DataTypes
        /// </summary>
        public static Type GetSystemType(this Types t) {
            switch (t) {
                case Types.Number: return typeof(Number);
                case Types.Text  : return typeof(Text);
                case Types.DateTime: return typeof(DateTime);
                default: return null;
            }
        }
    }
}
