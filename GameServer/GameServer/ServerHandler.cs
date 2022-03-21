using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GameServer
{
    class ServerHandler
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int checkClientId = _packet.ReadInt();
            string username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[checkClientId].tcp.sockets.Client.RemoteEndPoint} connected successfully and is now player {checkClientId}");
            if (checkClientId != _fromClient)
            {
                Console.WriteLine($" Player \" {username}\"(ID: {_fromClient}) has assumed the wrong client ID {checkClientId}!");
            }
            Server.clients[_fromClient].SendIntoGame(username);
        }

        public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            bool[] _inputs = new bool[_packet.ReadInt()];
            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = _packet.ReadBool();
            }
            Quaternion _rotation = _packet.ReadQuaternion();
            Server.clients[_fromClient].player.SetInput(_inputs,_rotation);
        }
    }
}
