using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public GameObject playerPrefab;
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

        //Optimizasyon i�in 
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

    #if UNITY_EDITOR
        Debug.Log("Build the project to start the server!");
    #else
        Server.Start(50, 26950);
    #endif
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
