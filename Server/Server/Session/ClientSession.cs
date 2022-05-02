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

		object _lock = new object();
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		long _pingpongTick = 0;
		public void Ping()
        {
			if(_pingpongTick > 0) {
				long delta = (System.Environment.TickCount64 - _pingpongTick);

				// Note : 30초가 지났다면
				if(delta > 30 * 1000) {
					Console.WriteLine("Disconnected by PingCheck");
					Disconnect();
					return;
                }

				S_Ping pingPacket = new S_Ping();
				Send(pingPacket);

				// Note : 1초 ~ 5초 사이로 보내줌
            }
        }

		public void HandlePong()
        {
			_pingpongTick = System.Environment.TickCount64;
        }

		#region Network
		// Note : 예약만 하고 보내는 것은 다른 스레드가
		public void Send(IMessage packet)
        {
			string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
			MsgId msgId = Enum.Parse<MsgId>(msgName);

			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuffer = new byte[size + 4];
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
			Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            lock (_lock) {
				_reserveQueue.Add(sendBuffer);
			}
		}

		// 실제 Network IO 보내는 부분
		public void FlushSend()
        {
			// Flush 전까지는 Send를 할 수 없으므로
			// sendList에 참조값을 전달하는 방식으로 우회
			List<ArraySegment<byte>> sendList = null;

            lock (_lock) {
				if(_reserveQueue.Count == 0) {
					return;
                }

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
            }

			Send(sendList);
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

            {
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
            }
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			// 게임로직 담당스레드에게 일감 넘김
			GameLogic.Instance.Push(() => {
				if(MyPlayer == null) {
					return;
                }

				GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
			});

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
