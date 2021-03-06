using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerSend 
    {
        #region TCP
        /// <summary>
        /// Belirtilen kullanıcıya TCP paketi gönderir
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_packet"></param>
        public static void SendTCPData(int _toClient, Packet _packet)
        {
            //Packet uzunluğunu hesaplar
            _packet.WriteLength();
            //Paket verisini Client'a gönderiyorum
            Server.clients[_toClient].tcp.SendData(_packet);
        }
      
        /// <summary>
        /// Herkese TCP paketi gönderir
        /// </summary>
        /// <param name="_packet"></param>
        public static void SendTCPDataAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server._MaxPlayer; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        /// <summary>
        /// Belirtilen kullanıcı hariç herkese TCP paketi gönderir.
        /// </summary>
        /// <param name="_except"></param>
        /// <param name="_packet"></param>
        public static void SendTCPDataAll(int _except , Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server._MaxPlayer; i++)
            {
                if(i!=_except)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }
        #endregion

        #region UDP

        /// <summary>
        /// Belirtilen kullanıcıya UDP paketi gönderir
        /// </summary>
        /// <param name="_toClient">UDP paketinin gidilmesi istenilen kullanıcı</param>
        /// <param name="_packet">Gönderilecek Paket</param>
        public static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>
        /// Tüm client'lara UDP paketi gönder
        /// </summary>
        /// <param name="_packet"></param>
        public static void SendUDPDataAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server._MaxPlayer; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        /// <summary>
        /// Belirtilen Kullanıcı hariç herkese paket gönderir
        /// </summary>
        /// <param name="_except">Paket gönderilmesi istenilmeyen client</param>
        /// <param name="_packet"></param>
        public static void SendUDPDataAll(int _except, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server._MaxPlayer; i++)
            {
                if (i != _except)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #endregion
       
        /// <summary>
        /// Server'a bağlanan kullanıcıya hoşgeldin mesajı gönderir.
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="msg"></param>
        public static void Welcome(int _toClient, string msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(msg);
                _packet.Write(_toClient);
                SendTCPData(_toClient,_packet);
            }
        }
    
        public static void SpawnPlayer(int _id, Player _player)
        {
            using(Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.userName);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_id,_packet);
            }
        }

        public static void PlayerPosition(Player _player)
        {
            using(Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);
                SendUDPDataAll(_packet);
            }
        }
        public static void PlayerRotation(Player _player)
        {
            using(Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);
                SendUDPDataAll(_player.id, _packet);
                
            }
        }
    
    }
}
