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
				DbTransaction.Instance.Flush();
				// While이 도는게 부담스러우므로
				// Thread.Sleep(0)으로 소유권을 살짝 넘겨줌
				Thread.Sleep(0);
			}
		}

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

		static void StartServerInfoTask()
        {
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) => {
				using(SharedDbContext shared = new SharedDbContext()) {
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if(serverDb != null) {
						serverDb.IpAdress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
                    } else {
						serverDb = new ServerDb() {
							Name = Program.Name,
							IpAdress = Program.IpAddress,
							Port = Program.Port,
							BusyScore = SessionManager.Instance.GetBusyScore()
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
                    }
                }
			});

			t.Interval = 10 * 1000;
			t.Start();
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
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			StartServerInfoTask();

			// DbTask
			{
				Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
            }

			// NetworkTask
			{
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
