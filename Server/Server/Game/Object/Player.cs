using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            // TODO : 플레이어 스피드도 데이터화
            Speed = 10.0f;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            Console.WriteLine($"TODO : damage {damage}");
        }
    }
}
