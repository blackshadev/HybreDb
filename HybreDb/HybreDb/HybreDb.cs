using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HybreDb.Actions;
using HybreDb.Communication;

namespace HybreDb {
    public class HybreDb {
        public Database Database;
        public Queue<IHybreAction> Queue;
        public SocketServer Server;
        public Thread ServerThread;

    }
}
