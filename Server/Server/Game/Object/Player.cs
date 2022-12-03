using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data.DB;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Inventory Inven { get; private set; } = new Inventory();

        // 아이템 착용으로 인한 추가 스탯
        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        // 순수 스탯 + 추가 스탯
        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker,damage);
        }
        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            // DB 연동
            DbTransaction.SavePlayerStatus_Step1(this, Room);
        }

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            // 플레이어의 인벤토리에 해당 아이템이 있는지 확인
            Item item = Inven.Get(equipPacket.ItemDbId);
            if (item == null)
                return;

            // 물약이면 장착에서 제외
            if (item.ItemType == ItemType.Consumable)
                return;

            // 착용 요청이라면, 겹치는 부위 해제
            if (equipPacket.Equipped)
            {
                Item unequipItem = null;
                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                // 방어구면 방어구 부위도 검사
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = Inven.Find(
                        i => i.Equipped && i.ItemType == ItemType.Armor
                        && ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null)
                {
                    // 메모리 선 적용
                    unequipItem.Equipped = false;

                    // DB에 Noti
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    // 클라에 통보
                    S_EquipItem equiptNoItem = new S_EquipItem();
                    equiptNoItem.ItemDbId = unequipItem.ItemDbId;
                    equiptNoItem.Equipped = unequipItem.Equipped;
                    Session.Send(equiptNoItem);
                }
            }

            {
                // 메모리 선 적용
                item.Equipped = equipPacket.Equipped;

                // DB에 Noti
                DbTransaction.EquipItemNoti(this, item);

                // 클라에 통보
                S_EquipItem equiptOkItem = new S_EquipItem();
                equiptOkItem.ItemDbId = equipPacket.ItemDbId;
                equiptOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equiptOkItem);
            }

            RefreshAdditionalStat();
        }
        // 플레이어의 추가 스탯 갱신
        public void RefreshAdditionalStat()
        {
            // 처음부터 공식을 다시 계산하는 것이 버그를 최소화 할 수 있음
            // 다소 부하는 있게 됨
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach(Item item in Inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;
                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }
    }
}
