using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HybreDb.Communication {

    public class ClientState : IDisposable {
        public SocketServer Server;

        /// <summary>
        /// Client socket
        /// </summary>
        public Socket Socket = null;

        public int DataLength;
        protected int DataOffset;

        public byte[] Buffer;

        public ClientState(SocketServer s, Socket cSocket) {
            Server = s;
            Socket = cSocket;

            WaitForMessage();
        }

        public void WaitForMessage() {
            DataOffset = 0;
            Buffer = new byte[4];
            try {
                Socket.BeginReceive(Buffer, 0, 4, SocketFlags.None, ReadLengthCallback, null);
            } catch { Dispose(); }
        }

        public void ReadLengthCallback(IAsyncResult ar) {

            DataLength = BitConverter.ToInt32(Buffer, 0);
            Buffer = new byte[DataLength];

            try {
                Socket.BeginReceive(Buffer, DataOffset, DataLength, SocketFlags.None, ReadCallback, null);
            } catch { Dispose(); }
        }
        
        public void ReadCallback(IAsyncResult ar) {

            DataOffset += Socket.EndReceive(ar);

            if (DataOffset < DataLength)
                Socket.BeginReceive(Buffer, DataOffset, DataLength - DataOffset, SocketFlags.None, ReadCallback, null);
            else {
                Server.ClientDataReceived(this);
                WaitForMessage();
            }

        }

        public void Send(string str) {
            var data = Encoding.Unicode.GetBytes(str);

            try {
                Socket.Send(BitConverter.GetBytes(data.Length), 0);
                Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, null);
            }
            catch { Dispose(); }
        }

        public void SendCallback(IAsyncResult ar) {
            Socket.EndSend(ar);
        }

        protected void Dispose(bool all) {
            Socket.Close();
            Socket.Dispose();
            Buffer = null;
            DataLength = 0;
            DataOffset = 0;
        }

        public void Dispose() {
            Dispose(true);
        }
    }

    public class ClientDataReceivedEvent : EventArgs {
        public string Message;
        public ClientState State;
    }

    public class SocketServer {
        protected Socket Server;
        protected int Port;
        protected bool IsRunning;
        protected ManualResetEvent Accepted;

        public delegate void ClientDataReceivedEventHandler(object s, ClientDataReceivedEvent e);
        public event ClientDataReceivedEventHandler OnDataReceived;

        public SocketServer(int port = 4242) {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Port = port;

            Accepted = new ManualResetEvent(false);
        }

        public virtual void Start() {
            Server.Bind(new IPEndPoint(IPAddress.Any, Port));
            Server.Listen(10);

            IsRunning = true;
            while (IsRunning) {
                Accepted.Reset();

                Server.BeginAccept(AcceptCallback, this);

                Accepted.WaitOne();
            }

        }

        public void ClientDataReceived(ClientState s) {
            OnDataReceived(s, new ClientDataReceivedEvent {
                State = s, 
                Message = Encoding.Unicode.GetString(s.Buffer)
            });
        }

        public static void AcceptCallback(IAsyncResult ar) {
            var s = (SocketServer)ar.AsyncState;

            s.Accepted.Set();

            var listener = s.Server;
            var cSocket = listener.EndAccept(ar);

            var state = new ClientState(s, cSocket);
        }

        public virtual void Stop() {
            IsRunning = false;
            Accepted.Set();
        }
    }
}
