﻿using Google.Protobuf;
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

		// Account 존재 여부 체크
		// TODO : 단, 이 로직은 문제가 있음
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
