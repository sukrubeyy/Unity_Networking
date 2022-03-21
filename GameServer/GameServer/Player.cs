using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GameServer
{
    public class Player
    {
        public int id;
        public string userName;
        public Vector3 position;
        public Quaternion rotation;
        public float moveSpeed = 5f / Constants.TICKS_PER_SEC;
        public bool[] inputs;

        public Player(int _id, string _userName, Vector3 _position)
        {
            id = _id;
            userName = _userName;
            position = _position;
            rotation = Quaternion.Identity;
            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 _inputDirection = Vector2.Zero;

            if (inputs[0])
            {
                _inputDirection.Y += 1;
            }
            if (inputs[1])
            {
                _inputDirection.Y -= 1;
            }
            if (inputs[2])
            {
                _inputDirection.X += 1;
            }
            if (inputs[3])
            {
                _inputDirection.X -= 1;
            }
            Move(_inputDirection);
        }

        public void Move(Vector2 _inputDirection)
        {
            //player'in yüzünün transformu = baktığı yön
            Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

            Vector3 _moveDirection = _right * _inputDirection.X + _forward * _inputDirection.Y;
            position += _moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);


        }
        public void SetInput(bool[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}
