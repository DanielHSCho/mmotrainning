using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
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
        public Inventory Inven { get; private set; } = new Inventory();

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            // TODO : 랭킹, PK포인트는 이쪽에
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            DbTransaction.SavePlayerStatus_Step1(this, Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {

            Item item = this.Inven.Get(equipPacket.ItemDbId);
            if (item == null) {
                return;
            }

            if (item.ItemType == ItemType.Consumable) {
                return;
            }

            // 착용 요청이라면 겹치는 부위 해제
            if (equipPacket.Equipped) {
                Item unequipItem = null;

                if (item.ItemType == ItemType.Weapon) {
                    unequipItem = this.Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon);
                } else if (item.ItemType == ItemType.Armor) {
                    ArmorType armorType = ((Armor)item).ArmorType;

                    unequipItem = this.Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Armor
                        && ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null) {
                    // 메모리 선 적용 후 DB에 알림
                    unequipItem.Equipped = false;

                    // DB Noti
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    // 클라 통보
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    this.Session.Send(equipOkItem);
                }
            }

            {
                // 메모리 선 적용 후 DB에 알림
                item.Equipped = equipPacket.Equipped;

                // DB Noti
                DbTransaction.EquipItemNoti(this, item);

                // 클라 통보
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                this.Session.Send(equipOkItem);
            }

            RefreshAdditionalStat();
        }

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach(Item item in Inven.Items.Values) {
                if(item.Equipped == false) {
                    continue;
                }

                switch (item.ItemType) {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }
    }
}
