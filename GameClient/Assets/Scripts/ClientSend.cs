using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();

        Client.instance.tcp.SendData(_packet);
    }

    public static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance._userName.text);

            SendTCPData(_packet);
        }
    }

    public static void PlayerMovement(bool[] _inputs)
    {
        using(Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool input in _inputs)
            {
                _packet.Write(input);
            }
            _packet.Write(GameManager.instance.players[Client.instance.myId].transform.rotation);
            SendUDPData(_packet);
        }
    }
    
    public static void PlayerShoot(Vector3 _shotFace)
    {
        using(Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_shotFace);
            SendTCPData(_packet);
        }
    }

    public static void PlayerThrowItem(Vector3 _face)
    {
        using(Packet _packet = new Packet((int)ClientPackets.playerThrowItem))
        {
            _packet.Write(_face);
            SendTCPData(_packet);
        }
    }

}
