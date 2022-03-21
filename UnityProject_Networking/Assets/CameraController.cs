using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float _sensitivy = 100f;
    public float _clampCamera = 85f;
    public PlayerManager player;

    float _verticalRotation;
    float _horizontalRotation;


    void Start()
    {
        //Player kameras� child obje oldu�u i�in local angels al�yoruz
        _verticalRotation = transform.localEulerAngles.x;
        //Sa�a sola d�necek olan player objesi oldu�u i�in onun y rotasyonunu al�yoruz
        _horizontalRotation = player.transform.eulerAngles.y;
    }
    
    void Update()
    {
        CameraRotation();
        Debug.DrawRay(transform.position, transform.forward * 2,Color.magenta);
    }

    void CameraRotation()
    {
        float _mouseY = -Input.GetAxis("Mouse Y");
        float _mouseX = Input.GetAxis("Mouse X");

        _verticalRotation += _mouseY * _sensitivy * Time.deltaTime;
        _horizontalRotation += _mouseX * _sensitivy * Time.deltaTime;

        _verticalRotation = Mathf.Clamp(_verticalRotation, -_clampCamera, _clampCamera);
        transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        player.transform.rotation = Quaternion.Euler(0, _horizontalRotation, 0); 
    }
}
