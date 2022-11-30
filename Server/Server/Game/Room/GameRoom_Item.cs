using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Data.DB;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            // 플레이어의 인벤토리에 해당 아이템이 있는지 확인
            Item item = player.Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;

            // 메모리 선 적용
            item.Equipped = equipPacket.Equipped;

            // DB에 Noti
            DbTransaction.EquipItemNoti(player, item);

            // 클라에 통보
            S_EquipItem equiptOkItem = new S_EquipItem();
            equiptOkItem.ItemDbId = item.ItemDbId;
            equiptOkItem.Equipped = item.Equipped;
            player.Session.Send(equiptOkItem);
        }
    }
}

