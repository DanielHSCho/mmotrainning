using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if(player == null) {
			return;
        }

		GameRoom room = player.Room;
		if(room == null) {
			return;
        }

		room.Push(room.HandleMove, player, movePacket);
	}

	public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null) {
			return;
		}

		GameRoom room = player.Room;
		if (room == null) {
			return;
		}

		room.Push(room.HandleSkill, player, skillPacket);
	}

	public static void C_LoginHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = packet as C_Login;
		ClientSession clientSession = session as ClientSession;

		// TODO : 콘솔도 나중엔 체계화 해야함
		Console.WriteLine($"UniqueId({loginPacket.UniqueId})");

		// TODO : 이런저런 보안 체크

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
				.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

			if (findAccount != null) {
				S_Login loginOk = new S_Login() { LoginOk = 1 };
				clientSession.Send(loginOk);
			} else {
				AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
				db.Accounts.Add(newAccount);
				db.SaveChanges();

				S_Login loginOk = new S_Login() { LoginOk = 1 };
				clientSession.Send(loginOk);
			}
		}
	}
}
