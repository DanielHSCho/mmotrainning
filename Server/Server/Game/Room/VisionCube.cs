using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class VisionCube
    {
        // Note : 1초에 한번씩 체크하면서 실시간 스폰/디스폰 관리
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; }

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
    }
}
