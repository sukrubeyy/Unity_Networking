using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    #region TCP
    /// <summary>
    /// Belirtilen kullan�c�ya TCP paketi g�nderir
    /// </summary>
    /// <param name="_toClient"></param>
    /// <param name="_packet"></param>
    public static void SendTCPData(int _toClient, Packet _packet)
    {
        //Packet uzunlu�unu hesaplar
        _packet.WriteLength();
        //Paket verisini Client'a g�nderiyorum
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>
    /// Herkese TCP paketi g�nderir
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
    /// Belirtilen kullan�c� hari� herkese TCP paketi g�nderir.
    /// </summary>
    /// <param name="_except"></param>
    /// <param name="_packet"></param>
    public static void SendTCPDataAll(int _except, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server._MaxPlayer; i++)
        {
            if (i != _except)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }
    #endregion

    #region UDP

    /// <summary>
    /// Belirtilen kullan�c�ya UDP paketi g�nderir
    /// </summary>
    /// <param name="_toClient">UDP paketinin gidilmesi istenilen kullan�c�</param>
    /// <param name="_packet">G�nderilecek Paket</param>
    public static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>
    /// T�m client'lara UDP paketi g�nder
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
    /// Belirtilen Kullan�c� hari� herkese paket g�nderir
    /// </summary>
    /// <param name="_except">Paket g�nderilmesi istenilmeyen client</param>
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
    /// Server'a ba�lanan kullan�c�ya ho�geldin mesaj� g�nderir.
    /// </summary>
    /// <param name="_toClient"></param>
    /// <param name="msg"></param>
    public static void Welcome(int _toClient, string msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(msg);
            _packet.Write(_toClient);
            SendTCPData(_toClient, _packet);
        }
    }

    public static void SpawnPlayer(int _id, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.userName);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_id, _packet);
        }
    }

    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);
            SendUDPDataAll(_packet);
        }
    }
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);
            SendUDPDataAll(_player.id, _packet);

        }
    }
}
