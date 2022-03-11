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

            public void Connect(TcpClient _client)
            {
                sockets = _client;
                sockets.ReceiveBufferSize = dataBufferSize;
                sockets.SendBufferSize = dataBufferSize;

                stream = sockets.GetStream();
                receiveData=new Packet();
                receiveBuffer = new byte[dataBufferSize];
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);

                ServerSend.Welcome(id, "Welcome To The Server");
            }
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
                        receiveData.Reset(HandleData(data));
                        stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
                   
                }
                catch(Exception ex)
                {
                    Console.Write($"ERROR : {ex.Message}");
                }
            }

            private bool HandleData(byte[] _Data)
            {
                int _packetLength = 0;
                receiveData.SetBytes(_Data);

                if (receiveData.UnreadLength() >= 4)
                {
                    _packetLength = receiveData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receiveData.UnreadLength())
                {
                    byte[] _packetBytes = receiveData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
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

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

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
