using UnityEngine;

public class Bilboard : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 15);
    }
}
