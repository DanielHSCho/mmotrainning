using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        GetDirInput();
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

            if (Input.GetKey(KeyCode.Space)) {
                State = CreatureState.Skill;
            }
        }
    }
}
