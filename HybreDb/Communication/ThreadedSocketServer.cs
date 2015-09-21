using System;
using System.Threading;

namespace HybreDb.Communication {
    public class ThreadedSocketServer : SocketServer {
        protected Thread Thread;

        public ThreadedSocketServer(int port = 4242) : base(port) {
            Thread = new Thread(base.Start);
        }

        public override void Start() {
            Thread.Start();
        }

        public override void Stop() {
            base.Stop();
            Thread.Join();
        }
    }
}