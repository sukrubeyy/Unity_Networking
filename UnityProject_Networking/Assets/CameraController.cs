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
        //Player kamerasý child obje olduðu için local angels alýyoruz
        _verticalRotation = transform.localEulerAngles.x;
        //Saða sola dönecek olan player objesi olduðu için onun y rotasyonunu alýyoruz
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
