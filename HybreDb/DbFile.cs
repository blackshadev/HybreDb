using System.IO;

namespace HybreDb {
    public static class DbFile {
        public static FileStream Open(string fname) {
            return new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
    }
}