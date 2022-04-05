using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    // TODO : Dictionary<int, GameObject>
    List<GameObject> _objects = new List<GameObject>();

    // TODO : 팩토리 패턴으로 Id 전달 시, 해당 id에 해당하는 애를 만들어주도록 개선해야함
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

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects) {
            if (condition.Invoke(obj)) {
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
