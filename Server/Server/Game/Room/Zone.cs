using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Zone
    {
        public int IndexY { get; private set; }
        public int IndexX { get; private set; }

        // Zone에 있는 플레이어들
        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        // Zone에 있는 몬스터들
        public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();
        // Zone에 있는 투사체들
        public HashSet<Projecttile> Projecttiles { get; set; } = new HashSet<Projecttile>();

        public Zone(int y, int x)
        {
            IndexY = y;
            IndexX = x;
        }

        public void Remove(GameObject gameObject)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            switch (type)
            {
                case GameObjectType.Player:
                    Players.Remove((Player)gameObject);
                    break;
                case GameObjectType.Monster:
                    Monsters.Remove((Monster)gameObject);
                    break;
                case GameObjectType.Projecttile:
                    Projecttiles.Remove((Projecttile)gameObject);
                    break;
            }
        }
        
        // 조건에 맞는 한 플레이어 서치
        public Player FindOne(Func<Player, bool> condition)
        {
            foreach(Player player in Players)
            {
                if (condition.Invoke(player))
                    return player;
            }
            return null;
        }

        // 조건에 맞는 모든 플레이어 서치
        public List<Player> FindAll(Func<Player, bool> condition)
        {
            List<Player> findList = new List<Player>();
            foreach (Player player in Players)
            {
                if (condition.Invoke(player))
                    findList.Add(player);
            }
            return findList;
        }
    }
}