using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public GameObject effectsObject;
    public  AudioSource shootSound;
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            ClientSend.PlayerShoot(cameraTransform.forward);
            effectsObject.GetComponent<ParticleSystem>().Play();
            shootSound.Play();
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClientSend.PlayerThrowItem(cameraTransform.forward);
        }
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

        ClientSend.PlayerMovement(_inputs);
    }
}
