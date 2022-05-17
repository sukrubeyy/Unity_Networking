using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string userName;
    public float healt;
    public float maxHealt;
    public GameObject model;
    public int itemCount = 0;
   
    public void Initialize(int _id, string _userName)
    {
        id = _id;
        userName = _userName;
        healt = maxHealt;
    }
    
    public void SetHealt(float _healt)
    {
        healt = _healt;
        LocalPlayerUIController.SetHealtMaterial(healt / 100f);
        if (healt <= 0f)
        {
            Die();
            LocalPlayerUIController.SetHealtMaterial(1f);
        }
    }
    public void Die()
    {
        model.SetActive(false);
    }

    public void Respawn()
    {
        model.SetActive(true);
        SetHealt(maxHealt);
    }
}
