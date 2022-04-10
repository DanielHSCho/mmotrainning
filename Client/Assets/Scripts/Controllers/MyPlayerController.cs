using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State) {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태 전환 확인
        if (Dir != MoveDir.None) {
            State = CreatureState.Moving;
            return;
        }

        // 스킬 상태 전환 확인
        if (Input.GetKey(KeyCode.Space)) {
            State = CreatureState.Skill;
            //_coSkill = StartCoroutine("CoStartPunch");
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
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
}
