using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Monster : GameObject
    {
        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;
        long _nextSearchTick = 0;
        long _nextMoveTick = 0;

        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            // TEMP
            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 5.0f;

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
        }

        protected virtual void UpdateIdle()
        {
            if(_nextSearchTick > Environment.TickCount64) {
                return;
            }

            _nextSearchTick = Environment.TickCount64 + 1000;

            // 1초마다 내 주변 서칭
            Player target = Room.FindPlayer(p => {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist;
            });

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
                return;
            }

            // 도망가는 거리
            int dist = (_target.CellPos - CellPos).cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist) {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            // 길 찾기
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects:false);
            
            // 갈 수 있는 위치가 없거나 너무 멀다면
            if(path.Count < 2 || path.Count > _chaseCellDist) {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            // 이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            // 다른 플레이어에게 이동 브로드캐스팅
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }

        protected virtual void UpdateSkill()
        {

        }

        protected virtual void UpdateDead()
        {

        }
    }
}
