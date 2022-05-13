using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Game
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        public HashSet<GameObject> GatherObjects()
        {
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
            HashSet<GameObject> currntObjects = GatherObjects();

            // 기존과 비교해 스폰/디스폰 처리
            List<GameObject> added = currntObjects.Except(PreviousObjects).ToList();
            if(added.Count > 0) {
                S_Spawn spawnPacket = new S_Spawn();

                foreach(GameObject gameObject in added) {
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
