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

			using (AppDbContext db = new AppDbContext()) {
				AccountDb findAccount = db.Accounts
					.Include(a=>a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null) {
					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
				} else {
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					db.SaveChanges();

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
				}
			}
		}
    }
}
