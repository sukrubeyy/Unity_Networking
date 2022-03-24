using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
   void FixedUpdate()
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
            Input.GetKey(KeyCode.D)
        };

        ClientSend.PlayerMovement(_inputs);
    }
}
