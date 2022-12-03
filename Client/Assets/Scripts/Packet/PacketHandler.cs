using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;

        Managers.Object.Add(enterGamePacket.Player, myPlayer:true);
	}

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;

        Managers.Object.Clear();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;

        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        // 이동 해당 플레이어 검색
        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        // 내 플레이어라면 좌표 재확인 불필요
        if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.PosInfo = movePacket.PosInfo;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;

        GameObject go = Managers.Object.FindById(skillPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.UseSkill(skillPacket.Info.SkillId);
        }
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;

        GameObject go = Managers.Object.FindById(changePacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.Hp = changePacket.Hp;
        }
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diepacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diepacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.Hp = 0;
            cc.OnDead();
        }
    }

    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_ConnectedHandler");
        C_Login loginPacket = new C_Login();
        // 시스템 별로 유니크한 아이디 할당해줌
        // 로컬에서 같은 머신으로 테스트 할 때는 추가 조치 필요
        loginPacket.UniqueId = SystemInfo.deviceUniqueIdentifier;
        Managers.Network.Send(loginPacket);
    }

    // 로그인 OK + 캐릭터 목록 받음
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        Debug.Log($"LoginOk({loginPacket.LoginOk})");

        // TODO : 로비 UI에서 캐릭터 보여주고, 선택할 수 있도록

        // 플레이어가 없는 경우 생성
        if(loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
            Managers.Network.Send(createPacket);
        }
        else
        {
            // 일단 무조건 첫 번째 플레이어로 접속
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            Managers.Network.Send(enterGamePacket);
        }

    }

    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPlayer = (S_CreatePlayer)packet;

        // 플레이어 생성 재송신
        if(createOkPlayer.Player == null)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"Player_{Random.Range(0, 10000).ToString("0000")}";
            Managers.Network.Send(createPacket);
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPlayer.Player.Name;
            Managers.Network.Send(enterGamePacket);
        }
    }

    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;
        
        Managers.Inven.Clear();
        // 메모리상 아이템 정보 적용
        foreach (ItemInfo itemInfo in  itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);
        }

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem itemList = (S_AddItem)packet;

        // 메모리상 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);
        }

        Debug.Log("아이템을 획득했습니다!");

        // UI 설정
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItemOk = (S_EquipItem)packet;

        // 메모리에 아이템 정보 적용
        Item item = Managers.Inven.Get(equipItemOk.ItemDbId);
        if (item == null)
            return;

        item.Equipped = equipItemOk.Equipped;
        
        Debug.Log("아이템을 착용 변경!");

        // UI 설정
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat changeStat = (S_ChangeStat)packet;

        // TODO
    }
}
