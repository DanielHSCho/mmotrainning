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
		// TODO : 추후 로그인창으로 수정 필요
		string path = Application.dataPath;
		loginPacket.UniqueId = path.GetHashCode().ToString();
		Managers.Network.Send(loginPacket);
	}

	public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
		S_Login loginPacket = packet as S_Login;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");

		// TODO : 로비 UI에서 캐릭터 출력 / 선택
		if(loginPacket.Players == null || loginPacket.Players.Count == 0) {
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";

			Managers.Network.Send(createPacket);
        } else {
			// TODO : 첫번째 캐릭터로 로그인한다고 가정
			LobbyPlayerInfo info = loginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;

			Managers.Network.Send(enterGamePacket);
        }
    }

	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
		S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;

		// 어떤 이유에서 캐릭터 생성 실패 시 재요청
		if(createOkPacket.Player == null) {
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";

			Managers.Network.Send(createPacket);
        } else {
			// TODO : 첫번째 캐릭터로 로그인한다고 가정
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createOkPacket.Player.Name;

			Managers.Network.Send(enterGamePacket);
		}
    }

	public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
		S_ItemList itemList = (S_ItemList)packet;

		//UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		//if(gameSceneUI == null) {
		//	return;
		//      }
		//UI_Inventory invenUI = gameSceneUI.InvenUI;

		Managers.Inven.Clear();

		// 메모리에 아이템 정보 적용
		foreach(ItemInfo itemInfo in itemList.Items) {
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
        }

		//// UI에 표시
		//invenUI.gameObject.SetActive(true);
		//invenUI.RefreshUI();

		if (Managers.Object.MyPlayer != null) {
			Managers.Object.MyPlayer.RefreshAdditionalStat();
		}
	}

	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemList = (S_AddItem)packet;

		foreach (ItemInfo itemInfo in itemList.Items) {
			Item item = Item.MakeItem(itemInfo);
			Managers.Inven.Add(item);
		}

		Debug.Log("아이템 획득");

		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		if (gameSceneUI != null) {
			gameSceneUI.InvenUI.RefreshUI();
			gameSceneUI.StatUI.RefreshUI();
		}

		if (Managers.Object.MyPlayer != null) {
			Managers.Object.MyPlayer.RefreshAdditionalStat();
		}
	}

	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemOk = (S_EquipItem)packet;

		Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
		if(item == null) {
			return;
        }

		item.Equipped = equipItemOk.Equipped;
		Debug.Log("아이템 착용 변경");


		UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
		if (gameSceneUI != null) {
			gameSceneUI.InvenUI.RefreshUI();
			gameSceneUI.StatUI.RefreshUI();
		}

		if(Managers.Object.MyPlayer != null) {
			Managers.Object.MyPlayer.RefreshAdditionalStat();
		}
	}

	public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
	{
		S_ChangeStat itemList = (S_ChangeStat)packet;

		// TODO
	}

	public static void S_PingHandler(PacketSession session, IMessage packet)
	{
		C_Pong pongPacket = new C_Pong();
		Debug.Log("[Server] Ping Check");
		Managers.Network.Send(pongPacket);
	}
}
