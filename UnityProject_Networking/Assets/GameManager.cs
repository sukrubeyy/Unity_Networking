using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance=this;
           
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
        if(_id==Client.instance.myId){
            player=Instantiate(localPlayerPrefab,_position,_rotation);
        }
        else
        {
            player=Instantiate(playerPrefab,_position,_rotation);
        }

        player.GetComponent<PlayerManager>().id=_id;
        player.GetComponent<PlayerManager>().userName=_userName;
        players.Add(_id,player.GetComponent<PlayerManager>());
    }
}
