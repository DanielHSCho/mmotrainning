﻿using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        // Me(GameRoom) -> You(Db) -> Me(GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if(player == null || room == null) {
                return;
            }

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            // You (DB직원에 요청)
            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success) {
                        // Me (GameRoom) - callback
                        room.Push(() => Console.WriteLine($"Hp Saved({playerDb.Hp})"));
                    }
                }
            });
        }

        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null) {
                return;
            }

            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
        }

        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            using (AppDbContext db = new AppDbContext()) {
                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success) {
                    // Me (GameRoom) - callback
                    room.Push(SavePlayerStatus_Step3, playerDb.Hp);
                }
            }
        }

        public static void SavePlayerStatus_Step3(int hp)
        {
            Console.WriteLine($"Hp Saved({hp})");
        }

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if(player == null || rewardData == null || room == null) {
                return;
            }

            // TODO : 퀴즈 - 살짝 문제가 있음 (멀티스레드 관련)
            // 동시에 저장요청이 올 경우 동일한 슬롯에 저장될 수 있음
            // 따라서 빈슬롯 반환 시에 별도의 자료구조에 바로 이 빈슬롯을 기억해두고
            // 적용된 빈슬롯은 필터링 되도록 해야함
            int? slot = player.Inven.GetEmptySlot();
            if(slot == null) {
                return;
            }

            ItemDb itemDb = new ItemDb() {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success) {
                        // Success Callback
                        room.Push(() => {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);

                            // Client Noti
                            {
                                S_AddItem itemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.Info);
                                itemPacket.Items.Add(itemInfo);

                                player.Session.Send(itemPacket);
                            }
                        });
                    }
                }
            });
        }


    }
}
