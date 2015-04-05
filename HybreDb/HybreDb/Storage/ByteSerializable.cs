using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb.Storage {
    public interface ITreeSerializable {
        void Serialize(BinaryWriter wrtr);
        void Deserialize(BinaryReader rdr);

    }

    public delegate byte[] BytesConverter(Object o);
}
