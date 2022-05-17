using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public Transform[] playerSpawnPos;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("NetworkManager is Already Exist!");
            Destroy(this);
        }
    }
    private void Start()
    {
        //Optimizasyon için 
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        Server.Start(50, 26950);
        }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {

        int rnd = Random.Range(0, playerSpawnPos.Length);
        return Instantiate(playerPrefab, playerSpawnPos[rnd].position, Quaternion.identity).GetComponent<Player>();
    }
    
    public Projectile InstantieProjectile(Transform _spawnPos)
    {
        return Instantiate(projectilePrefab, _spawnPos.position + _spawnPos.forward * 0.7f, Quaternion.identity).GetComponent<Projectile>();
    }

    public void InstantieEnemy(Vector3 position)
    {
        Instantiate(enemyPrefab, position, Quaternion.identity);
    }


}
