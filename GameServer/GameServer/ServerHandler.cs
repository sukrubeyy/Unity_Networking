using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerHandler
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int checkClientId = _packet.ReadInt();
            string username = _packet.ReadString();

            Console.Write($"{Server.clients[checkClientId].tcp.sockets.Client.RemoteEndPoint} connected successfully and is now player {checkClientId}");
            if (checkClientId != _fromClient)
            {
                Console.WriteLine($" Player \" {username}\"(ID: {_fromClient}) has assumed the wrong client ID {checkClientId}!");
            }
            //TODO oyuncuyu oyun sahnesine gönder
        }

        public static void UDPTestReceived(int _toClient, Packet _packet)
        {
            string _msg = _packet.ReadString();
            Console.WriteLine($"Received Packet via UDP. Contains Message {_msg}");
        }
    }
}
