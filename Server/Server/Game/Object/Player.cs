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
    }
}
