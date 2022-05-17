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
        if (GameManager.instance.players.TryGetValue(id, out PlayerManager _player))
        {
            _player.transform.position = position;
        }
    }

    public static void PlayerRotation(Packet _packet)
    {
        int id = _packet.ReadInt();
        Quaternion rotation = _packet.ReadQuaternion();
        if (GameManager.instance.players.TryGetValue(id, out PlayerManager _player))
        {
            _player.transform.rotation = rotation;
        }
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

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 spawnPos = _packet.ReadPosition();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, spawnPos, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        GameManager.instance.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayerId = _packet.ReadInt();
        GameManager.instance.itemSpawners[_spawnerId].ItemPickedUp();
        GameManager.instance.players[_byPlayerId].itemCount++;

    }
    public static void SpawnProjectile(Packet _packet)
    {
        int _pId = _packet.ReadInt();
        Vector3 _pos = _packet.ReadPosition();
        int _byPlayer = _packet.ReadInt();

        GameManager.instance.SpawnProjectile(_pId, _pos);
        GameManager.instance.players[_byPlayer].itemCount--;
    }
    public static void ProjectilePosition(Packet _packet)
    {
        int _pId = _packet.ReadInt();
        Vector3 _pos = _packet.ReadPosition();
        if (GameManager.instance.projectiles.TryGetValue(_pId, out ProjectileManager _projectile))
        {
            _projectile.transform.position = _pos;
        }
    }

    public static void ProjectileExp(Packet _packet)
    {
        int _pId = _packet.ReadInt();
        Vector3 _pos = _packet.ReadPosition();

        GameManager.instance.projectiles[_pId].Exp(_pos);
    }

    public static void SpawnEnemy(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _enPos = _packet.ReadPosition();
        GameManager.instance.SpawnEnemy(_id, _enPos);
    }

    public static void EnemyPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _pos = _packet.ReadPosition();
        if (GameManager.instance.enemies.TryGetValue(_id, out EnemyManager _enemy))
        {
            _enemy.transform.position = _pos;
        }
    }

    public static void EnemyHealt(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _Healt = _packet.ReadFloat();
        GameManager.instance.enemies[_id].SetHealt(_Healt);
    }
}
