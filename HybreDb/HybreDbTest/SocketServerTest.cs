using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using HybreDb.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HybreDbTest {
    [TestClass]
    public class SocketServerTest {
        
        [TestMethod]
        public void Small() {
            var s = new SocketServer();
            s.OnDataReceived += (o, e) => Console.WriteLine(e.Message);
            var t = new Thread(s.Start);
            t.Start();

            Send(4242, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras varius auctor erat id suscipit. Nulla eget imperdiet lectus, vitae consequat mi. Proin sollicitudin condimentum nunc, ac imperdiet dolor sodales eu. Nunc non dui quis arcu tincidunt imperdiet non sed risus. Donec rutrum scelerisque tellus eget vestibulum. Praesent lacus risus, tristique et vulputate sit amet, dignissim id dui. Integer at venenatis odio, sed euismod ipsum. Nunc suscipit tempor diam, eu porttitor nunc vehicula at. Donec mauris velit, venenatis ut ligula eu, commodo eleifend odio. Mauris molestie vitae lacus a consectetur. Nullam interdum aliquam mi et placerat. Nullam et augue nec nisi laoreet bibendum sed at ante. Pellentesque semper nec leo varius porta. Nullam fringilla rhoncus orci congue tristique. Donec at arcu dui. Maecenas non mi ultrices, vestibulum diam at, vulputate augue");

            s.Stop();
            t.Join();
        }

        protected static void Send(int port, string message) {
            var c = new TcpClient("127.0.0.1", port);
            var data = System.Text.Encoding.Unicode.GetBytes(message);

            var strm = c.GetStream();
            strm.Write(data, 0, data.Length);
            strm.Close();
        }

    }
}
