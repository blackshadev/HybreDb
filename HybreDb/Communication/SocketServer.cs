using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HybreDb.Communication {

    /// <summary>
    /// Preserves the client's message and socket
    /// </summary>
    public class ClientState : IDisposable {
        public SocketServer Server;

        /// <summary>
        /// Client socket
        /// </summary>
        public Socket Socket = null;

        /// <summary>
        /// Data length of the current message
        /// </summary>
        public int DataLength;
        
        /// <summary>
        /// Offset in the buffer currently on
        /// </summary>
        protected int DataOffset;

        /// <summary>
        /// Raw data received from the user
        /// </summary>
        public byte[] Buffer;

        public ClientState(SocketServer s, Socket cSocket) {
            Server = s;
            Socket = cSocket;

            WaitForMessage();
        }

        /// <summary>
        /// Waits for a message by first receiving the length of the upcoming message
        /// </summary>
        public void WaitForMessage() {
            DataOffset = 0;
            Buffer = new byte[4];
            try {
                Socket.BeginReceive(Buffer, 0, 4, SocketFlags.None, ReadLengthCallback, null);
            } catch { Dispose(); }
        }

        /// <summary>
        /// Reads the length of the upcomming message
        /// </summary>
        public void ReadLengthCallback(IAsyncResult ar) {

            DataLength = BitConverter.ToInt32(Buffer, 0);
            Buffer = new byte[DataLength];

            try {
                Socket.BeginReceive(Buffer, DataOffset, DataLength, SocketFlags.None, ReadCallback, null);
            } catch { Dispose(); }
        }
        
        /// <summary>
        ///  Reads a part of the message and updates the data offset
        /// </summary>
        public void ReadCallback(IAsyncResult ar) {
            try {
                DataOffset += Socket.EndReceive(ar);

                if (DataOffset < DataLength)
                    Socket.BeginReceive(Buffer, DataOffset, DataLength - DataOffset, SocketFlags.None, ReadCallback,
                        null);
                else {
                    Server.ClientDataReceived(this);
                    WaitForMessage();
                }

            } catch { Dispose(); }
        }

        /// <summary>
        /// Sends a message to the client
        /// </summary>
        /// <param name="str">String data to send</param>
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

        public void Dispose() { Dispose(true); }
    }

    /// <summary>
    /// Event arguments used in the OnDataReceived event
    /// </summary>
    public class ClientDataReceivedEvent : EventArgs {
        public string Message;
        public ClientState State;
    }

    /// <summary>
    /// Custom socket server used to send and receive string data 
    /// </summary>
    public class SocketServer {
        protected Socket Server;
        protected int Port;
        protected bool IsRunning;
        protected ManualResetEvent Accepted;

        public delegate void ClientDataReceivedEventHandler(object s, ClientDataReceivedEvent e);
        
        /// <summary>
        /// Event called upon receiving a client message
        /// </summary>
        public event ClientDataReceivedEventHandler OnDataReceived;

        public SocketServer(int port = 4242) {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Port = port;

            Accepted = new ManualResetEvent(false);
        }

        /// <summary>
        /// Starts the socket server
        /// </summary>
        public virtual void Start() {
            Server.Bind(new IPEndPoint(IPAddress.Any, Port));
            Server.Listen(10);

            IsRunning = true;
            while (IsRunning) {
                Accepted.Reset();

                Server.BeginAccept(AcceptCallback, null);

                Accepted.WaitOne();
            }

        }

        public void ClientDataReceived(ClientState s) {
            OnDataReceived(s, new ClientDataReceivedEvent {
                State = s, 
                Message = Encoding.Unicode.GetString(s.Buffer)
            });
        }

        /// <summary>
        /// Callback used upon accepting a client connection
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallback(IAsyncResult ar) {
            Accepted.Set();

            var listener = Server;
            var cSocket = listener.EndAccept(ar);

            var c = new ClientState(this, cSocket);
        }

        /// <summary>
        /// Stops accepting messages and disposes the server socket
        /// </summary>
        public virtual void Stop() {
            IsRunning = false;
            Accepted.Set();
            Server.Dispose();
        }
    }
}
