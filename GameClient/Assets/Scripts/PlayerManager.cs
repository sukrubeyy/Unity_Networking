using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
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
        PlayerController.instance.ChangeSoundEffect(2);
        if (healt <= 0f)
        {
            Die();
            PlayerController.instance.ChangeSoundEffect(4);
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
        PlayerController.instance.ChangeSoundEffect(5);
    }
}
