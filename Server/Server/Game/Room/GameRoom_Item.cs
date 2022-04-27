using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {

        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null) {
                return;
            }

            Item item = player.Inven.Get(equipPacket.ItemDbId);
            if(item == null) {
                return;
            }

            // DB 연동
        }
    }
}
