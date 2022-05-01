using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    // TODO : 팩토리 패턴으로 Id 전달 시, 해당 id에 해당하는 애를 만들어주도록 개선해야함
    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        if(MyPlayer != null && MyPlayer.Id == info.ObjectId) {
            return;
        }

        GameObjectType objectType = GetObjectTypeById(info.ObjectId);

        if(objectType == GameObjectType.Player) {
            if (myPlayer) {
                GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
                MyPlayer.Stat = info.StatInfo;

                MyPlayer.SyncPos();
            } else {
                GameObject go = Managers.Resource.Instantiate("Creature/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PosInfo = info.PosInfo;
                pc.Stat = info.StatInfo;
                pc.SyncPos();
            }
        } else if(objectType == GameObjectType.Monster) {
            GameObject go = Managers.Resource.Instantiate("Creature/Monster");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            MonsterController mc = go.GetComponent<MonsterController>();
            mc.Id = info.ObjectId;
            mc.PosInfo = info.PosInfo;
            mc.Stat = info.StatInfo;
            mc.SyncPos();

        } else if(objectType == GameObjectType.Projectile) {
            GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
            go.name = "Arrow";
            _objects.Add(info.ObjectId, go);

            ArrowController ac = go.GetComponent<ArrowController>();
            ac.PosInfo = info.PosInfo;
            ac.Stat = info.StatInfo;
            ac.SyncPos();
        }
    }

    public void Remove(int id)
    {
        if (MyPlayer != null && MyPlayer.Id == id) {
            return;
        }

        GameObject go = FindById(id);
        if(go == null) {
            return;
        }

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    // TODO : 매우 느린 성능 - 샘플용
    public GameObject FindCreature(Vector3Int cellPos)
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
        foreach (GameObject obj in _objects.Values) {
            Managers.Resource.Destroy(obj);
        }
        _objects.Clear();
        MyPlayer = null;
    }

}
