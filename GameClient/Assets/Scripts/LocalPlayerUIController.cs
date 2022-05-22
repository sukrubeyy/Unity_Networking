using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerUIController : MonoBehaviour
{
    public GameObject _HealtBarImage;
    public static Material _HealtBarMat;

    private void Start()
    {
        _HealtBarMat = _HealtBarImage.GetComponent<Image>().material;
    }

    public void SetHealtMaterial(float _value)
    {
        _HealtBarMat.SetFloat("_HealtBarValue", _value);
    }
}
