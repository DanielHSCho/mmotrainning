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
        foreach(GameObject obj in _objects) {
            CreatureController creController = obj.GetComponent<CreatureController>();
            if(creController == null) {
                continue;
            }

            if(creController.CellPos == cellPos) {
                return obj;
            }
        }

        return null;
    }

    public void Clear()
    {
        _objects.Clear();
    }
 
}
