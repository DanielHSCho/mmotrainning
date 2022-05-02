using ServerCore;
using System;
using System.Net;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
			// TODO : 나중엔 실제 붙고 싶은 아이피를 전달
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return _session; },
				1);
		}
    }
}
