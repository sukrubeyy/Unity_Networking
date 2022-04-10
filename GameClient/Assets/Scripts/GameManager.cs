using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();
    public Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();
    public Dictionary<int, EnemyManager> enemies = new Dictionary<int, EnemyManager>();
    public static GameManager instance;
    public GameObject itemSpawnPrefab;
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else if (instance != null)
        {
            Debug.Log("GameManager is already exist! Destroying object");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _userName, Vector3 _position, Quaternion _rotation)
    {
        GameObject player;
        if (_id == Client.instance.myId)
        {
            player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            player = Instantiate(playerPrefab, _position, _rotation);
        }

        player.GetComponent<PlayerManager>().Initialize(_id, _userName);
        players.Add(_id, player.GetComponent<PlayerManager>());
    }

    public void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem)
    {
        GameObject spawner = Instantiate(itemSpawnPrefab, _position, itemSpawnPrefab.transform.rotation);
        spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);
        itemSpawners.Add(_spawnerId, spawner.GetComponent<ItemSpawner>());
    }

    public void SpawnProjectile(int _id, Vector3 _pos)
    {
        GameObject _projectile = Instantiate(projectilePrefab, _pos, Quaternion.identity);
        _projectile.GetComponent<ProjectileManager>().Initialize(_id);
        projectiles.Add(_id, _projectile.GetComponent<ProjectileManager>());
    }

    public void SpawnEnemy(int _id, Vector3 _pos)
    {
        GameObject enemy = Instantiate(enemyPrefab, _pos, Quaternion.identity);
        enemy.GetComponent<EnemyManager>().Initialize(_id);
        enemies.Add(_id, enemy.GetComponent<EnemyManager>());        
    }
}
