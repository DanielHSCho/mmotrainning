using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player
    {
        public GameRoom Room { get; set; }
        public ClientSession Session { get; set; }
    }
}
