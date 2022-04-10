using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected Coroutine _coSkill;
    protected bool _rangeSkill = false;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (State == CreatureState.Idle) {
            switch (_lastDir) {
                case MoveDir.Up:
                    _animator.Play("IDLE_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = true;
                    break;
                default:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        } else if (State == CreatureState.Moving) {
            switch (Dir) {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        } else if (State == CreatureState.Skill) {
            switch (_lastDir) {
                case MoveDir.Up:
                    _animator.Play(_rangeSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play(_rangeSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play(_rangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play(_rangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        } else {

        }
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태 전환 확인
        if(Dir != MoveDir.None) {
            State = CreatureState.Moving;
            return;
        }
    }

    IEnumerator CoStartPunch()
    {
        // 피격 판정
        GameObject go = Managers.Object.Find(GetFrontCellPos());
        if(go != null) {
            CreatureController controller = go.GetComponent<CreatureController>();
            if (controller != null) {
                controller.OnDamaged();
            }
        }

        _rangeSkill = false;
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController arrowController = go.GetComponent<ArrowController>();
        arrowController.Dir = _lastDir;
        arrowController.CellPos = CellPos;

        _rangeSkill = true;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
    }

    public override void OnDamaged()
    {
        Debug.Log("Player Hit!");
    }
}
