using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CustomHttpRequestProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostEntry.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            var localEndPoint = new IPEndPoint(ipAddress, 8080);

            var tcpListener = new TcpListener(localEndPoint);

            tcpListener.Start();

            Console.WriteLine("Listening...");

            TcpClient tcpClient = null;

            while (true)
            {
                try
                {
                    if (tcpClient != null)
                    {
                        tcpClient.Close();
                    }

                    tcpClient = tcpListener.AcceptTcpClient();

                    if (tcpClient.Connected)
                    {
                        var receiveMessage = ReceiveMessage(tcpClient);

                        Console.WriteLine(receiveMessage);

                        var body =
                            string.Format(
                                "<html><body><h1>Hello World! ({0:yyyy/MM/dd HH:mm:ss.fff})</h1></body></html>\r\n",
                                DateTime.Now);

                        var headers =
                            string.Format(
                                "HTTP/1.1 200 OK\r\nContent-type: text/html\r\nContent-Length: {0}\n\n",
                                Encoding.ASCII.GetByteCount(body));

                        SendMessage(headers + body, tcpClient);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetBaseException().Message);
                    Console.Read();
                }
            }
        }

        private static string ReceiveMessage(TcpClient tcpClient)
        {
            var receiveMsg = string.Empty;
            var receiveBytes = new byte[tcpClient.ReceiveBufferSize];

            var networkStream = tcpClient.GetStream();

            if (networkStream.CanRead)
            {
                while (networkStream.DataAvailable)
                {
                    var numberOfBytesRead = networkStream.Read(receiveBytes, 0, tcpClient.ReceiveBufferSize);
                    receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                }
            }

            return receiveMsg;
        }

        private static void SendMessage(string message, TcpClient tcpClient)
        {
            var networkStream = tcpClient.GetStream();

            if (networkStream.CanWrite)
            {
                var messageBytes = Encoding.Default.GetBytes(message);
                networkStream.Write(messageBytes, 0, messageBytes.Length);
            }
        }
    }
}