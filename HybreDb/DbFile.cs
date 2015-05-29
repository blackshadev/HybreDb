using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybreDb {
    public static class DbFile {

        public static FileStream Open(string fname) {
            return new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

    }
}
