using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int myId = _packet.ReadInt();

        Debug.Log($"Message From Server : {_msg}");
        Client.instance.myId = myId;

        //TODO SEND WELCOME RECEIVE PACKET 
        ClientSend.WelcomeReceived();

        //UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _userName = _packet.ReadString();
        Vector3 _position = _packet.ReadPosition();
        Quaternion _rotation = _packet.ReadQuaternion();
        GameManager.instance.SpawnPlayer(_id, _userName, _position, _rotation);

    }

    public static void PlayerPosition(Packet _packet)
    {
        int id = _packet.ReadInt();
        Vector3 position = _packet.ReadPosition();
        GameManager.instance.players[id].transform.position = position;
    }

    public static void PlayerRotation(Packet _packet)
    {
        int id = _packet.ReadInt();
        Quaternion rotation = _packet.ReadQuaternion();

        GameManager.instance.players[id].transform.rotation = rotation;
    }

    public static void PlayerDisconnect(Packet _packet)
    {
        int id = _packet.ReadInt();
        Destroy(GameManager.instance.players[id].gameObject);
        GameManager.instance.players.Remove(id);
    }
    
    public static void PlayerHealt(Packet _packet)
    {
        int id = _packet.ReadInt();
        float healt = _packet.ReadFloat();
        GameManager.instance.players[id].SetHealt(healt);
    }

    public static void PlayerRespawn(Packet _packet)
    {
        int id = _packet.ReadInt();
        GameManager.instance.players[id].Respawn();
    }
}
