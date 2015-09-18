using System;
using System.Net.Sockets;
using System.Text;
using HybreDb.Communication;
using NUnit.Framework;

namespace HybreDbTest {
    public class SocketServerTest {
        [TestCase] 
        public void Small() {
            var s = new ThreadedSocketServer();
            s.OnDataReceived += (o, e) => Console.WriteLine(e.Message);
            s.Start();

            Send(4242,
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras varius auctor erat id suscipit. Nulla eget imperdiet lectus, vitae consequat mi. Proin sollicitudin condimentum nunc, ac imperdiet dolor sodales eu. Nunc non dui quis arcu tincidunt imperdiet non sed risus. Donec rutrum scelerisque tellus eget vestibulum. Praesent lacus risus, tristique et vulputate sit amet, dignissim id dui. Integer at venenatis odio, sed euismod ipsum. Nunc suscipit tempor diam, eu porttitor nunc vehicula at. Donec mauris velit, venenatis ut ligula eu, commodo eleifend odio. Mauris molestie vitae lacus a consectetur. Nullam interdum aliquam mi et placerat. Nullam et augue nec nisi laoreet bibendum sed at ante. Pellentesque semper nec leo varius porta. Nullam fringilla rhoncus orci congue tristique. Donec at arcu dui. Maecenas non mi ultrices, vestibulum diam at, vulputate augue");
            Send(4242,
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras varius auctor erat id suscipit. Nulla eget imperdiet lectus, vitae consequat mi. Proin sollicitudin condimentum nunc, ac imperdiet dolor sodales eu. Nunc non dui quis arcu tincidunt imperdiet non sed risus. Donec rutrum scelerisque tellus eget vestibulum. Praesent lacus risus, tristique et vulputate sit amet, dignissim id dui. Integer at venenatis odio, sed euismod ipsum. Nunc suscipit tempor diam, eu porttitor nunc vehicula at. Donec mauris velit, venenatis ut ligula eu, commodo eleifend odio. Mauris molestie vitae lacus a consectetur. Nullam interdum aliquam mi et placerat. Nullam et augue nec nisi laoreet bibendum sed at ante. Pellentesque semper nec leo varius porta. Nullam fringilla rhoncus orci congue tristique. Donec at arcu dui. Maecenas non mi ultrices, vestibulum diam at, vulputate augue");

            s.Stop();
        }

        protected static void Send(int port, string message) {
            var c = new TcpClient("127.0.0.1", port);
            byte[] data = Encoding.Unicode.GetBytes(message);

            NetworkStream strm = c.GetStream();

            strm.Write(BitConverter.GetBytes(data.Length), 0, 4);
            strm.Write(data, 0, data.Length);
            strm.Close();
        }
    }
}