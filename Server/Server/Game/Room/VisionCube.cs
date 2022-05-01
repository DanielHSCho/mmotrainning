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
    }
}
