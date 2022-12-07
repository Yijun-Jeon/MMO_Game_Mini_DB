using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public const int VisionCells = 5;
        public int RoomId { get; set; }

        Dictionary<int,Player> _players = new Dictionary<int,Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int,Monster>();
        Dictionary<int, Projecttile> _projecttiles = new Dictionary<int,Projecttile>();

        public Zone[,] Zones { get; private set; }
        // 하나의 존의 범위
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();
        
        public Zone GetZone(Vector2Int cellPos)
        {
            // Cell 좌표에서 Grid 좌표로 변환
            // Grid 좌표에서 ZoneCell 단위로 변환
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            if (x < 0 || x >= Zones.GetLength(1))
                return null;

            if (y < 0 || y >= Zones.GetLength(0))
                return null;

            return Zones[y, x];
            
        }

        public void Init(int mapId,int zoneCells)
        {
            Map.LoadMap(mapId,"../../../../../Common/MapData");

            // Zone 초기화
            ZoneCells = zoneCells;

            // Ex) 10 zoneCells
            // 1~10칸 = 1존
            // 11~20칸 = 2존
            // 21~30칸 = 3존
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            Zones = new Zone[countY, countX];

            for(int y = 0; y < countY; y++)
            {
                for(int x = 0; x<countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }

            // TEMP
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(1);
            monster.CellPos = new Vector2Int(5, 5);
            EnterGame(monster);
            //Push(EnterGame, monster);
        }

        // 누군가가 주기적으로 호출해줘야함
        public void Update()
        { 
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

                player.RefreshAdditionalStat();

                // Map의 _players 갱신
                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                // 해당 위치에 맞는 Zone에 플레이어 배치
                GetZone(player.CellPos).Players.Add(player);

                if (player.Hp == 0)
                    player.OnDead(player);

                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    player.Vision.Update();
                }
            }
            else if(type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                GetZone(monster.CellPos).Monsters.Add(monster);
                // Map의 _players 갱신
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

                monster.Update();
            }
            else if(type == GameObjectType.Projecttile)
            {
                Projecttile projecttile = gameObject as Projecttile;
                _projecttiles.Add(gameObject.Id, projecttile);
                projecttile.Room = this;

                GetZone(projecttile.CellPos).Projecttiles.Add(projecttile);
                projecttile.Update();
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

                // 해당 위치에 맞는 Zone에 플레이어 제거
                GetZone(player.CellPos).Players.Remove(player);

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

                GetZone(monster.CellPos).Monsters.Remove(monster);
                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projecttile)
            {
                Projecttile projecttile = null;
                if (_projecttiles.Remove(objectId, out projecttile) == false)
                    return;

                GetZone(projecttile.CellPos).Projecttiles.Remove(projecttile);
                projecttile.Room = null;
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

        public void Broadcast(Vector2Int pos,IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);

            foreach (Player p in zones.SelectMany(z => z.Players))
            {
                int dx = pos.x - p.CellPos.x;
                int dy = pos.y - p.CellPos.y;
                // 시야각 밖에 위치
                if (Math.Abs(dx) > GameRoom.VisionCells)
                    continue;
                if (Math.Abs(dy) > GameRoom.VisionCells)
                    continue;

                p.Session.Send(packet);
            }
        }

        // 내 영역에 속하는 Zone 리스트
        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int cells = GameRoom.VisionCells) // 반경
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int[] delta = new int[2] { -cells, +cells };
            foreach(int dy in delta)
            {
                foreach(int dx in delta)
                {
                    int y = cellPos.y + dy;
                    int x = cellPos.x + dx;
                    Zone zone = GetZone(new Vector2Int(x, y));
                    if (zone == null)
                        continue;

                    zones.Add(zone);
                }
            }
            return zones.ToList();
        }
    }
}

