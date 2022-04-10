using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    public int Id { get; set; }

    [SerializeField]
    public float _speed = 5.0f;

    protected bool _updated = false;

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set {
            if (_positionInfo.Equals(value)) {
                return;
            }

            _positionInfo = value;
            UpdateAnimation();
        }
    }

    public Vector3Int CellPos {
        get {
            return new Vector3Int(PosInfo.PosX, PosInfo.PosY, 0);
        }

        set {
            if(PosInfo.PosX == value.x && PosInfo.PosY == value.y) {
                return;
            }

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }
    protected Animator _animator;
    protected SpriteRenderer _sprite;

    public virtual CreatureState State
    {
        get { return PosInfo.State; }
        set {
            if (PosInfo.State == value) {
                return;
            }

            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }

    protected MoveDir _lastDir = MoveDir.Down;
    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set {
            if (PosInfo.MoveDir == value) {
                return;
            }

            PosInfo.MoveDir = value;
            if (value != MoveDir.None) {
                _lastDir = value;
            }

            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir GetDirFromVec(Vector3Int dir)
    {
        if (dir.x > 0) {
            return MoveDir.Right;
        } else if (dir.x < 0) {
            return MoveDir.Left;
        } else if (dir.y > 0) {
            return MoveDir.Up;
        } else if (dir.y < 0) {
            return MoveDir.Down;
        } else {
            return MoveDir.None;
        }
    }

    public Vector3Int GetFrontCellPos()
    {
        Vector3Int cellPos = CellPos;

        switch (_lastDir) {
            case MoveDir.Up:
                cellPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellPos += Vector3Int.right;
                break;
        }

        return cellPos;
    }

    protected virtual void UpdateAnimation()
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
                    _animator.Play("ATTACK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("ATTACK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        } else {

        }
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }

    protected virtual void UpdateController()
    {
        switch (State) {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Skill:
                UpdateSkill();
                break;
            case CreatureState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
    }

    protected virtual void UpdateMoving()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        // 거의 도착했다면
        if (dist < _speed * Time.deltaTime) {
            transform.position = destPos;
            MoveToNextPos();
        } else {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
    }

    protected virtual void MoveToNextPos()
    {
        
    }

    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {

    }

    public virtual void OnDamaged()
    {

    }
}
