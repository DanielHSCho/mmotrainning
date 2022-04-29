using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            // 내가 가진 방을 모두 한번씩 Update
            foreach (GameRoom room in _rooms.Values) {
                room.Update();
            }
        }

        // ALERT : Add는 일감형태로 호출할 것
        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapId);

            gameRoom.RoomId = _roomId;
            _rooms.Add(_roomId, gameRoom);
            _roomId++;

            return gameRoom;
        }

        // ALERT : Remove는 일감형태로 호출할 것
        public bool Remove(int roomId)
        {
            return _rooms.Remove(roomId);
        }

        // ALERT : Find는 일감형태로 호출할 것
        public GameRoom Find(int roomId)
        {
            GameRoom room = null;
            if (_rooms.TryGetValue(roomId, out room)) {
                return room;
            }

            return null;
        }
    }
}
