using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int id;
    public float healt;
    public float maxHealt = 100;

    public void Initialize(int _id)
    {
        id = _id;
        healt = maxHealt;
    }

    public void SetHealt(float _healt)
    {
        healt = _healt;
        if (healt <= 0)
        {
            GameManager.instance.enemies.Remove(id);
            Destroy(gameObject);
        }
    }
}
