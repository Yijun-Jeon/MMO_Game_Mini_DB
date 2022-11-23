using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		//Console.WriteLine($"C_Move ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		// 멀티쓰레드 대비하여 꺼내서 사용
		// MyPlayer가 도중에 null로 바뀌어도 무방함
		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

        // 멀티쓰레드 대비하여 꺼내서 사용
        // MyPlayer가 LeaveGame하여 GameRoom이 null로 바뀌어도 무방함
        GameRoom room = player.Room;
		if (room == null)
			return;

		// GameRoom에서만 처리하도록 조치
		//room.HandleMove(player, movePacket);
		room.Push(room.HandleMove, player, movePacket);
	}

    public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

		//room.HandleSkill(player, skillPacket);
		room.Push(room.HandleSkill, player, skillPacket);
    }

public static void C_LoginHandler(PacketSession session, IMessage packet)
{
	C_Login loginPacket = packet as C_Login;
	ClientSession clientSession = session as ClientSession;

    Console.WriteLine($"UniqueId({loginPacket.UniqueId})");

	// TODO : 이런 저런 보안 체크

	// 유효한 아이디인지 확인
	using (AppDbContext db = new AppDbContext())
    {
		AccountDb findAccount = db.Accounts
			.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

		if(findAccount != null)
        {
			S_Login loginOk = new S_Login() { LoginOk = 1 };
			clientSession.Send(loginOk);
        }
		else
        {
			// 아이디가 없으면 생성해 줌
			AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
			db.Accounts.Add(newAccount);
			db.SaveChanges();

			S_Login loginOk = new S_Login() { LoginOk = 1 };
			clientSession.Send(loginOk);
		}
    }
}
}
