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

        /// <summary>
        /// Maksimum player ve port ataması ayrıca tcp,udp protokollerini oluşturmaktadır.
        /// </summary>
        /// <param name="_maxPlayer">Server'daki maksimum oyuncu sayını belirler</param>
        /// <param name="_port">Server port adresini belirliyorz</param>
        public static void Start(int _maxPlayer , int _port)
        {
            _MaxPlayer = _maxPlayer;
            _Port = _port;

            Console.WriteLine("Server Starting....");


            InitializeClient();

            tcpListener = new TcpListener(IPAddress.Any, _Port);
            tcpListener.Start();
            
            //TCP protokolüne client'ların giriş isteği gönderebilmesini başlatıyoruz.
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            udpListener = new UdpClient(_port);

            //UDP protokolünü türettik ve paket almasını sağlıyoruz.
            udpListener.BeginReceive(UDPReceiveCallback, null);
            Console.WriteLine($"Server Started {_Port}");
            
        }

        /// <summary>
        /// Client tarafından gönderilen UDP paketlerini yakalar
        /// </summary>
        /// <param name="_result"></param>
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                //Paket alımını sonlandırır ve alınan paketleri byte[] türünde döndürür.
                //paketleri yakalayıp _data değişkenine atıyoruz
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                //Ardından UDP protokolündeki paket yakalamayı tekrar aktif hale getiriyoruz
                udpListener.BeginReceive(UDPReceiveCallback, null);

                //Veri boyutu 4'ten ufaksa işlemi bittir
                if (_data.Length < 4)
                {
                    return;
                }

                using(Packet _packet = new Packet(_data))
                {
                    int clientID = _packet.ReadInt();
                    //Sunucu id'si 0'a eşit olacak bu yüzden eğer herhangi bir client id 0 olursa server çöker.
                    if (clientID == 0)
                    {
                        return;
                    }
             //Eğer gönderilen pakette client'ın id'si yer almıyorsa udp protokolünü client class'ı içerisinde tekrar bağla ve işlemi kır
                    if (clients[clientID].udp.endPoint==null)
                    {
                        clients[clientID].udp.Connect(_clientEndPoint);
                        return;
                    }

                    //Client'ın udp protokolünde bulunan ID ile Port üzerinde gelen paket id'si eşitse
                    if (clients[clientID].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        //Paketi Yakala
                        clients[clientID].udp.HandleData(_packet);
                    }
                }
            }

            catch (Exception _ex)
            {
                Console.WriteLine($"Error Receiving UDP DATA : {_ex.Message}");
            }
        }

        /// <summary>
        /// Client'a Udp Paketi Gönderir
        /// </summary>
        /// <param name="_endPoint">Client ID</param>
        /// <param name="_packet">PAKET</param>
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

        /// <summary>
        /// TCP Protokolüne Herhangi bir client bağlanmaya çalıştığında çalışacak method.
        /// </summary>
        /// <param name="result"></param>
        private static void TcpConnectCallback(IAsyncResult result)
        {
            //TCP protokolüne gelen isteği kabul eder ve geriye TcpClient türünde değer döndürür.
            TcpClient _client = tcpListener.EndAcceptTcpClient(result);

            //Protokol'e gelen isteklerin devam etmesini sağlıyoruz.
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
            Console.WriteLine($"Incoming Connected From {_client.Client.RemoteEndPoint}");

            //Başlangıçta oluştuduğumuz Dictionary<int,Client>'in tüm değerlerini dönüyoruz. Client class'ı içinde bulunan
            //Tcp class'ı içindeki TcpClient türünde oluşturduğumuz socket değişkeni null ise server hala full değil demektir
            for (int i = 1; i <= _MaxPlayer; i++)
            {
                if (clients[i].tcp.sockets == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"Failed to connect : {_client.Client.RemoteEndPoint}");
        }

        /// <summary>
        /// Server üzerinde clients dataları ve paketleri tutmak için Dictionary<int,Delegate(int _FromClient, Packet _packet)>
        /// değişkenine veri oluşturmaktadır.
        /// </summary>
        private static void InitializeClient()
        {
            for (int i = 1; i <=_MaxPlayer; i++)
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
