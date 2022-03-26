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
    /// Maksimum player ve port atamas� ayr�ca tcp,udp protokollerini olu�turmaktad�r.
    /// </summary>
    /// <param name="_maxPlayer">Server'daki maksimum oyuncu say�n� belirler</param>
    /// <param name="_port">Server port adresini belirliyorz</param>
    public static void Start(int _maxPlayer, int _port)
    {
        _MaxPlayer = _maxPlayer;
        _Port = _port;

        Debug.Log("Server Starting....");


        InitializeClient();

        tcpListener = new TcpListener(IPAddress.Any, _Port);
        tcpListener.Start();

        //TCP protokol�ne client'lar�n giri� iste�i g�nderebilmesini ba�lat�yoruz.
        tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        udpListener = new UdpClient(_port);

        //UDP protokol�n� t�rettik ve paket almas�n� sa�l�yoruz.
        udpListener.BeginReceive(UDPReceiveCallback, null);
        Debug.Log($"Server Started on port :  {_Port}");

    }

    /// <summary>
    /// Client taraf�ndan g�nderilen UDP paketlerini yakalar
    /// </summary>
    /// <param name="_result"></param>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //Paket al�m�n� sonland�r�r ve al�nan paketleri byte[] t�r�nde d�nd�r�r.
            //paketleri yakalay�p _data de�i�kenine at�yoruz
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            //Ard�ndan UDP protokol�ndeki paket yakalamay� tekrar aktif hale getiriyoruz
            udpListener.BeginReceive(UDPReceiveCallback, null);

            //Veri boyutu 4'ten ufaksa i�lemi bittir
            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int clientID = _packet.ReadInt();
                //Sunucu id'si 0'a e�it olacak bu y�zden e�er herhangi bir client id 0 olursa server ��ker.
                if (clientID == 0)
                {
                    return;
                }
                //E�er g�nderilen pakette client'�n id'si yer alm�yorsa udp protokol�n� client class'� i�erisinde tekrar ba�la ve i�lemi k�r
                if (clients[clientID].udp.endPoint == null)
                {
                    clients[clientID].udp.Connect(_clientEndPoint);
                    return;
                }

                //Client'�n udp protokol�nde bulunan ID ile Port �zerinde gelen paket id'si e�itse
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
    /// Client'a Udp Paketi G�nderir
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
    /// TCP Protokol�ne Herhangi bir client ba�lanmaya �al��t���nda �al��acak method.
    /// </summary>
    /// <param name="result"></param>
    private static void TcpConnectCallback(IAsyncResult result)
    {
        //TCP protokol�ne gelen iste�i kabul eder ve geriye TcpClient t�r�nde de�er d�nd�r�r.
        TcpClient _client = tcpListener.EndAcceptTcpClient(result);

        //Protokol'e gelen isteklerin devam etmesini sa�l�yoruz.
        tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
        Debug.Log($"Incoming Connected From {_client.Client.RemoteEndPoint}");

        //Ba�lang��ta olu�tudu�umuz Dictionary<int,Client>'in t�m de�erlerini d�n�yoruz. Client class'� i�inde bulunan
        //Tcp class'� i�indeki TcpClient t�r�nde olu�turdu�umuz socket de�i�keni null ise server hala full de�il demektir
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
    /// Server �zerinde clients datalar� ve paketleri tutmak i�in Dictionary<int,Delegate(int _FromClient, Packet _packet)>
    /// de�i�kenine veri olu�turmaktad�r.
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
