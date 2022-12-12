using DummyClient.Session;
using ServerCore;
using System;
using System.Net;
using System.Threading;

namespace DummyClient
{
    class Program
    {
		static int DummyClientCount { get; } = 500;

		static void Main(string[] args)
        {
			// 서버 시작까지 잠시 기다림
			Thread.Sleep(10000);

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); },
				Program.DummyClientCount);

			while(true)
            {
				Thread.Sleep(10000);
            }
		}
    }
}
