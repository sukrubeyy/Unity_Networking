using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float freeQuency = 3f;
    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(freeQuency);
        if (Enemy.enemies.Count < Enemy.maxEnemy)
        {
            NetworkManager.instance.InstantieEnemy(transform.position);
        }
        StartCoroutine(SpawnEnemy());
    }
}
