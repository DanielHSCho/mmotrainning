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
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            // GameLogicTask
            {
				Task gameLogicTask = new Task(GameLogicTask, TaskCreationOptions.LongRunning);
				gameLogicTask.Start();
            }

			// DBTask -  DbTask는 메인 스레드에서 처리하도록함
			DbTask();
		}
	}
}
