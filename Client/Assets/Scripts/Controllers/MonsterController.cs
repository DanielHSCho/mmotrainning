using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coSkill;
    Coroutine _coPatrol;
    Coroutine _coSearch;

    [SerializeField]
    Vector3Int _destCellPos;

    [SerializeField]
    GameObject _target;

    [SerializeField]
    float _searchRange = 10.0f;

    [SerializeField]
    float _skillRange = 1.0f;

    [SerializeField]
    bool _rangedSkill = false;

    public override CreatureState State
    {
        get { return PosInfo.State; }
        set {
            if (PosInfo.State == value) {
                return;
            }

            base.State = value;
            if (_coPatrol != null) {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            if (_coSearch != null) {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        State = CreatureState.Idle;
        Dir = MoveDir.Down;

        _rangedSkill = (Random.Range(0, 2) == 0 ? true : false);

        if (_rangedSkill) {
            _skillRange = 10.0f;
        } else {
            _skillRange = 1.0f;
        }
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if(_coPatrol == null) {
            _coPatrol = StartCoroutine("CoPatrol");
        }

        if (_coSearch == null) {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    protected override void MoveToNextPos()
    {
        Vector3Int destPos = _destCellPos;
        if(_target != null) {
            destPos = _target.GetComponent<CreatureController>().CellPos;

            // 스킬 사용
            Vector3Int dir = destPos - CellPos;
            if(dir.magnitude <= _skillRange && (dir.x == 0 || dir.y == 0)) {

                Dir = GetDirFromVec(dir);
                State = CreatureState.Skill;

                if (_rangedSkill) {
                    _coSkill = StartCoroutine("CoStartShootArrow");
                } else {
                    _coSkill = StartCoroutine("CoStartPunch");
                }

                return;
            }
        }

        // 맵 매니저 Astar 이용
        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);
        if (path.Count < 2 || (_target != null && path.Count > 20)) { // 길을 못찾음 or 플레이어가 너무 멀리갔다
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];
        Vector3Int moveCellDir = nextPos - CellPos;

        Dir = GetDirFromVec(moveCellDir);

        if (Managers.Map.CanGo(nextPos) && Managers.Object.Find(nextPos) == null) {
            CellPos = nextPos;
        } else {
            State = CreatureState.Idle;
        }
    }

    public override void OnDamaged()
    {
        // TEMP
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = this.transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);

        // TODO : 나중에 오브젝트 매니저에서 한번에 처리할 수 있도록 개선해야함
        Managers.Object.Remove(Id);
        Managers.Resource.Destroy(this.gameObject);
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for(int i = 0; i < 10; i++) {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int randomPos = CellPos + new Vector3Int(xRange, yRange, 0);

            // 이동이 가능하고, 아무 오브젝트가 없다면 이동
            if(Managers.Map.CanGo(randomPos) && Managers.Object.Find(randomPos) == null){
                _destCellPos = randomPos;
                State = CreatureState.Moving;
                yield break;
            }
        }

        // 시도해봤지만 목적지를 못찾음 -> 대기상태 전환
        State = CreatureState.Idle;
    }

    IEnumerator CoSearch()
    {
        while (true) {
            yield return new WaitForSeconds(1);

            if(_target != null) {
                continue;
            }

            _target = Managers.Object.Find((go) => {
                PlayerController pc = go.GetComponent<PlayerController>();
                if(pc == null) {
                    return false;
                }

                Vector3Int dir = (pc.CellPos - CellPos);
                if(dir.magnitude > _searchRange) {
                    return false;
                }

                return true;
            });
        }
    }

    IEnumerator CoStartPunch()
    {
        // 피격 판정
        GameObject go = Managers.Object.Find(GetFrontCellPos());
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
