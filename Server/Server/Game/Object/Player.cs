using Google.Protobuf.Protocol;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            // TODO : 랭킹, PK포인트는 이쪽에

            // DB 연동?
            using (AppDbContext db = new AppDbContext()) {
                PlayerDb playerDb = db.Players.Find(PlayerDbId);
                playerDb.Hp = Stat.Hp;
                db.SaveChanges();
            }
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }
    }
}
