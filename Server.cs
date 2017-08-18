using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    class Program
    {
        static IPAddress ip = GetIP();
        static int port = 8820;
        static List<TcpClient> clients = new List<TcpClient>();
        static Queue<string> messages_to_send = new Queue<string>();

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(ip, port);  //IPAddress.Any
            server.Start();
            
            Thread send = new Thread(SendMessages);
            send.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client);

                Thread handleClient = new Thread(() => ClientHandler(client));
                handleClient.Start();
            }
        }

        public static void SendMessages()
        {
            while (true)
            {
                if (messages_to_send.Count > 0)
                {
                    string message = messages_to_send.Dequeue();
                    try
                    {
                        lock (clients)
                        {
                            foreach (TcpClient client in clients)
                            {
                                NetworkStream stream = client.GetStream();
                                StreamWriter sw = new StreamWriter(stream)
                                {
                                    AutoFlush = true
                                };
                                sw.WriteLine(message);
                            }
                        }
                    }
                    catch { break; }

                }
            }
        }
        
        public static void ClientHandler(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            StreamReader sr = new StreamReader(stream);

            StreamWriter sw = new StreamWriter(stream)
            {
                AutoFlush = true
            };
            string message = "Welcome! Enter your name: ";
            sw.WriteLine(message);

            string name = sr.ReadLine();

            message = " has connected!";
            Console.WriteLine(name + message);
            

            while (message != "quit")
            {
                messages_to_send.Enqueue(name + " >> " + message);
                while (!stream.DataAvailable) { }
                message = sr.ReadLine();
            }
            clients.Remove(client);

            string disconnect_msg = name + " has disconnected!";
            messages_to_send.Enqueue(disconnect_msg);
            Console.WriteLine(disconnect_msg);

            stream.Close();
            client.Close();
        }

        public static IPAddress GetIP()
        {
            string host = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            List<IPAddress> adress_lst = hostEntry.AddressList.ToList();
            List<IPAddress> addresses = adress_lst.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();
            return addresses.FirstOrDefault();
        }
    }
}