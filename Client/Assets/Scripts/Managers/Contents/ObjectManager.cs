using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public void Add(PlayerInfo info, bool myPlayer = false)
    {
        if (myPlayer) {
            GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
            go.name = info.Name;
            _objects.Add(info.PlayerId, go);

            MyPlayer = go.GetComponent<MyPlayerController>();
            MyPlayer.Id = info.PlayerId;
            MyPlayer.CellPos = new Vector3Int(info.PosX, info.PosY, 0);
        } else {
            GameObject go = Managers.Resource.Instantiate("Creature/Player");
            go.name = info.Name;
            _objects.Add(info.PlayerId, go);

            PlayerController pc = go.GetComponent<PlayerController>();
            pc.Id = info.PlayerId;
            pc.CellPos = new Vector3Int(info.PosX, info.PosY, 0);
        }
    }

    // TODO : 팩토리 패턴으로 Id 전달 시, 해당 id에 해당하는 애를 만들어주도록 개선해야함
    public void Add(int id, GameObject go)
    {
        _objects.Add(id, go);
    }

    public void Remove(int id)
    {
        _objects.Remove(id);
    }

    public void RemoveMyPlayer()
    {
        if(MyPlayer == null) {
            return;
        }

        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    // TODO : 매우 느린 성능 - 샘플용
    public GameObject Find(Vector3Int cellPos)
    {
        foreach(GameObject obj in _objects.Values) {
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
        foreach (GameObject obj in _objects.Values) {
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
