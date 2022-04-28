using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
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

            if(item.ItemType == ItemType.Consumable) {
                return;
            }

            // 착용 요청이라면 겹치는 부위 해제
            if (equipPacket.Equipped) {
                Item unequipItem = null;

                if(item.ItemType == ItemType.Weapon) {
                    unequipItem = player.Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon);
                } else if(item.ItemType == ItemType.Armor) {
                    ArmorType armorType = ((Armor)item).ArmorType;
                   
                    unequipItem = player.Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Armor
                        && ((Armor)i).ArmorType == armorType);
                }

                if(unequipItem != null) {
                    // 메모리 선 적용 후 DB에 알림
                    unequipItem.Equipped = false;

                    // DB Noti
                    DbTransaction.EquipItemNoti(player, unequipItem);

                    // 클라 통보
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    player.Session.Send(equipOkItem);
                }
            }

            {
                // 메모리 선 적용 후 DB에 알림
                item.Equipped = equipPacket.Equipped;

                // DB Noti
                DbTransaction.EquipItemNoti(player, item);

                // 클라 통보
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                player.Session.Send(equipOkItem);
            }
        }
    }
}
