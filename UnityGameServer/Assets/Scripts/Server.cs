using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
public class Server 
{
    public static int _MaxPlayer { get; private set; }
    public static int _Port { get; private set; }
    private static TcpListener tcpListener;
    private static UdpClient udpListener;
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public static Dictionary<int, PacketHandler> packetHandlers;
    public delegate void PacketHandler(int _fromClient, Packet _packet);

    /// <summary>
    /// Maksimum player ve port atamasý ayrýca tcp,udp protokollerini oluþturmaktadýr.
    /// </summary>
    /// <param name="_maxPlayer">Server'daki maksimum oyuncu sayýný belirler</param>
    /// <param name="_port">Server port adresini belirliyorz</param>
    public static void Start(int _maxPlayer, int _port)
    {
        _MaxPlayer = _maxPlayer;
        _Port = _port;

        Debug.Log("Server Starting....");


        InitializeClient();

        tcpListener = new TcpListener(IPAddress.Any, _Port);
        tcpListener.Start();

        //TCP protokolüne client'larýn giriþ isteði gönderebilmesini baþlatýyoruz.
        tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        udpListener = new UdpClient(_port);

        //UDP protokolünü türettik ve paket almasýný saðlýyoruz.
        udpListener.BeginReceive(UDPReceiveCallback, null);
        Debug.Log($"Server Started on port :  {_Port}");

    }

    /// <summary>
    /// Client tarafýndan gönderilen UDP paketlerini yakalar
    /// </summary>
    /// <param name="_result"></param>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //Paket alýmýný sonlandýrýr ve alýnan paketleri byte[] türünde döndürür.
            //paketleri yakalayýp _data deðiþkenine atýyoruz
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            //Ardýndan UDP protokolündeki paket yakalamayý tekrar aktif hale getiriyoruz
            udpListener.BeginReceive(UDPReceiveCallback, null);

            //Veri boyutu 4'ten ufaksa iþlemi bittir
            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int clientID = _packet.ReadInt();
                //Sunucu id'si 0'a eþit olacak bu yüzden eðer herhangi bir client id 0 olursa server çöker.
                if (clientID == 0)
                {
                    return;
                }
                //Eðer gönderilen pakette client'ýn id'si yer almýyorsa udp protokolünü client class'ý içerisinde tekrar baðla ve iþlemi kýr
                if (clients[clientID].udp.endPoint == null)
                {
                    clients[clientID].udp.Connect(_clientEndPoint);
                    return;
                }

                //Client'ýn udp protokolünde bulunan ID ile Port üzerinde gelen paket id'si eþitse
                if (clients[clientID].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    //Paketi Yakala
                    clients[clientID].udp.HandleData(_packet);
                }
            }
        }

        catch (Exception _ex)
        {
            Debug.Log($"Error Receiving UDP DATA : {_ex.Message}");
        }
    }

    /// <summary>
    /// Client'a Udp Paketi Gönderir
    /// </summary>
    /// <param name="_endPoint">Client ID</param>
    /// <param name="_packet">PAKET</param>
    public static void SendUDPData(IPEndPoint _endPoint, Packet _packet)
    {
        try
        {
            if (_endPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _endPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error Sending data to {_endPoint} via UDP {_ex.Message}");
        }
    }

    /// <summary>
    /// TCP Protokolüne Herhangi bir client baðlanmaya çalýþtýðýnda çalýþacak method.
    /// </summary>
    /// <param name="result"></param>
    private static void TcpConnectCallback(IAsyncResult result)
    {
        //TCP protokolüne gelen isteði kabul eder ve geriye TcpClient türünde deðer döndürür.
        TcpClient _client = tcpListener.EndAcceptTcpClient(result);

        //Protokol'e gelen isteklerin devam etmesini saðlýyoruz.
        tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        Debug.Log($"Incoming Connected From {_client.Client.RemoteEndPoint}");

        //Baþlangýçta oluþtuduðumuz Dictionary<int,Client>'in tüm deðerlerini dönüyoruz. Client class'ý içinde bulunan
        //Tcp class'ý içindeki TcpClient türünde oluþturduðumuz socket deðiþkeni null ise server hala full deðil demektir
        for (int i = 1; i <= _MaxPlayer; i++)
        {
            if (clients[i].tcp.sockets == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"Failed to connect : {_client.Client.RemoteEndPoint}");
    }

    /// <summary>
    /// Server üzerinde clients datalarý ve paketleri tutmak için Dictionary<int,Delegate(int _FromClient, Packet _packet)>
    /// deðiþkenine veri oluþturmaktadýr.
    /// </summary>
    private static void InitializeClient()
    {
        for (int i = 1; i <= _MaxPlayer; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived,ServerHandler.WelcomeReceived },
                {(int)ClientPackets.playerMovement,ServerHandler.PlayerMovement }
            };
        Debug.Log("Initialize Packets");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
