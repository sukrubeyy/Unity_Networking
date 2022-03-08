using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int _DataBufferSize = 4096;
    public string ip = "127.0.0.1";
    public int myId = 0;
    public int _Port = 26950;
    public TCP tcp;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        IntitializeClientData();

        tcp.Connect();

    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receiveData;
        public void Connect()
        {
            
            socket = new TcpClient
            {
                ReceiveBufferSize = _DataBufferSize,
                SendBufferSize = _DataBufferSize
            };

            receiveBuffer = new byte[_DataBufferSize];
            socket.BeginConnect(instance.ip, instance._Port, ConnectCallBack, socket);
        }

        private void ConnectCallBack(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                return;
            }


            stream = socket.GetStream();
            receiveData = new Packet();
            stream.BeginRead(receiveBuffer, 0, _DataBufferSize, ReceiveCallBack, null);
        }

        private void ReceiveCallBack(IAsyncResult result)
        {

            try
            {
                int bytLength = stream.EndRead(result);
                if (bytLength <= 0)
                {
                    return;
                }


                byte[] data = new byte[bytLength];
                Array.Copy(receiveBuffer, data, bytLength);
                receiveData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, _DataBufferSize, ReceiveCallBack, null);

            }
            catch (Exception e)
            {
                //TODO Disconnect
                Console.WriteLine($"ERROR : {e.Message}");
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
                        packetHandlers[_packetId](_packet);
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

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(),0,_packet.Length(),null,null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error Sending Data to server via TCP  {e.Message}");
            }
        }


    }

    private void IntitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>(){
            {(int)ServerPackets.welcome,ClientHandle.Welcome}
        };
        Debug.Log("Initialize Packets");
    }




}
