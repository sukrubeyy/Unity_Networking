using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string userName;
    public CharacterController characterController;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float yVelocity = 0;
    public bool[] inputs;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        yVelocity *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _userName)
    {
        id = _id;
        userName = _userName;
    
        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        Vector2 _inputDirection = Vector2.zero;

        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }
        Move(_inputDirection);
    }

    public void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (characterController.isGrounded)
        {
            yVelocity = 0f;
            //Karakter Space Tuþuna bastýgýnda
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }

        yVelocity += gravity;
        _moveDirection.y = yVelocity;

        characterController.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
}
