﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameLogic
    {
        public static GameLogic Instance { get; } = new GameLogic();

        object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            // 내가 가진 방을 모두 한번씩 Update
            foreach(GameRoom room in _rooms.Values) {
                room.Update();
            }
        }

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapId);

            lock (_lock) {
                gameRoom.RoomId = _roomId;
                _rooms.Add(_roomId, gameRoom);
                _roomId++;
            }

            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            lock (_lock) {
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            lock (_lock) {
                GameRoom room = null;
                if(_rooms.TryGetValue(roomId, out room)) {
                    return room;
                }

                return null;
            }
        }
    }
}
