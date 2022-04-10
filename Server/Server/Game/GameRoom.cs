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

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if(newPlayer == null) {
                return;
            }

            lock (_lock) {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                // 본인에게 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach(Player player in _players) {
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
                    foreach(Player otherPlayer in _players) {
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
                Player player = _players.Find(p => p.Info.PlayerId == playerId);
                if(player == null) {
                    return;
                }

                _players.Remove(player);
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

                    foreach(Player otherPlayer in _players) {
                        if(player != otherPlayer) {
                            otherPlayer.Session.Send(despawnPacket);
                        }
                    }
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock) {
                foreach(Player player in _players) {
                    player.Session.Send(packet);
                }
            }
        }
    }
}
