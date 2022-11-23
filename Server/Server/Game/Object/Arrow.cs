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

        long _nextMoveTick = 0;
        public override void Update()
        {
            if (Data == null || Data.projecttile == null || Owner == null || Room == null)
                return;

            // 아직 시간이 덜 됨
            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long)(1000 / Data.projecttile.speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            Vector2Int destPos = GetFrontCellPos();
            if(Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket);
            }
            // 충돌
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    // 피격 판정
                    // 화살 데미지 + 플레이어 공격력
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                }
                // 소멸
                //Room.LeaveGame(Id);
                Room.Push(Room.LeaveGame, Id);
            }
        }
    }
}
