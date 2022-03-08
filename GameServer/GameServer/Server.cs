using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class Server
    {
        public static int _MaxPlayer { get; private set; }
        public static int _Port { get; private set; }

        public static TcpListener tcpListener;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static void Start(int _maxPlayer , int _port)
        {
            _MaxPlayer = _maxPlayer;
            _Port = _port;

            Console.WriteLine("Server Starting....");

            InitializeClient();

            tcpListener = new TcpListener(IPAddress.Any, _Port);
            tcpListener.Start();

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

            Console.WriteLine($"Server Started {_Port}");
            
        }

        private static void TcpConnectCallback(IAsyncResult result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            Console.WriteLine($"Incoming Connected From {_client.Client.RemoteEndPoint}");

            //TODO - Client içerisindeki TCP class içindeki socket'in boş olup olmadığını kontrol edip 
            //Kullanıcı bağlantısı gerçekleştir.

            for (int i = 0; i < _MaxPlayer; i++)
            {
                if (clients[i].tcp.sockets == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"Failed to connect : {_client.Client.RemoteEndPoint}");
        }

        private static void InitializeClient()
        {
            for (int i = 0; i < _MaxPlayer; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived,ServerHandler.WelcomeReceived }
            };


            Console.WriteLine("Initialize Packets");
        }

    }
}
