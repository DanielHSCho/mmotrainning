﻿using DummyClient.Session;
using ServerCore;
using System;
using System.Net;
using System.Threading;

namespace DummyClient
{
    class Program
    {
		static int DummyClientCount { get; } = 30;

		static void Main(string[] args)
        {
			Thread.Sleep(3000);

			// TODO : 나중엔 실제 붙고 싶은 아이피를 전달
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); },
				Program.DummyClientCount);

            while (true) {
				Thread.Sleep(10000);
            }
		}
    }
}
