using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int,Player> _players = new Dictionary<int,Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int,Monster>();
        Dictionary<int, Projecttile> _projecttiles = new Dictionary<int,Projecttile>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId,"../../../../../Common/MapData");

            // TEMP
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(1);
            monster.CellPos = new Vector2Int(5, 5);
            //EnterGame(monster);
            Push(EnterGame, monster);
        }

        // 누군가가 주기적으로 호출해줘야함
        public void Update()
        {
            foreach (Monster monster in _monsters.Values)
            {
                monster.Update();
            }
            foreach(Projecttile projecttile in _projecttiles.Values)
            {
                projecttile.Update();
            }

            Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if(type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                // Map의 _players 갱신
                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    // 다른 유저들 정보
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.Info);


                    foreach(Projecttile p in _projecttiles.Values)
                        spawnPacket.Objects.Add(p.Info);

                    player.Session.Send(spawnPacket);
                }
            }
            else if(type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                // Map의 _players 갱신
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

            }
            else if(type == GameObjectType.Projecttile)
            {
                Projecttile projecttile = gameObject as Projecttile;
                _projecttiles.Add(gameObject.Id, projecttile);
                projecttile.Room = this;
            }
               

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach(Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }
            } 

        }
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.OnLeaveGame();
                Map.ApplyLeave(player);
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;

                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projecttile)
            {
                Projecttile projecttile = null;
                if (_projecttiles.Remove(objectId, out projecttile) == false)
                    return;

                projecttile.Room = null;
            }

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach(Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        // GameRoom Update -> Monster.Update 안에서 실행되므로 유지해도 됨
        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            // 모든 플레이어를 스캔하는 무식한 방법
            foreach(Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }
            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach(Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}

