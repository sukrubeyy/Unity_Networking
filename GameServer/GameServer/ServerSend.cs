using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerSend 
    {
        public static void SendTCPData(int _toClient, Packet _packet)
        {
            //Packet uzunluğunu hesaplar
            _packet.WriteLength();

            //Paket verisini Client'a gönderiyorum
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        public static void SendTCPDataAll(Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 0; i <= Server._MaxPlayer; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        public static void SendTCPDataAll(int _except , Packet _packet)
        {
            _packet.WriteLength();

            for (int i = 0; i <= Server._MaxPlayer; i++)
            {
                if(i!=_except)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }
        public static void Welcome(int _toClient, string msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(msg);
                _packet.Write(_toClient);
                SendTCPData(_toClient,_packet);
            }
        }
    }
}
