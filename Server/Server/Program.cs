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

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			// DB
			InitializeDB(forceReset: true);

			GameLogic.Instance.Push(() =>
			{
				// 게임룸 생성
				GameLogic.Instance.Add(1);
			});

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            // GameLogicTask
            {
				// 쓰레드 풀링X -> 별도로 하나를 더 파줌
				Task gameLogicTask = new Task(GameLogicTask, TaskCreationOptions.LongRunning);
				gameLogicTask.Start();
            }
			// NetworkTask
			{
				// 쓰레드 풀링X -> 별도로 하나를 더 파줌
				Task networkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
				networkTask.Start();
			}

			// DbTask
			DbTask();
		}
	}
}
