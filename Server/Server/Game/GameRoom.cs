using Google.Protobuf;
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
        Map _map = new Map();

        public void Init(int mapId)
        {
            _map.LoadMap(mapId);
        }

        public void EnterGame(Player newPlayer)
        {
            if(newPlayer == null) {
                return;
            }

            lock (_lock) {
                _players.Add(newPlayer.Info.PlayerId, newPlayer);
                newPlayer.Room = this;

                // 본인에게 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach(Player player in _players.Values) {
                        if(newPlayer != player) {
                            spawnPacket.Players.Add(player.Info);
                        }
                        newPlayer.Session.Send(spawnPacket);
                    }
                }

                // 타인에게 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach(Player otherPlayer in _players.Values) {
                        if(newPlayer != otherPlayer) {
                            otherPlayer.Session.Send(spawnPacket);
                        }
                    }
                }
            }
        }

        public void LeaveGame(int playerId)
        {
            lock (_lock) {
                // TODO : 딕셔너리로 개선할 것
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
                    despawnPacket.PlayerIds.Add(player.Info.PlayerId);

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

                // 일단 서버에서 좌표 이동
                PlayerInfo info = player.Info;
                info.PosInfo = movePacket.PosInfo;

                // 다른 플레이어에 브로드캐스팅
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.Info.PlayerId;
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
                PlayerInfo info = player.Info;

                // TODO : 스킬 사용 가능 여부 체크
                if(info.PosInfo.State != CreatureState.Idle) {
                    return;
                }

                info.PosInfo.State = CreatureState.Skill;

                // 스킬 정보
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerId = info.PlayerId;
                // TODO : 추후 데이터 시트로 구분되어야 함 (xml / json)
                skill.Info.SkillId = 1;
                Broadcast(skill);

                // 데미지 판정 (평타라면 즉시 데미지를 줄 수 있으므로)
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
