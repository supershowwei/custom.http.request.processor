using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CustomHttpRequestProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ipAddresses = Dns.GetHostAddresses(Dns.GetHostName());

            var localEndPoint = new IPEndPoint(ipAddresses[0], 8080);

            var tcpListener = new TcpListener(localEndPoint);

            tcpListener.Start();

            Console.WriteLine("Listening...");

            while (true)
            {
                try
                {
                    var tcpClient = tcpListener.AcceptTcpClient();

                    if (tcpClient.Connected)
                    {
                        var receiveMessage = ReceiveMessage(tcpClient);

                        Console.WriteLine(receiveMessage);

                        var sb = new StringBuilder();
                        sb.AppendLine(@"HTTP/1.1 200 OK");
                        sb.AppendLine(@"Content-Length: 66");
                        sb.AppendLine(string.Empty);
                        sb.AppendLine(
                            string.Format(
                                @"<html><body>Hello World! ({0:yyyy/MM/dd HH:mm:ss.fff})</body></html>",
                                DateTime.Now));

                        SendMessage(sb.ToString(), tcpClient);
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