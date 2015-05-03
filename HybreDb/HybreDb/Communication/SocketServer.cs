using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HybreDb.Communication {

    public class ClientState {
        public const int BufferSize = 1024;

        public SocketServer Server;

        /// <summary>
        /// Client socket
        /// </summary>
        public Socket Socket = null;

        public byte[] Buffer = new byte[BufferSize];

        public StringBuilder Data = new StringBuilder();

        public ClientState(SocketServer s, Socket cSocket) {
            Server = s;
            Socket = cSocket;

            Socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReadCallback, this);
        }
        
        public static void ReadCallback(IAsyncResult ar) {

            var state = (ClientState) ar.AsyncState;

            var bytesRead = state.Socket.EndReceive(ar);

            if (bytesRead > 0) {
                state.Data.Append(Encoding.Unicode.GetString(state.Buffer, 0, bytesRead));

                state.Socket.BeginReceive(state.Buffer, 0, BufferSize, 0, ReadCallback, state);
            } else
                state.Server.ClientDataReceived(state);
            

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

        public void Start() {
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
            OnDataReceived(s, new ClientDataReceivedEvent { State = s, Message = s.Data.ToString() });
        }

        public static void AcceptCallback(IAsyncResult ar) {
            var s = (SocketServer)ar.AsyncState;

            s.Accepted.Set();

            var listener = s.Server;
            var cSocket = listener.EndAccept(ar);

            var state = new ClientState(s, cSocket);
        }

        public void Stop() {
            IsRunning = false;
            Accepted.Set();
        }
    }
}
