using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DamienG.Security.Cryptography;

namespace HybreDb.Storage {
    public class Storage {
        public static readonly Crc32 Crc = new Crc32();

        public string Filename { get; private set; }


        public Storage(string fname) {
            Filename = fname;
        }

        protected byte[] GetBytes(ISerializable o) {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, o);
                
                return ms.ToArray();
            }
        }

        public void Write(ISerializable o) {
            var d = GetBytes(o);
            var crc = Crc.ComputeHash(d);
        }

    }
}
