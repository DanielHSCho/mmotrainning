using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		Managers.Object.Add(enterGamePacket.Player, myPlayer:true);
	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
		Managers.Object.Clear();
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;

		foreach(ObjectInfo obj in spawnPacket.Objects) {
			Managers.Object.Add(obj, myPlayer: false);
        }
	}

	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPakcet = packet as S_Despawn;

		foreach (int id in despawnPakcet.ObjectIds) {
			Managers.Object.Remove(id);
		}
	}

	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject go = Managers.Object.FindById(movePacket.ObjectId);
		if(go == null) {
			return;
        }

		// TODO : 추후에 거리가 정말 멀어진 경우에만 보정
		if(Managers.Object.MyPlayer.Id == movePacket.ObjectId) {
			return;
        }

		BaseController bc = go.GetComponent<BaseController>();
		if(bc == null) {
			return;
        }

		bc.PosInfo = movePacket.PosInfo;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;

		GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
		if (go == null) {
			return;
		}

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null) {
			cc.UseSkill(skillPacket.Info.SkillId);
		}
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;

		GameObject go = Managers.Object.FindById(changePacket.ObjectId);
		if (go == null) {
			return;
		}

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null) {
			cc.Hp = changePacket.Hp;
		}
	}

	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;

		GameObject go = Managers.Object.FindById(diePacket.ObjectId);
		if (go == null) {
			return;
		}

		CreatureController cc = go.GetComponent<CreatureController>();
		if (cc != null) {
			cc.Hp = 0;
			cc.OnDead();
		}
	}

	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		Debug.Log("S_ConnectedHandler");

		C_Login loginPacket = new C_Login();
		// Note : 디바이스 시스템에 따라 유니크 아이디 생성
		// 로컬에서 멀티플레이시에는 문제될 될 수 있어 예외처리 필요
		loginPacket.UniqueId = SystemInfo.deviceUniqueIdentifier;
		Managers.Network.Send(loginPacket);
	}

	public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
		S_Login loginPacket = packet as S_Login;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");

		// TODO : 로비 UI에서 캐릭터 출력 / 선택
    }

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
		S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;

    }
}
