using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    public Grid _grid;
    public float _speed = 5.0f;

    Vector3Int _cellPos = Vector3Int.zero;
    bool _isMoving = false;
    Animator _animator;

    MoveDir _dir = MoveDir.Down;
    public MoveDir Dir
    {
        get { return _dir; }
        set {
            if(_dir == value) {
                return;
            }

            switch (value) {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case MoveDir.None:
                    if(_dir == MoveDir.Up) {
                        _animator.Play("IDLE_BACK");
                        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }else if(_dir == MoveDir.Down) {
                        _animator.Play("IDLE_FRONT");
                        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    } else if (_dir == MoveDir.Left) {
                        _animator.Play("IDLE_RIGHT");
                        transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    } else { 
                        _animator.Play("IDLE_RIGHT");
                        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }
                    break;
            }

            _dir = value;
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }

    void Update()
    {
        GetDirInput();
        UpdatePosition();
        UpdateIsMoving();
    }

    void UpdatePosition()
    {
        if(_isMoving == false) {
            return;
        }

        Vector3 destPos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        // 거의 도착했다면
        if (dist < _speed * Time.deltaTime) { 
            transform.position = destPos;
            _isMoving = false;
        } else {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _isMoving = true;
        }
    }

    void UpdateIsMoving()
    {
        if (_isMoving == false) {
            switch (_dir) {
                case MoveDir.Up:
                    _cellPos += Vector3Int.up;
                    _isMoving = true;
                    break;
                case MoveDir.Down:
                    _cellPos += Vector3Int.down;
                    _isMoving = true;
                    break;
                case MoveDir.Left:
                    _cellPos += Vector3Int.left;
                    _isMoving = true;
                    break;
                case MoveDir.Right:
                    _cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
            }
        }
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
