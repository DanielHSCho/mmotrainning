﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        Map _map = new Map();

        public void Init(int mapId)
        {
            _map.LoadMap(mapId);
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null) {
                return;
            }

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock (_lock) {
                if (type == GameObjectType.Player) {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    player.Room = this;

                    // 본인에게 정보 전송
                    {
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.Info;
                        player.Session.Send(enterPacket);

                        S_Spawn spawnPacket = new S_Spawn();
                        foreach (Player p in _players.Values) {
                            if (player != p) {
                                spawnPacket.Objects.Add(p.Info);
                            }
                        }
                        player.Session.Send(spawnPacket);
                    }
                } else if(type == GameObjectType.Monster) {

                    Monster monster = gameObject as Monster;
                    _monsters.Add(gameObject.Id, monster);
                    monster.Room = this;

                } else if(type == GameObjectType.Projectile) {

                    Projectile projectile = gameObject as Projectile;
                    _projectiles.Add(gameObject.Id, projectile);
                    projectile.Room = this; 

                }


                // 타인에게 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.Info);

                    foreach(Player otherPlayer in _players.Values) {
                        if(otherPlayer.Id != gameObject.Id) {
                            otherPlayer.Session.Send(spawnPacket);
                        }
                    }
                }
            }
        }

        public void LeaveGame(int playerId)
        {
            lock (_lock) {
                Player player = null;
                if(_players.Remove(playerId, out player) == false) {
                    return;
                }

                player.Room = null;


                // 본인에게 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                // 타인에게 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.ObjectId);

                    foreach(Player otherPlayer in _players.Values) {
                        if(player != otherPlayer) {
                            otherPlayer.Session.Send(despawnPacket);
                        }
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if(player == null) {
                return;
            }

            lock (_lock) {
                // TODO : 검증
                PositionInfo movePosInfo = movePacket.PosInfo;
                ObjectInfo info = player.Info;

                // 다른 좌표로 이동할 경우, 갈 수 있는지 체크
                if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY) {
                    if(!_map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY))) {
                        return;
                    }
                }

                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                _map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                // 다른 플레이어에 브로드캐스팅
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.Info.ObjectId;
                resMovePacket.PosInfo = movePacket.PosInfo;

                Broadcast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if(player == null) {
                return;
            }

            lock (_lock) {
                ObjectInfo info = player.Info;

                if(info.PosInfo.State != CreatureState.Idle) {
                    return;
                }

                // TODO : 스킬 사용 가능 여부 체크

                info.PosInfo.State = CreatureState.Skill;
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerId = info.ObjectId;
                // TODO : 추후 데이터 시트로 구분되어야 함 (xml / json)
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);

                if (skillPacket.Info.SkillId == 1) {
                    // 데미지 판정 (평타라면 즉시 데미지를 줄 수 있으므로)
                    Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                    Player target = _map.Find(skillPos);
                    if (target != null) {
                        Console.WriteLine("Hit Player!");
                    }
                } else if(skillPacket.Info.SkillId == 2) {

                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                    if(arrow == null) {
                        return;
                    }

                    arrow.Owner = player;
                    arrow.PosInfo.State = CreatureState.Moving;
                    arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                    arrow.PosInfo.PosX = player.PosInfo.PosX;
                    arrow.PosInfo.PosY = player.PosInfo.PosY;
                    EnterGame(arrow);
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock) {
                foreach(Player player in _players.Values) {
                    player.Session.Send(packet);
                }
            }
        }
    }
}
