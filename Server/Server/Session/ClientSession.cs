using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;
using Server.Data;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;

		public Player MyPlayer { get; set; } 
		public int SessionId { get; set; }


		#region Network
		public void Send(IMessage packet)
        {
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = Enum.Parse<MsgId>(msgName);

			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			Send(new ArraySegment<byte>(sendBuffer));
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

            {
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
            }

			// TODO : 로비에서 캐릭터 선택

			// TODO : 실제 MMO는 여기서 온갖 로드 정보를 클라에 알려준 후
			// 클라에서 로드가 끝나면 Okay 패킷 전달해주면 그때 입장처리 해야 함
			MyPlayer = ObjectManager.Instance.Add<Player>();
            {
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				// TODO : 나중엔 이 부분이 DB에서 긁어와서 스탯 세팅
				StatInfo stat = null;
				DataManager.StatDict.TryGetValue(1, out stat);
				MyPlayer.Stat.MergeFrom(stat);

				MyPlayer.Session = this;
            }

			// TODO : 클라에서 캐릭터 선택 후 입장 요청할 때 처리해야함
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
        #endregion
    }
}
