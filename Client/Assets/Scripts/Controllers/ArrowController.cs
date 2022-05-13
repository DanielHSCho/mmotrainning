using Google.Protobuf.Protocol;
using UnityEngine;

public class ArrowController : BaseController
{
    protected override void Init()
    {
        int rotateZ = 0;
        switch (Dir) {
            case MoveDir.Up:
                rotateZ = 0;
                break;
            case MoveDir.Down:
                rotateZ = -180;
                break;
            case MoveDir.Left:
                rotateZ = 90;
                break;
            case MoveDir.Right:
                rotateZ = -90;
                break;
        }

        transform.rotation = Quaternion.Euler(0, 0, rotateZ);
        State = CreatureState.Moving;

        base.Init();
    }

    protected override void UpdateAnimation()
    {
        // 별도 처리 하지 않음
    }
}
