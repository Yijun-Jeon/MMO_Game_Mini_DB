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

		clientSession.HandleLogin(loginPacket);
	}

	public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
		C_EnterGame enterGamePacket = packet as C_EnterGame;
		// 캐스팅이 확실한 경우 사용 가능 - 좀 더 빠름
		ClientSession clientSession = (ClientSession)session;

		clientSession.HandleEnterGame(enterGamePacket);

	}

	public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		C_CreatePlayer createPlayerPacket = (C_CreatePlayer)packet;
		ClientSession clientSession = (ClientSession)session;

		clientSession.HandleCreatePlayer(createPlayerPacket);
	}

	public static void C_EquipItemHandler(PacketSession session, IMessage packet)
	{
		C_EquipItem equipPacket = (C_EquipItem)packet;
		ClientSession clientSession = (ClientSession)session;

		// 게임룸에 들어가야 장착이 가능하기 때문에 룸에서 처리
		Player player = clientSession.MyPlayer;
		if (player == null)
			return;
		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleEquipItem, player, equipPacket);
	}

	public static void C_PongHandler(PacketSession session, IMessage packet)
	{
		ClientSession clientSession = (ClientSession)session;
		clientSession.HandlePong();
	}
}
