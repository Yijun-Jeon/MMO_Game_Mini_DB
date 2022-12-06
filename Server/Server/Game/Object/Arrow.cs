using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Arrow : Projecttile
    {
        // 화살을 쏜 주인
        public GameObject Owner { get; set; }
        public override void Update()
        {
            if (Data == null || Data.projecttile == null || Owner == null || Room == null)
                return;

            int tick = (int)(1000 / Data.projecttile.speed);
            Room.PushAfter(tick, Update);

            Vector2Int destPos = GetFrontCellPos();
            if(Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(CellPos,movePacket);
            }
            // 충돌
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    // 피격 판정
                    // 화살 데미지 + 플레이어 공격력
                    target.OnDamaged(this, Data.damage + Owner.TotalAttack);
                }
                // 소멸
                //Room.LeaveGame(Id);
                Room.Push(Room.LeaveGame, Id);
            }
        }

        public override GameObject GetOnwer()
        {
            return Owner;
        }
    }
}
