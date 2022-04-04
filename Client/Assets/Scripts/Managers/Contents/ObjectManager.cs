using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    // TODO : Dictionary<int, GameObject>
    List<GameObject> _objects = new List<GameObject>();

    public void Add(GameObject go)
    {
        _objects.Add(go);
    }

    public void Remove(GameObject go)
    {
        _objects.Remove(go);
    }

    // TODO : 매우 느린 성능 - 샘플용
    public GameObject Find(Vector3Int cellPos)
    {
        
    }

    public void Clear()
    {
        _objects.Clear();
    }
 
}
