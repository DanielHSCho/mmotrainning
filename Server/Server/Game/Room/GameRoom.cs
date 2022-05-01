using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId, int zoneCells)
        {
            Map.LoadMap(mapId);

            // Zone
            ZoneCells = zoneCells;

            // 1 ~ 10칸 = 1존
            // 11 ~ 20칸 = 2존
            int countY = (Map.SizeY + zoneCells -1) / zoneCells;
            int countX = (Map.SizeX + zoneCells -1) / zoneCells;
            Zones = new Zone[countY, countX];
            for(int y = 0; y < countY; y++) {
                for(int x = 0; x < countX; x++) {
                    Zones[y, x] = new Zone(y, x);
                }
            }

            // TEMP
            Monster monster = ObjectManager.Instance.Add<Monster>();
            // TODO : 하드코딩
            monster.Init(1);
            monster.CellPos = new Vector2Int(5, 5);
            EnterGame(monster);
        }

        public void Update()
        {
            Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null) {
                return;
            }

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player) {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                player.RefreshAdditionalStat();

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

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

                    foreach (Monster m in _monsters.Values) {
                        spawnPacket.Objects.Add(m.Info);
                    }

                    foreach (Projectile p in _projectiles.Values) {
                        spawnPacket.Objects.Add(p.Info);
                    }

                    player.Session.Send(spawnPacket);
                }
            } else if (type == GameObjectType.Monster) {

                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

                monster.Update();

            } else if (type == GameObjectType.Projectile) {

                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                projectile.Update();
            }


            // 타인에게 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);

                foreach (Player otherPlayer in _players.Values) {
                    if (otherPlayer.Id != gameObject.Id) {
                        otherPlayer.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player) {
                Player player = null;
                if (_players.Remove(objectId, out player) == false) {
                    return;
                }

                player.OnLeaveGame();
                Map.ApplyLeave(player);
                player.Room = null;

                // 본인에게 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            } else if (type == GameObjectType.Monster) {

                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false) {
                    return;
                }

                Map.ApplyLeave(monster);
                monster.Room = null;

            } else if (type == GameObjectType.Projectile) {

                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false) {
                    return;
                }

                projectile.Room = null;
            }


            // 타인에게 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);

                foreach (Player otherPlayer in _players.Values) {
                    if (objectId != otherPlayer.Id) {
                        otherPlayer.Session.Send(despawnPacket);
                    }
                }
            }
        }

        // TODO
        // Note : 이 함수는 GameRoom안에서 잡큐 형태로 호출되는 함수 안에서 불려짐
        // 외부에서 이 함수를 직접 호출하는 일이 없도록 주의할 것
        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            // TODO : 무식한 방법으로 우선 찾아보자
            foreach (Player player in _players.Values) {
                if (condition.Invoke(player)) {
                    return player;
                }
            }

            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player player in _players.Values) {
                player.Session.Send(packet);
            }
        }
    }
}
