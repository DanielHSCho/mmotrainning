﻿using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
			if(ServerState != PlayerServerState.ServerStateLogin) {
				return;
            }

			LobbyPlayers.Clear();

			using (AppDbContext db = new AppDbContext()) {
				AccountDb findAccount = db.Accounts
					.Include(a=>a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null) {
					// AccountDbId 기억
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach(PlayerDb playerDb in findAccount.Players) {
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo() {
							PlayerDbId = playerDb.PlayerDbId,
							Name = playerDb.PlayerName,
							StatInfo = new StatInfo() {
								Level = playerDb.Level,
								Hp = playerDb.Hp,
								MaxHp = playerDb.MaxHp,
								Attack = playerDb.Attack,
								Speed = playerDb.Speed,
								TotalExp = playerDb.TotalExp
                            }
						};

						// 메모리에도 들고
						LobbyPlayers.Add(lobbyPlayer);

						// 패킷에도 넣어준다
						loginOk.Players.Add(lobbyPlayer);
                    }

					Send(loginOk);
					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				} else {
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					bool success = db.SaveChangesEx();
					if(success == false) {
						return;
                    }

					// AccountDbId 기억
					AccountDbId = newAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);

					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
			if(ServerState != PlayerServerState.ServerStateLobby) {
				return;
            }

			LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
			if(playerInfo == null) {
				return;
            }

			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
				MyPlayer.Info.Name = playerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
				MyPlayer.Session = this;

				S_ItemList itemListPacket = new S_ItemList();

				// 아이템 목록 로드
				using(AppDbContext db = new AppDbContext()) {
					List<ItemDb> items = db.Items.Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
						.ToList();

					foreach(ItemDb itemDb in items) {
						Item item = Item.MakeItem(itemDb);
						if(item != null) {
							MyPlayer.Inven.Add(item);

							ItemInfo info = new ItemInfo();
							info.MergeFrom(item.Info);
							itemListPacket.Items.Add(info);
						}
					}
                }

				Send(itemListPacket);
			}

			ServerState = PlayerServerState.ServerStateGame;

			// 게임로직 담당스레드에게 일감 넘김
			GameLogic.Instance.Push(()=> {
				GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.EnterGame, MyPlayer, true);
			});
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
			if (ServerState != PlayerServerState.ServerStateLobby) {
				return;
			}

			using(AppDbContext db = new AppDbContext()) {

				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name)
					.FirstOrDefault();

				if(findPlayer != null) {
					Send(new S_CreatePlayer());
                } else {
					// 새 플레이어 생성
					StatInfo stat = null;
					DataManager.StatDict.TryGetValue(1, out stat);

					// DB에 플레이어 추가
					PlayerDb newPlayerDb = new PlayerDb() {
						PlayerName = createPacket.Name,
						Level = stat.Level,
						Hp = stat.Hp,
						MaxHp = stat.MaxHp,
						Attack = stat.Attack,
						Speed = stat.Speed,
						TotalExp = 0,
						AccountDbId = AccountDbId
					};

					db.Players.Add(newPlayerDb);
					bool success = db.SaveChangesEx();
					if (success == false) {
						return;
					}

					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo() {
						PlayerDbId = newPlayerDb.PlayerDbId,
						Name = createPacket.Name,
						StatInfo = new StatInfo() {
							Level = stat.Level,
							Hp = stat.Hp,
							MaxHp = stat.MaxHp,
							Attack = stat.Attack,
							Speed = stat.Speed,
							TotalExp = 0
						}
					};

					LobbyPlayers.Add(lobbyPlayer);

					// 클라에 캐릭터 생성 알림
					S_CreatePlayer newPlayer = new S_CreatePlayer() {
						Player = new LobbyPlayerInfo()};

					newPlayer.Player.MergeFrom(lobbyPlayer);

					Send(newPlayer);
				}
            }
		}
    }
}
