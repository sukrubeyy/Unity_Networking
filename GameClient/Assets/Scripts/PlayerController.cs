using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public GameObject effectsObject;
    public AudioSource _AudioSource;
    public List<AudioClip> _SoundClips;
    public static int id;
    public static PlayerController instance;

    private void Start()
    {
        if(instance!=null)
        {
            Destroy(this);
        }
        else if(instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            ClientSend.PlayerShoot(cameraTransform.forward);
            effectsObject.GetComponent<ParticleSystem>().Play();
            ChangeSoundEffect(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClientSend.PlayerThrowItem(cameraTransform.forward);
            if (GameManager.instance.players[id].itemCount > 0)
                ChangeSoundEffect(1);
        }
        if (transform.position.y > -5)
        {
            ChangeSoundEffect(7);
        }
    }

    public void ChangeSoundEffect(int _id)
    {
        _AudioSource.clip = _SoundClips[_id];
        _AudioSource.Play();
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>
    /// Player Movement Inputlarýný alýp ClientSend.PlayerMovement methoduna gönderecek Ordan da Server'a iletilecek.
    /// </summary>
    public void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };
        if(_inputs[4])
        {
            ChangeSoundEffect(6);
        }

        ClientSend.PlayerMovement(_inputs);
    }
    public static void SetPlayerID(int _playerID)
    {
        id = _playerID;
    }
}


