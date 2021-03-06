using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    #region TCP
    /// <summary>
    /// Belirtilen kullan?c?ya TCP paketi g?nderir
    /// </summary>
    /// <param name="_toClient"></param>
    /// <param name="_packet"></param>
    public static void SendTCPData(int _toClient, Packet _packet)
    {
        //Packet uzunlu?unu hesaplar
        _packet.WriteLength();
        //Paket verisini Client'a g?nderiyorum
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>
    /// Herkese TCP paketi g?nderir
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
    /// Belirtilen kullan?c? hari? herkese TCP paketi g?nderir.
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
    /// Belirtilen kullan?c?ya UDP paketi g?nderir
    /// </summary>
    /// <param name="_toClient">UDP paketinin gidilmesi istenilen kullan?c?</param>
    /// <param name="_packet">G?nderilecek Paket</param>
    public static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>
    /// T?m client'lara UDP paketi g?nder
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
    /// Belirtilen Kullan?c? hari? herkese paket g?nderir
    /// </summary>
    /// <param name="_except">Paket g?nderilmesi istenilmeyen client</param>
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
    /// Server'a ba?lanan kullan?c?ya ho?geldin mesaj? g?nderir.
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
    
    public static void PlayerDisconnect(int _id)
    {
        using(Packet _packet = new Packet((int)ServerPackets.playerDisconnect))
        {
            _packet.Write(_id);
            SendTCPDataAll(_id,_packet);
        }
    }

    public static void PlayerHealt(Player _player)
    {
        using(Packet _packet = new Packet((int)ServerPackets.playerHealt))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.healt);
            SendTCPDataAll(_packet);
        }
    }

    public static void PlayerRespawn(Player _player)
    {
        using(Packet _packet = new Packet((int)ServerPackets.playerRespawn))
        {
            _packet.Write(_player.id);
            SendTCPDataAll(_packet);
        }
    }
    
    public static void CreateItemSpawner(int _toClient, int _spawnerId, Vector3 _position, bool _hasItem)
    {
        using(Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_position);
            _packet.Write(_hasItem);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ItemSpawned(int _spawnerId)
    {
        using(Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_spawnerId);
            SendTCPDataAll(_packet);
        }
    } 
    public static void ItemPickedUp(int _spawnerId,int _byPlayer)
    {
        using(Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_byPlayer);
            SendTCPDataAll(_packet);
        }
    }

    public static void SpawnProjectile(Projectile _projectile, int _byPlayer)
    {
        using(Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);
            _packet.Write(_byPlayer);
            SendTCPDataAll(_packet);
        }
    }

    public static void ProjectilePosition(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectilePos))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);
            SendUDPDataAll(_packet);
        }
    }

    public static void ProjectileExp(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectileExp))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);
            SendTCPDataAll(_packet);
        }
    }

    public static void SpawnEnemy(Enemy _enemy)
    {
        using(Packet _packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTCPDataAll(SpawnEnemyData(_enemy, _packet));
        }
    }
    public static void SpawnEnemy(int toClient , Enemy _enemy)
    {
        using(Packet _packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTCPData(toClient,SpawnEnemyData(_enemy, _packet));
        }
    }

    private static Packet SpawnEnemyData(Enemy enemy, Packet _packet)
    {
        _packet.Write(enemy.id);
        _packet.Write(enemy.transform.position);
        return _packet;
    }

    public static void EnemyPosition(Enemy _enemy)
    {
        using(Packet _packet = new Packet((int)ServerPackets.enemyPosition))
        {
            _packet.Write(_enemy.id);
            _packet.Write(_enemy.transform.position);
            SendUDPDataAll(_packet);
        }
    }

    public static void EnemyHealt(Enemy enemy)
    {
        using(Packet _packet = new Packet((int)ServerPackets.enemyHealt))
        {
            _packet.Write(enemy.id);
            _packet.Write(enemy.healt);
            SendTCPDataAll(_packet);
        }
    }














}
