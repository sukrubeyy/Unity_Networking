using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public Text nickName;
    public PlayerManager playerManager;

    private void Start()
    {
        if (string.IsNullOrEmpty(playerManager.userName))
        {
            int rnd = Random.Range(0, 9999);
            nickName.text = "Player"+rnd;
        }
        else if(playerManager.userName!=null)
        {
            nickName.text = playerManager.userName;
        }
    }

    

}
