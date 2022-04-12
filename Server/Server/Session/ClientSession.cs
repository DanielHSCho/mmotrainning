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

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; } 
		public int SessionId { get; set; }

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

			// TODO : 실제 MMO는 여기서 온갖 로드 정보를 클라에 알려준 후
			// 클라에서 로드가 끝나면 Okay 패킷 전달해주면 그때 입장처리 해야 함
			MyPlayer = PlayerManager.Instance.Add();
            {
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				MyPlayer.Session = this;
            }

			RoomManager.Instance.Find(1).EnterGame(MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
