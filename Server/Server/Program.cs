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

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

		static void TickRoom(GameRoom room, int tick = 100)
        {
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += ((s, e) => { room.Update(); });
			timer.AutoReset = true;
			timer.Enabled = true;

			// 타이머들을 관리할 수 있게 기억해두자
			// timer.Stop으로 수동으로 끌 수 있음
			_timers.Add(timer);
        }

		static void Main(string[] args)
		{
			// 데이터 로드
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			// Test Code
			using(AppDbContext db = new AppDbContext()) {
				PlayerDb player = db.Players.FirstOrDefault();
				if(player != null) {
					db.Items.Add(new ItemDb() {
						TemplateId = 1,
						Count = 1,
						Slot = 0,
						Owner = player
					});

					db.Items.Add(new ItemDb() {
						TemplateId = 100,
						Count = 1,
						Slot = 1,
						Owner = player
					});

					db.Items.Add(new ItemDb() {
						TemplateId = 101,
						Count = 1,
						Slot = 2,
						Owner = player
					});

					db.Items.Add(new ItemDb() {
						TemplateId = 200,
						Count = 10,
						Slot = 5,
						Owner = player
					});

					db.SaveChanges();
				}
            }

			// 1번 방을 만들자
			GameRoom room = RoomManager.Instance.Add(1);
			TickRoom(room, 50);

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			//JobTimer.Instance.Push(FlushRoom);

			while (true)
			{
				// TODO : 나중엔 이 부분 틱룸처럼 개선해야함
				// 이벤트락으로 일감이 있을때만 깨어나서 동작하도록 <
				DbTransaction.Instance.Flush();

				//JobTimer.Instance.Flush();
				//Thread.Sleep(100);
			}
		}
	}
}
