using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coPatrol;

    protected override void Init()
    {
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if(_coPatrol == null) {
            _coPatrol = StartCoroutine("CoPatrol");
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
        Managers.Object.Remove(this.gameObject);
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
                
            }
        }
    }
}
