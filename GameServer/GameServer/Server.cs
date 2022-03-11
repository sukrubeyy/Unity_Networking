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

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

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
            udpListener = new UdpClient(_port);
            udpListener.BeginReceive(UDPReceiveCallback, null);
            Console.WriteLine($"Server Started {_Port}");
            
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);
                if (_data.Length < 4)
                {
                    return;
                }

                using(Packet _packet = new Packet(_data))
                {
                    int clientID = _packet.ReadInt();

                    //Bunu neden yaptığımızı anlamadım ARAŞTIR

                    if (clientID == 0)
                    {
                        return;
                    }

                    if (clients[clientID].udp.endPoint==null)
                    {
                        clients[clientID].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[clientID].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[clientID].udp.HandleData(_packet);
                    }
                }
            }

            catch (Exception _ex)
            {
                Console.WriteLine($"Error Receiving UDP DATA : {_ex.Message}");
            }
        }

        public static void SendUDPData(IPEndPoint _endPoint , Packet _packet)
        {
            try
            {
                if (_endPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _endPoint, null, null);
                }
            }
            catch(Exception _ex)
            {
                Console.WriteLine($"Error Sending data to {_endPoint} via UDP {_ex.Message}");
            }
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
                {(int)ClientPackets.welcomeReceived,ServerHandler.WelcomeReceived },
                {(int)ClientPackets.udpTestReceive,ServerHandler.UDPTestReceived }
            };


            Console.WriteLine("Initialize Packets");
        }

    }
}
