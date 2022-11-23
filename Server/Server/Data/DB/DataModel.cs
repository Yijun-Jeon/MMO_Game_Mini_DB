﻿using System;
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
        public AccountDb Account { get; set; }
    }
}