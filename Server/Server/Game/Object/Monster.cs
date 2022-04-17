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
        long _nextSearchTick = 0;

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

        }
        protected virtual void UpdateSkill()
        {

        }

        protected virtual void UpdateDead()
        {

        }
    }
}
