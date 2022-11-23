using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    // 투사체 모두를 관리하는 클래스
    public class Projecttile : GameObject
    {
        public Data.Skill Data { get; set; }
        public Projecttile()
        {
            ObjectType = GameObjectType.Projecttile;
        }

        public virtual void Update()
        {

        }
    }
}
