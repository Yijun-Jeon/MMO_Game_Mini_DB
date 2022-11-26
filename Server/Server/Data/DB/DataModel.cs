using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Server.DB
{
    [Table("Account")]
    public class AccountDb
    {
        // 자동으로 PK로 설정
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }
        // 1:m 관계
        public ICollection<PlayerDb> Players { get; set; }
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }

        // m:1 관계
        [ForeignKey("Account")]
        public int AccountDbId { get; set; }
        public AccountDb Account { get; set; }

        // 1:m 관계
        public ICollection<ItemDb> Items { get; set; }

        // StatInfo와 동일하게 Stat 정보
        public int Level { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Attack { get; set; }
        public float Speed { get; set; }
        public int TotalExp { get; set; }
    }

    [Table("Item")]
    public class ItemDb
    {
        public int ItemDbId { get; set; }
        public int TemplateId { get; set; } // 아이템 구분 Id
        public int Count { get; set; } // 보유 수
        // ex) 0~10 : 착용중인 아이템, 11~40 : 보유중인 아이템, 40~ :창고에 있는 아이템...
        public int Slot { get; set; } // 인벤토리 슬롯 넘버

        [ForeignKey("Owner")]
        public int? OwnerDbId { get; set; }
        public PlayerDb Owner { get; set; }
    }
}