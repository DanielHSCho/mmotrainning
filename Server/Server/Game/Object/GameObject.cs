﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }

        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
        }

        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set { PosInfo.MoveDir = value; }
        }

        public CreatureState State
        {
            get { return PosInfo.State; }
            set { PosInfo.State = value; }
        }

        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }

        public virtual void Update()
        {
        }

        public Vector2Int CellPos
        {
            get {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }

            set {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPos(MoveDir dir)
        {
            Vector2Int cellPos = CellPos;

            switch (dir) {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }

            return cellPos;
        }

        public static MoveDir GetDirFromVec(Vector2Int dir)
        {
            if (dir.x > 0) {
                return MoveDir.Right;
            } else if (dir.x < 0) {
                return MoveDir.Left;
            } else if (dir.y > 0) {
                return MoveDir.Up;
            } else {
                return MoveDir.Down;
            }
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(changePacket);

            if(Stat.Hp <= 0) {
                Stat.Hp = 0;
                OnDead(attacker);
            }
        }

        public virtual void OnDead(GameObject attacker)
        {
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(diePacket);

            GameRoom room = Room;
            room.Push(room.LeaveGame, Id);
            
            // 재입장을 위한 초기화
            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;
            // TODO : 마을좌표나 리스폰 좌표 데이터가 있다면 연동
            PosInfo.PosX = 0;
            PosInfo.PosY = 0;

            // 플레이어가 있던 Room이 Null이 되도 미리 받아뒀던 방으로 재입장
            room.Push(room.EnterGame, this);
        }
    }
}
