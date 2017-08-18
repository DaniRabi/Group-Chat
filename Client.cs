using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace SocketClient
{
    class Program
    {
        static int port = 8820;
        static string host = Dns.GetHostName();

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient(host, port);

            Thread receive = new Thread(() => ReceiveMessages(client));
            receive.Start();

            SendMessages(client);

            //Console.ReadLine();
        }

        public static void SendMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamWriter sw = new StreamWriter(stream)
            {
                AutoFlush = true
            };

            string message = "";
            while (message != "quit")
            {
                try
                {
                    message = Console.ReadLine();
                    sw.WriteLine(message);
                }
                catch
                {
                    break;
                }
            }
            Console.WriteLine("You have disconnected!");
            stream.Close();
            client.Close();
        }

        public static void ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader sr = new StreamReader(stream);

            string message = "";
            while (true)
            {
                try
                {
                    while (!stream.DataAvailable)
                    {
                    }
                    message = sr.ReadLine();
                    Console.WriteLine(message);
                }
                catch { break; }
            }
        }


    }
}
