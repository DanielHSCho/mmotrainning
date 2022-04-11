using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		Player player = clientSession.MyPlayer;
		if(player == null) {
			return;
        }

		GameRoom room = player.Room;
		if(room == null) {
			return;
        }

		// TODO : 검증

		// 일단 서버에서 좌표 이동
		PlayerInfo info = player.Info;
		info.PosInfo = movePacket.PosInfo;

		// 다른 플레이어에 브로드캐스팅
		S_Move resMovePacket = new S_Move();
		resMovePacket.PlayerId = player.Info.PlayerId;
		resMovePacket.PosInfo = movePacket.PosInfo;

		room.Broadcast(resMovePacket);
	}
}
