using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public int spawnerID;
    public bool hasItem;
    public MeshRenderer itemModel;
    private Vector3 basePosition;
    public float itemRotSpeed = 50f;
    public float itemBobSpeed = 2f;

    private void Update()
    {
        if (hasItem)
        {
            transform.Rotate(Vector3.up, itemRotSpeed * Time.deltaTime, Space.World);
            transform.position = basePosition + new Vector3(0f, 0.25f * Mathf.Sin(Time.time * itemBobSpeed),0f);
        }
    }
    public void Initialize(int _spawnerId, bool _hasItem)
    {
        spawnerID = _spawnerId;
        hasItem = _hasItem;
        itemModel.enabled = hasItem;
        basePosition = transform.position;
    }

    public void ItemSpawned()
    {
        hasItem = true;
        itemModel.enabled = true;
    } 
    public void ItemPickedUp()
    {
        hasItem = false;
        itemModel.enabled = false;
    }
}
