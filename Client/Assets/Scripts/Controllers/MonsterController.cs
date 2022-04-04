using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        // GetDirInput();
        base.UpdateController();
    }

    void GetDirInput()
    {
        // TODO : 이동 부분은 인풋 매니저화 해야함
        if (Input.GetKey(KeyCode.W)) {
            //transform.position += Vector3.up * Time.deltaTime * _speed;
            Dir = MoveDir.Up;
        } else if (Input.GetKey(KeyCode.S)) {
            //transform.position += Vector3.down * Time.deltaTime * _speed;
            Dir = MoveDir.Down;
        } else if (Input.GetKey(KeyCode.A)) {
            //transform.position += Vector3.left * Time.deltaTime * _speed;
            Dir = MoveDir.Left;
        } else if (Input.GetKey(KeyCode.D)) {
            //transform.position += Vector3.right * Time.deltaTime * _speed;
            Dir = MoveDir.Right;
        } else {
            Dir = MoveDir.None;
        }
    }
}
