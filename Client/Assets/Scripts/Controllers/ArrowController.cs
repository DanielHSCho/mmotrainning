using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
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
                if (Managers.Object.Find(destPos) == null) {
                    CellPos = destPos;
                }
            } else {
            }
        }
    }
}
