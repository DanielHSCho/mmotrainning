using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
        {
            while (true) {
				GameLogic.Instance.Update();
				Thread.Sleep(0);
            }
        }

		static void DbTask()
        {
			while (true) {
				// TODO : 나중엔 이 부분 틱룸처럼 개선해야함
				// 이벤트락으로 일감이 있을때만 깨어나서 동작하도록 <
				DbTransaction.Instance.Flush();

				// While이 도는게 부담스러우므로
				// Thread.Sleep(0)으로 소유권을 살짝 넘겨줌
				Thread.Sleep(0);
			}
		}

		// Note : 얘는 여러 스레드를 둬서 분배할 수 있음
		static void NetworkTask()
        {
            while (true) {
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
				foreach(ClientSession session in sessions) {
					session.FlushSend();
                }

				Thread.Sleep(0);
            }
        }

		// TODO : config파일 같은 것으로 관리해야함
		public static string Name { get; } = "다니엘";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		static void Main(string[] args)
		{
			// 데이터 로드
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			// 게임 동작 전 데이터 로드 후 생성
			GameLogic.Instance.Push(() => { GameRoom room = GameLogic.Instance.Add(1); });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			// TODO : Config로 빼야한다
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            // DbTask
            {
				Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
            }

			// NetworkTask
			{
				// Note : 롱러닝 Task와 스레드는 큰차이 없음
				// Task networkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
				// networkTask.Start();

				Thread t = new Thread(NetworkTask);
				t.Name = "NetworkSend";
				t.Start();
			}

			// GameLogic
			Thread.CurrentThread.Name = "GameLogic";
			GameLogicTask();
		}
	}
}
