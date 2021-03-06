using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        long _coolTick = 0;
        int _skillRange = 1;

        long _nextSearchTick = 0;
        long _nextMoveTick = 0;

        IJob _job;

        public Monster()
        {
            ObjectType = GameObjectType.Monster;
        }

        public void Init(int templateId)
        {
            TemplateId = templateId;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = monsterData.stat.MaxHp;
            State = CreatureState.Idle;
        }

        // FSM 방식
        public override void Update()
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

            // 몬스터는 5프레임마다 (0.2초마다) 갱신
            if(Room != null) {
                _job = Room.PushAfter(200, Update);
            }
        }

        protected virtual void UpdateIdle()
        {
            if(_nextSearchTick > Environment.TickCount64) {
                return;
            }

            _nextSearchTick = Environment.TickCount64 + 1000;

            // 1초마다 내 주변 서칭
            Player target = Room.FindClosestPlayer(CellPos, _searchCellDist);

            if(target == null) {
                return;
            }

            _target = target;
            State = CreatureState.Moving;
        }

        protected virtual void UpdateMoving()
        {
            if(_nextMoveTick > Environment.TickCount64) {
                return;
            }

            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            // 내가 공격하려는 대상이 로그아웃을하거나 다른 방으로 이동시
            if(_target == null || _target.Room != Room) {
                _target = null;
                State = CreatureState.Idle;
                BroardcastMove();
                return;
            }

            // 도망가는 거리
            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist) {
                _target = null;
                State = CreatureState.Idle;
                BroardcastMove();
                return;
            }

            // 길 찾기
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects:true);
            
            // 갈 수 있는 위치가 없거나 너무 멀다면
            if(path.Count < 2 || path.Count > _chaseCellDist) {
                _target = null;
                State = CreatureState.Idle;
                BroardcastMove();
                return;
            }

            // 스킬 사용 여부 체크 / 대각선 스킬 사용 방지
            if(dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            // 이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);
            BroardcastMove();
        }

        void BroardcastMove()
        {
            // 다른 플레이어에게 이동 브로드캐스팅
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(CellPos, movePacket);
        }

        protected virtual void UpdateSkill()
        {
            if(_coolTick == 0) {
                // 유효 타겟 여부
                if(_target == null || _target.Room != Room || _target.Hp == 0) {
                    _target = null;
                    State = CreatureState.Moving;
                    BroardcastMove();
                    return;
                }

                // 스킬 사용 가능 여부
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));

                if(canUseSkill == false) {
                    // 타겟 Null 처리는 제외 - 다시 판단이 가능하도록
                    State = CreatureState.Moving;
                    BroardcastMove();
                    return;
                }

                // 타겟팅 방향 주시
                MoveDir lookDir = GetDirFromVec(dir);
                if(Dir != lookDir) {
                    Dir = lookDir;
                    BroardcastMove();
                }


                // TODO : 몬스터 스킬 데이터 시트로 연동해야함
                Skill skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData);

                // 데미지 판정
                _target.OnDamaged(this, skillData.damage + TotalAttack);

                // 스킬 사용 브로드 캐스팅
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            // 다음 스킬 사용 가능 시간인지 체크
            if(_coolTick > Environment.TickCount64) {
                return;
            }

            _coolTick = 0;
        }

        protected virtual void UpdateDead()
        {
        }

        public override void OnDead(GameObject attacker)
        {
            if (_job != null) {
                _job.Cancel = true;
                _job = null;
            }

            base.OnDead(attacker);

            GameObject owner = attacker.GetOwner();

            if(owner.ObjectType == GameObjectType.Player) {
                RewardData rewardData = GetRandomReward();
                if(rewardData != null) {
                    Player player = (Player)owner;
                    DbTransaction.RewardPlayer(player, rewardData, Room);
                }
            }
        }

        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

            // 100분율 (0 ~ 100까지)
            int rand = new Random().Next(0, 101);

            int sum = 0;
            foreach(RewardData rewardData in monsterData.rewards) {
                sum += rewardData.probability;
                if(rand <= sum) {
                    return rewardData;
                }
            }

            return null;
        }
    }
}
