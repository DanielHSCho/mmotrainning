using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coSkill;

    protected override void Init()
    {
        base.Init();

        State = CreatureState.Idle;
        Dir = MoveDir.Down;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
    }

    public override void OnDamaged()
    {
        // TODO : 나중에 오브젝트 매니저에서 한번에 처리할 수 있도록 개선해야함
        // Managers.Object.Remove(Id);
        // Managers.Resource.Destroy(this.gameObject);
    }

    public override void UseSkill(int skillId)
    {
        if (skillId == 1) {
            _coSkill = StartCoroutine("CoStartPunch");
        } else if (skillId == 2) {
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
    }
}
