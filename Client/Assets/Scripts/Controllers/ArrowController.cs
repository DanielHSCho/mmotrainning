using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        switch (_lastDir) {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }

        base.Init();
    }

    protected override void UpdateAnimation()
    {
        // 딱히 애니메이션이 있지 않기 때문에 재정의
    }

    protected override void UpdateIdle()
    {
        if (_dir != MoveDir.None) {
            Vector3Int destPos = CellPos;

            switch (_dir) {
                case MoveDir.Up:
                    destPos += Vector3Int.up;
                    break;
                case MoveDir.Down:
                    destPos += Vector3Int.down;
                    break;
                case MoveDir.Left:
                    destPos += Vector3Int.left;
                    break;
                case MoveDir.Right:
                    destPos += Vector3Int.right;
                    break;
            }

            State = CreatureState.Moving;

            if (Managers.Map.CanGo(destPos)) {
                GameObject go = Managers.Object.Find(destPos);
                
                if (go == null) {
                    CellPos = destPos;
                } else {
                    // TEMP
                    GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
                    effect.transform.position = go.transform.position;
                    effect.GetComponent<Animator>().Play("START");
                    GameObject.Destroy(effect, 0.5f);

                    Managers.Object.Remove(go);
                    Managers.Resource.Destroy(go);

                    // 화살 제거
                    Managers.Resource.Destroy(gameObject);
                }
            } else {
                Managers.Resource.Destroy(gameObject);
            }
        }
    }
}
