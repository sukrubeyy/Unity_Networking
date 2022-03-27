using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string userName;
    public float healt;
    public float maxHealt;
    public MeshRenderer model;

    public void Initialize(int _id, string _userName)
    {
        id = _id;
        userName = _userName;
        healt = maxHealt;

    }
    
    public void SetHealt(float _healt)
    {
        healt = _healt;
        if (healt <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealt(maxHealt);
    }

}
