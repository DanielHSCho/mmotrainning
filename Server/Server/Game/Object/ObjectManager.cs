using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        // 비트 플래그로 쪼개서 사용
        // [UNUSED(1)] [TYPE(7)] [ID(24)]
        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameobject = new T();

            lock (_lock) {
                gameobject.Id = GenerateId(gameobject.ObjectType);

                // 플레이어 타입인 경우 별도 처리
                if(gameobject.ObjectType == GameObjectType.Player) {
                    _players.Add(gameobject.Id, gameobject as Player);
                }
            }

            return gameobject;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock) {
                // Type은 24비트만큼 밀어준다
                return ((int)type << 24) | (_counter++);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            // 24비트만큼 오른쪽 이동
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock) {
                if(objectType == GameObjectType.Player) {
                    return _players.Remove(objectId);
                }
            }

            return false;
        }

        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock) {
                if(objectType == GameObjectType.Player) {
                    Player player = null;
                    if (_players.TryGetValue(objectId, out player)) {
                        return player;
                    }
                }

                return null;
            }
        }
    }
}
