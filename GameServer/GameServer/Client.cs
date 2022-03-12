using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace GameServer
{
    public class Client
    {
        public  int id;
        public  TCP tcp;
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
            public  TcpClient sockets;
            private NetworkStream stream;
            private Packet receiveData;
            private readonly  int id;
            private byte[] receiveBuffer;
            public TCP(int _id)
            {
                id = _id;
            }

            /// <summary>
            /// Client protokole giriş yapar, Veri boyutları belirlenir, Ağ izlenmeye ve okunmaya başlar,
            /// Server Client'a hoşgeldin mesajı gönderir.
            /// </summary>
            /// <param name="_client">Protokol'e girişi kabul edilen kullanıcı.</param>
            public void Connect(TcpClient _client)
            {
                sockets = _client;
                //Alınabilecek paket boyutu
                sockets.ReceiveBufferSize = dataBufferSize;
                //Gönderilebilecek paket boyutu
                sockets.SendBufferSize = dataBufferSize;
                //Client'ın TCP ağındaki durumu izlenmeye başlanıyor
                stream = sockets.GetStream();

                //Oyunda bulunan client tarafından gönderilen paketleri tutacak değişken
                receiveData=new Packet();
                receiveBuffer = new byte[dataBufferSize];
                //Ağ üzerindeki veriyi okumaya başla.
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
                    Console.WriteLine($" ERROR SENDİNG DATA TO PLAYER {id} VİA TCP : {e.Message}");
                }
            }

            /// <summary>
            /// TCP Ağ üzerinden Client'a gelen paketleri yakalar.
            /// </summary>
            /// <param name="result"></param>
            private void ReceiveCallBack(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        return;
                    }
                        
                        byte[] data = new byte[byteLength];
                        Array.Copy(receiveBuffer, data, byteLength);
                        //Paketi yeniden kullanabilmek için sıfırlar.
                        receiveData.Reset(HandleData(data));
                        stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
                   
                }
                catch(Exception ex)
                {
                    Console.Write($"ERROR : {ex.Message}");
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
                //Paketi receiveData'ya atarız.
                receiveData.SetBytes(_Data);
                //Okunan değer 4'ten büyükse
                if (receiveData.UnreadLength() >= 4)
                {
                     //Paket boyutunu oku 0'dan küçükse true döndürür ve paket sıfırlanır.
                    _packetLength = receiveData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
                //Paket boyutu 0'dan büyükse ve okunmayan paket boyutu packetLength'den büyükse 
                while (_packetLength > 0 && _packetLength <= receiveData.UnreadLength())
                {
                    //Verileri tekrar oku ve _packetBytes içerisine atama gerçekleştir
                    byte[] _packetBytes = receiveData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            //Server içerisinde bu paketi yakala ve delegate çalıştır
                            Server.packetHandlers[_packetId](id,_packet);
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
                ServerSend.UDPTest(id);
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
            /// UDP AĞ üzerinden Gönderilen paket'in boyutunu ve paketi byte olarak okur.
            /// Ardından Server içerisinde bulunan packetHandlers'da bulunan Delegate'i çalıştırır.
            /// </summary>
            /// <param name="_packet">Server Class'ı içerisinde UDP Protokolü paket yakalayıcısı method'u içerisinde gönderilen paket</param>
            public void HandleData(Packet _packet)
            {
                int _packetLength = _packet.ReadInt();
                byte[] _packetBytes = _packet.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using(Packet _packet = new Packet(_packetBytes))
                    {
                        int packetId = _packet.ReadInt();
                        Server.packetHandlers[packetId](id,_packet);
                    }
                });
            }
        }
    
    
    }
}
