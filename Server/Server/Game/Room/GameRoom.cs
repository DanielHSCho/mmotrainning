using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public const int VisionCells = 5;
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();

        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            if(x < 0 || x >= Zones.GetLength(1)) {
                return null;
            }

            if (y < 0 || y >= Zones.GetLength(0)) {
                return null;
            }

            return Zones[y, x];
        }

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

                GetZone(player.CellPos).Players.Add(player);

                // 본인에게 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    player.Vision.Update();
                }
            } else if (type == GameObjectType.Monster) {

                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                GetZone(monster.CellPos).Monsters.Add(monster);
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

                monster.Update();

            } else if (type == GameObjectType.Projectile) {

                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                GetZone(projectile.CellPos).Projectiles.Add(projectile);
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

                GetZone(player.CellPos).Players.Remove(player);

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

                GetZone(monster.CellPos).Monsters.Remove(monster);

                Map.ApplyLeave(monster);
                monster.Room = null;

            } else if (type == GameObjectType.Projectile) {

                Projectile projectile = null;
                if (_projectiles.Remove(objectId, out projectile) == false) {
                    return;
                }

                GetZone(projectile.CellPos).Projectiles.Remove(projectile);
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

        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);
            
            foreach(Player p in zones.SelectMany(z => z.Players)) {
                p.Session.Send(packet);
            }
        }

        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int cells = GameRoom.VisionCells)
        {
            // 인접 존 검색

            HashSet<Zone> zones = new HashSet<Zone>();

            // 상 / 하 / 좌 / 우 모서리 정보
            int[] delta = new int[2] { -cells, +cells };

            // 4가지 케이스가 나올 것
            foreach(int dy in delta) {
                foreach(int dx in delta) {
                    int y = cellPos.y + dy;
                    int x = cellPos.x + dy;
                    Zone zone = GetZone(new Vector2Int(x, y));
                    if(zone == null) {
                        continue;
                    }

                    zones.Add(zone);
                }
            }

            return zones.ToList();
        }
    }
}
