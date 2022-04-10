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
        /// Client protokole giri� yapar, Veri boyutlar� belirlenir, A� izlenmeye ve okunmaya ba�lar,
        /// Server Client'a ho�geldin mesaj� g�nderir.
        /// </summary>
        /// <param name="_client">Protokol'e giri�i kabul edilen kullan�c�.</param>
        public void Connect(TcpClient _client)
        {
            sockets = _client;
            //Al�nabilecek paket boyutu
            sockets.ReceiveBufferSize = dataBufferSize;
            //G�nderilebilecek paket boyutu
            sockets.SendBufferSize = dataBufferSize;
            //Client'�n TCP a��ndaki durumu izlenmeye ba�lan�yor
            stream = sockets.GetStream();

            //Oyunda bulunan client taraf�ndan g�nderilen paketleri tutacak de�i�ken
            receiveData = new Packet();
            receiveBuffer = new byte[dataBufferSize];
            //A� �zerindeki veriyi okumaya ba�la.
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
            ServerSend.Welcome(id, "Welcome To The Server");
        }

        /// <summary>
        /// Client'a TCP paketi g�nderir.
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
                Debug.Log($" ERROR SEND�NG DATA TO PLAYER {id} V�A TCP : {e.Message}");
            }
        }

        /// <summary>
        /// TCP A� �zerinden Client'a gelen paketleri yakalar.
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
                //Paketi yeniden kullanabilmek i�in s�f�rlar.
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
        /// Server'a g�nderilen UDP paketini yakalar.
        /// </summary>
        /// <param name="_Data"></param>
        /// <returns></returns>
        private bool HandleData(byte[] _Data)
        {
            int _packetLength = 0;
            //Paketi receiveData'ya atar�z.
            receiveData.SetBytes(_Data);
            //Okunan de�er 4'ten b�y�kse
            if (receiveData.UnreadLength() >= 4)
            {
                //Paket boyutunu oku 0'dan k���kse veya e�itse  true d�nd�r�r ve paket s�f�rlan�r.
                _packetLength = receiveData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
            //Paket boyutu 0'dan b�y�kse ve okunmayan paket boyutu packetLength'den b�y�kse 
            while (_packetLength > 0 && _packetLength <= receiveData.UnreadLength())
            {
                //Verileri tekrar oku ve _packetBytes i�erisine atama ger�ekle�tir
                byte[] _packetBytes = receiveData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        //Server i�erisinde bu paketi yakala ve delegate �al��t�r
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
        /// Client'a UDP paketi g�nderir.
        /// </summary>
        /// <param name="_packet"></param>
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>
        /// UDP A� �zerinden G�nderilen paket'in boyutunu ve paketi byte olarak okur.
        /// Ard�ndan Server i�erisinde bulunan packetHandlers'da bulunan Delegate'i �al��t�r�r.
        /// </summary>
        /// <param name="_packet">Server Class'� i�erisinde UDP Protokol� paket yakalay�c�s� method'u i�erisinde g�nderilen paket</param>
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
        //T�m player'lar� benim i�in spawn eder
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

        //T�m kullan�c�larda benim spawn olmam� sa�lar
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
