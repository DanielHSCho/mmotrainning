using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game
{
    public class VisionCube
    {
        // Note : 1초에 한번씩 체크하면서 실시간 스폰/디스폰 관리
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        public HashSet<GameObject> GatherObjects()
        {
            // 잡 방식으로 실행될 수 있으므로
            if (Owner == null || Owner.Room == null) {
                return null;
            }

            HashSet<GameObject> objects = new HashSet<GameObject>();
            Vector2Int cellPos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZones(cellPos);

            foreach(Zone zone in zones) {
                foreach(Player player in zone.Players) {
                    int dx = player.CellPos.x - cellPos.x;
                    int dy = player.CellPos.y - cellPos.y;
                    if(Math.Abs(dx) > GameRoom.VisionCells) {
                        continue;
                    }

                    if (Math.Abs(dy) > GameRoom.VisionCells) {
                        continue;
                    }

                    objects.Add(player);
                }

                foreach (Monster monster in zone.Monsters) {
                    int dx = monster.CellPos.x - cellPos.x;
                    int dy = monster.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells) {
                        continue;
                    }

                    if (Math.Abs(dy) > GameRoom.VisionCells) {
                        continue;
                    }

                    objects.Add(monster);
                }

                foreach (Projectile projectile in zone.Projectiles) {
                    int dx = projectile.CellPos.x - cellPos.x;
                    int dy = projectile.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells) {
                        continue;
                    }

                    if (Math.Abs(dy) > GameRoom.VisionCells) {
                        continue;
                    }

                    objects.Add(projectile);
                }
            }

            return objects;
        }

        public void Update()
        {
            if(Owner == null || Owner.Room == null) {
                return;
            }

            // 존단위로 쪼개서 확인해 부하를 줄임

            // 업데이트 시점에서 오브젝트를 긁어온다
            // 시야각 기준 최대 100개 내외
            HashSet<GameObject> currntObjects = GatherObjects();

            // 기존과 비교해 스폰/디스폰 처리
            List<GameObject> added = currntObjects.Except(PreviousObjects).ToList();
            if(added.Count > 0) {
                S_Spawn spawnPacket = new S_Spawn();

                foreach(GameObject gameObject in added) {
                    // 참조값을 전달하도록 함
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(gameObject.Info);
                    spawnPacket.Objects.Add(info);
                }

                Owner.Session.Send(spawnPacket);
            }

            List<GameObject> removed = PreviousObjects.Except(currntObjects).ToList();
            if (removed.Count > 0) {
                S_Despawn despawnPacket = new S_Despawn();

                foreach (GameObject gameObject in removed) {
                   
                    despawnPacket.ObjectIds.Add(gameObject.Id);
                }

                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currntObjects;
            Owner.Room.PushAfter(500, Update);
        }
    }
}
