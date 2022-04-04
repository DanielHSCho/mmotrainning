using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    public float _speed = 5.0f;

    public Grid _grid;
    Vector3Int _cellPos = Vector3Int.zero;
    MoveDir _dir = MoveDir.None;
    bool _isMoving = false;

    void Start()
    {
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
            _dir = MoveDir.Up;
        } else if (Input.GetKey(KeyCode.S)) {
            //transform.position += Vector3.down * Time.deltaTime * _speed;
            _dir = MoveDir.Down;
        } else if (Input.GetKey(KeyCode.A)) {
            //transform.position += Vector3.left * Time.deltaTime * _speed;
            _dir = MoveDir.Left;
        } else if (Input.GetKey(KeyCode.D)) {
            //transform.position += Vector3.right * Time.deltaTime * _speed;
            _dir = MoveDir.Right;
        } else {
            _dir = MoveDir.None;
        }
    }
}
