using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    // PreGame - Game에 들어가기 이전 상태 관리
    public partial class ClientSession : PacketSession
    {
		// 메모리에 저장되는 플레이어들
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();
		public int AccountDbId { get; private set; }
        public void HandleLogin(C_Login loginPacket)
        {
			// TODO : 이런 저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			// 두 번 호출될 경우 오류 방지를 위해 초기화
			LobbyPlayers.Clear();

			// 유효한 아이디인지 확인
			using (AppDbContext db = new AppDbContext())
			{
				AccountDb findAccount = db.Accounts
					.Include(a => a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null)
				{
					// AccountDbId 메모리에 기억
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach(PlayerDb playerDb in findAccount.Players)
                    {
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
						{
							Name = playerDb.PlayerName,
							StatInfo = new StatInfo()
							{
								Hp = playerDb.Hp,
								MaxHp = playerDb.MaxHp,
								Level = playerDb.Level,
								TotalExp = playerDb.TotalExp,
								Attack = playerDb.Attack,
								Speed = playerDb.Speed
                            }

						};
						// 서버 메모리에도 저장
						LobbyPlayers.Add(lobbyPlayer);

						// 패킷에 넣어줌
						loginOk.Players.Add(lobbyPlayer);
                    }
					Send(loginOk);

					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
				else
				{
					// 아이디가 없으면 생성해 줌
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					db.SaveChanges(); // TODO : Exception

					// AccountDbId 메모리에 기억
					AccountDbId = newAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);

					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
			// 로비에서만 생성 가능
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			using(AppDbContext db = new AppDbContext())
            {
				// 해당 이름을 가진 플레이어가 이미 있는지 확인
				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

				if(findPlayer != null)
                {
					// 이름이 겹침
					Send(new S_CreatePlayer());
                }
				else
                {
					// 1레벨 스탯 정보 추출
					StatInfo stat = null;
					DataManager.StatDict.TryGetValue(1, out stat);

					// DB에 플레이어 생성
					PlayerDb newPlayerDb = new PlayerDb()
					{
						PlayerName = createPacket.Name,
						Level = stat.Level,
						Hp = stat.Hp,
						MaxHp = stat.MaxHp,
						Attack = stat.Attack,
						Speed = stat.Speed,
						TotalExp = 0,
						AccountDbId = AccountDbId,
					};
					db.Players.Add(newPlayerDb);
					db.SaveChanges(); // TODO : ExceptionHandling

					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
					{
						Name = createPacket.Name,
						StatInfo = new StatInfo()
						{
							Hp = stat.Hp,
							MaxHp = stat.MaxHp,
							Level = stat.Level,
							TotalExp = 0,
							Attack = stat.Attack,
							Speed = stat.Speed
						}

					};
					// 서버 메모리에도 저장
					LobbyPlayers.Add(lobbyPlayer);

					// 클라에 전송
					S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
					newPlayer.Player.MergeFrom(lobbyPlayer);

					Send(newPlayer);
				}
            }
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
			// 로비에서만 게임 입장 가능
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
			if (playerInfo == null)
				return;

			// 기존 입장 & MyPlayer setting 부분
			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.Info.Name = playerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;
				MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
				MyPlayer.Session = this;
			}

			ServerState = PlayerServerState.ServerStateGame;

			// 1번방에 플레이어 입장
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}
    }
}
