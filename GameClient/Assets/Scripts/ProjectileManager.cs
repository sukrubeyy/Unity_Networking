using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;
    public GameObject expPEffect;

    public void Initialize(int _id)
    {
        id = _id;
    }
    public void Exp(Vector3 _v3)
    {
        transform.position = _v3;
        Instantiate(expPEffect, transform.position, Quaternion.identity);
        GameManager.instance.projectiles.Remove(id);
        Destroy(gameObject);
    }
}
