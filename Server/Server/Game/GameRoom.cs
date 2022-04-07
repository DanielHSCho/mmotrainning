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
            }
        }

        public void LeaveGame(int playerId)
        {

        }
    }
}
