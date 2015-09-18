using System;
using System.IO;

namespace HybreDb.Storage {
    public interface IByteSerializable {
        void Serialize(BinaryWriter wrtr);
        void Deserialize(BinaryReader rdr);
    }

    public delegate byte[] BytesConverter(Object o);
}