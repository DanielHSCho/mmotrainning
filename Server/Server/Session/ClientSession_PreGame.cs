using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
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

        }

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {

        }
    }
}
