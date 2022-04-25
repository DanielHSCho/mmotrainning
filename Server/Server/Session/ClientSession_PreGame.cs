using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
			// TODO : 이런저런 보안 체크
			if(ServerState != PlayerServerState.ServerStateLogin) {
				return;
            }

			/* 
			 <해킹 이슈> - 로그인 관련
				- 동시에 다른 사람이 같은 UniqueId를 보낸다면?
					- DB 저장이 안될 경우를 체크 (SaveChanges)
				- 악의적으로 여러번 보낸다면 (1초에 100번씩 보낸다면)
				- 생뚱맞은 타이밍에 이 패킷을 보낸다면
					- 로그인했는지 등의 상태 관리
			*/

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
					db.SaveChanges();

					// AccountDbId 기억
					AccountDbId = findAccount.AccountDbId;

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
				MyPlayer.Info.Name = playerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
				MyPlayer.Session = this;
			}

			ServerState = PlayerServerState.ServerStateGame;

			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
			// TODO : 이런저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLobby) {
				return;
			}

			using(AppDbContext db = new AppDbContext()) {

				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name)
					.FirstOrDefault();

				if(findPlayer != null) {
					// 이름이 겹친다
					Send(new S_CreatePlayer());
                } else {
					// 새 플레이어 생성
					// 1레벨 스탯 정보 추출
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
					// TODO : ExceptionHandling
					// => 찰나의 순간에 동일한 플레이어 이름이 요청될 경우
					db.SaveChanges();

					// 메모리에 추가
					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo() {
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
