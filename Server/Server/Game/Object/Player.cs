using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
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

            // 피가 깎일 때마다 DB에 접근하면 부하가 심함
            // 플레이어가 방을 나갈 때 비로소 DB에 저장

            using (AppDbContext db = new AppDbContext())
            {
                // DB 접근 2번 - get, set
                /*PlayerDb playerDb = db.Players.Find(PlayerDbId);
                playerDb.Hp = Stat.Hp;
                db.SaveChanges();*/

                // DB 접근 1번 - set
                PlayerDb playerDb = new PlayerDb();
                playerDb.PlayerDbId = PlayerDbId;
                playerDb.Hp = Stat.Hp;

                db.Entry(playerDb).State = EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(playerDb.Hp)).IsModified = true;
                db.SaveChangesEx();
            }
        }
    }
}
