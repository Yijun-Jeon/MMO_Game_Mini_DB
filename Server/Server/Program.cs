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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Server.Data;
using Server.Data.DB;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

namespace Server
{
	class Program
	{
		// 1. GameRoom 방식의 간단한 동기화 <- OK
		// 2. 더 넓은 영역 관리
		// 3. 심리스 MMO

		// 1. Recv (N개)     서빙
		// 2. GameLogic (1)  요리사
		// 3. Send (1개)     서빙
		// 4. DB (1)         결제/장부

		static Listener _listener = new Listener();

		static void GameLogicTask()
        {
			while(true)
            {
				GameLogic.Instance.Update();
				Thread.Sleep(0);
            }
        }

		static void DbTask()
        {
			while (true)
			{
				// 일단 무식하게 무한루프로 Flush
				DbTransaction.Instance.Flush();
				// 실행권을 잠시 넘겨줘서 CPU 낭비 방지
				Thread.Sleep(0);
			}
		}

		static void NetworkTask()
        {
			while(true)
            {
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();

				foreach (ClientSession session in sessions)
				{
					session.FlushSend();
				}
				Thread.Sleep(0);
            }
        }

		// DB 새로 생성 - 시간이 좀 걸림
		static void InitializeDB(bool forceReset = true)
		{
			using AppDbContext db = new AppDbContext();
			{
				// 강제 Reset이 아니고
				// 만약 DB가 이미 만들어져 있다면
				if (!forceReset && (db.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
					return;

				db.Database.EnsureDeleted(); // 처음 초기화
				db.Database.EnsureCreated(); // 모델링과 일치하게 생성

				Console.WriteLine("DB Initialized");
			}
			//db.Dispose(); // 사용 후 처리
		}

		// 서버의 정보를 갱신
		public static string Name { get; } = "데포르쥬";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		// 주기적으로 서버 정보 갱신
		static void StartServerInfoTask()
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
			{
				// 공유 DB에 자신의 서버 정보 갱신
				using(SharedDbContext shared = new SharedDbContext())
				{
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if(serverDb != null)
					{
						serverDb.IpAddress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
					}
					else
					{
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							IpAddress = Program.IpAddress,
							Port = Program.Port,
							BusyScore = SessionManager.Instance.GetBusyScore(),
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
					}
				}
			});
			// 10 초마다 실행
			t.Interval = 10 * 1000;
			t.Start();
		}


		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			// DB
			InitializeDB(forceReset: false);

			GameLogic.Instance.Push(() =>
			{
				// 게임룸 생성
				GameLogic.Instance.Add(1);
			});

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			// SharedDb
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
				t.Name = "Network Send";
				t.Start();
			}

			// GameLogic
			Thread.CurrentThread.Name = "GameLogic";
			GameLogicTask();
		}
	}
}
