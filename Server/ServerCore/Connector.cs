using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public class Connector
	{
		Func<Session> _sessionFactory;

		public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				// 휴대폰 설정
				Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				_sessionFactory = sessionFactory;

				SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.Completed += OnConnectCompleted;
				args.RemoteEndPoint = endPoint;
				args.UserToken = socket;

				RegisterConnect(args);

				// TEST TMEP TODO REMOVE :
				// 0.1초 안에 몇백명 오는 부분 방지용 임시 코드
				// 500개 될 때 연결 되지 않는 부분 수정
				Thread.Sleep(10);
			}
		}

		void RegisterConnect(SocketAsyncEventArgs args)
		{
			Socket socket = args.UserToken as Socket;
			if (socket == null)
				return;

            try {
				bool pending = socket.ConnectAsync(args);
				if (pending == false)
					OnConnectCompleted(null, args);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
		{
            try {
				if (args.SocketError == SocketError.Success) {
					Session session = _sessionFactory.Invoke();
					session.Start(args.ConnectSocket);
					session.OnConnected(args.RemoteEndPoint);
				} else {
					Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
				}
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
	}
}
