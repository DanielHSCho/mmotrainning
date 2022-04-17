using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coSkill;

    [SerializeField]
    bool _rangedSkill = false;

    protected override void Init()
    {
        base.Init();

        State = CreatureState.Idle;
        Dir = MoveDir.Down;

        _rangedSkill = (Random.Range(0, 2) == 0 ? true : false);
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

    IEnumerator CoStartPunch()
    {
        // 피격 판정
        GameObject go = Managers.Object.FindCreature(GetFrontCellPos());
        if (go != null) {
            CreatureController controller = go.GetComponent<CreatureController>();
            if (controller != null) {
                controller.OnDamaged();
            }
        }

        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Moving;
        _coSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController arrowController = go.GetComponent<ArrowController>();
        arrowController.Dir = Dir;
        arrowController.CellPos = CellPos;

        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Moving;
        _coSkill = null;
    }
}
