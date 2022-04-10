using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    public int id;
    public Player player;
    public TCP tcp;
    public UDP udp;
    public static int dataBufferSize = 4096;
    public Client(int _id)
    {
        id = _id;
        tcp = new TCP(id);
        udp = new UDP(id);
    }


    public class TCP
    {
        public TcpClient sockets;
        private NetworkStream stream;
        private Packet receiveData;
        private readonly int id;
        private byte[] receiveBuffer;
        public TCP(int _id)
        {
            id = _id;
        }

        /// <summary>
        /// Client protokole giriþ yapar, Veri boyutlarý belirlenir, Að izlenmeye ve okunmaya baþlar,
        /// Server Client'a hoþgeldin mesajý gönderir.
        /// </summary>
        /// <param name="_client">Protokol'e giriþi kabul edilen kullanýcý.</param>
        public void Connect(TcpClient _client)
        {
            sockets = _client;
            //Alýnabilecek paket boyutu
            sockets.ReceiveBufferSize = dataBufferSize;
            //Gönderilebilecek paket boyutu
            sockets.SendBufferSize = dataBufferSize;
            //Client'ýn TCP aðýndaki durumu izlenmeye baþlanýyor
            stream = sockets.GetStream();

            //Oyunda bulunan client tarafýndan gönderilen paketleri tutacak deðiþken
            receiveData = new Packet();
            receiveBuffer = new byte[dataBufferSize];
            //Að üzerindeki veriyi okumaya baþla.
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
            ServerSend.Welcome(id, "Welcome To The Server");
        }

        /// <summary>
        /// Client'a TCP paketi gönderir.
        /// </summary>
        /// <param name="_packet"></param>
        public void SendData(Packet _packet)
        {
            try
            {
                if (sockets != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($" ERROR SENDÝNG DATA TO PLAYER {id} VÝA TCP : {e.Message}");
            }
        }

        /// <summary>
        /// TCP Að üzerinden Client'a gelen paketleri yakalar.
        /// </summary>
        /// <param name="result"></param>
        private void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);
                //Paketi yeniden kullanabilmek için sýfýrlar.
                receiveData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);

            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR receiving tcp data : {ex.Message}");
                Server.clients[id].Disconnect();
            }
        }

        /// <summary>
        /// Server'a gönderilen UDP paketini yakalar.
        /// </summary>
        /// <param name="_Data"></param>
        /// <returns></returns>
        private bool HandleData(byte[] _Data)
        {
            int _packetLength = 0;
            //Paketi receiveData'ya atarýz.
            receiveData.SetBytes(_Data);
            //Okunan deðer 4'ten büyükse
            if (receiveData.UnreadLength() >= 4)
            {
                //Paket boyutunu oku 0'dan küçükse veya eþitse  true döndürür ve paket sýfýrlanýr.
                _packetLength = receiveData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
            //Paket boyutu 0'dan büyükse ve okunmayan paket boyutu packetLength'den büyükse 
            while (_packetLength > 0 && _packetLength <= receiveData.UnreadLength())
            {
                //Verileri tekrar oku ve _packetBytes içerisine atama gerçekleþtir
                byte[] _packetBytes = receiveData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        //Server içerisinde bu paketi yakala ve delegate çalýþtýr
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
                _packetLength = 0;
                if (receiveData.UnreadLength() >= 4)
                {
                    _packetLength = receiveData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }
            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            sockets.Close();
            stream = null;
            receiveData = null;
            receiveBuffer = null;
            sockets = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint;
        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _point)
        {
            endPoint = _point;
        }

        /// <summary>
        /// Client'a UDP paketi gönderir.
        /// </summary>
        /// <param name="_packet"></param>
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>
        /// UDP AÐ üzerinden Gönderilen paket'in boyutunu ve paketi byte olarak okur.
        /// Ardýndan Server içerisinde bulunan packetHandlers'da bulunan Delegate'i çalýþtýrýr.
        /// </summary>
        /// <param name="_packet">Server Class'ý içerisinde UDP Protokolü paket yakalayýcýsý method'u içerisinde gönderilen paket</param>
        public void HandleData(Packet _packet)
        {
            int _packetLength = _packet.ReadInt();
            byte[] _packetBytes = _packet.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int packetId = _packet.ReadInt();
                    Server.packetHandlers[packetId](id, _packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string _playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(id, _playerName);
        //Tüm player'larý benim için spawn eder
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }

        //Tüm kullanýcýlarda benim spawn olmamý saðlar
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }

        foreach (ItemSpawner _itemSpawner in ItemSpawner.spawners.Values)
        {
            ServerSend.CreateItemSpawner(id, _itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem);
        }


        foreach (Enemy _enemy in Enemy.enemies.Values)
        {
            ServerSend.SpawnEnemy(id, _enemy);
        }
    }

    public void Disconnect()
    {

        Debug.Log($"{tcp.sockets.Client.RemoteEndPoint} Disconnect from the Server");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

        tcp.Disconnect();
        udp.Disconnect();
        ServerSend.PlayerDisconnect(id);
    }

}
