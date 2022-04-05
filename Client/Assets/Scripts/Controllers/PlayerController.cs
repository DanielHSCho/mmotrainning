using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    Coroutine _coSkill;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State) {
            case CreatureState.Idle:
                GetDirInput();
                GetIdleInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }
        base.UpdateController();
    }

    private void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetDirInput()
    {
        // TODO : 이동 부분은 인풋 매니저화 해야함
        if (Input.GetKey(KeyCode.W)) {
            Dir = MoveDir.Up;
        } else if (Input.GetKey(KeyCode.S)) {
            Dir = MoveDir.Down;
        } else if (Input.GetKey(KeyCode.A)) {
            Dir = MoveDir.Left;
        } else if (Input.GetKey(KeyCode.D)) {
            Dir = MoveDir.Right;
        } else {
            Dir = MoveDir.None;
        }
    }

    void GetIdleInput()
    {
        if (Input.GetKey(KeyCode.Space)) {
            State = CreatureState.Skill;
            //_coSkill = StartCoroutine("CoStartPunch");
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
    }

    IEnumerator CoStartPunch()
    {
        // 피격 판정
        GameObject go = Managers.Object.Find(GetFrontCellPos());
        if(go != null) {
            Debug.Log(go.name);
        }

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

        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
    }
}
