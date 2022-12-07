using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Game.Room
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        // 이전 시야에서 보인 객체들
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        // 현재 시야각의 객체들 긁어옴
        public HashSet<GameObject> GatherObjects()
        {
            if (Owner == null || Owner.Room == null)
                return null;

            HashSet<GameObject> objects = new HashSet<GameObject>();

            // 현재 Zone을 특정
            List<Zone> zones = Owner.Room.GetAdjacentZones(Owner.CellPos);

            Vector2Int cellPos = Owner.CellPos;
            foreach (Zone zone in zones)
            {
                foreach(Player player in zone.Players)
                {
                    int dx = cellPos.x - player.CellPos.x;
                    int dy = cellPos.y - player.CellPos.y;
                    // 시야각 밖에 위치
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(player);
                }

                foreach (Monster monster in zone.Monsters)
                {
                    int dx = cellPos.x - monster.CellPos.x;
                    int dy = cellPos.y - monster.CellPos.y;
                    // 시야각 밖에 위치
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(monster);
                }

                foreach (Projecttile projecttile in zone.Projecttiles)
                {
                    int dx = cellPos.x - projecttile.CellPos.x;
                    int dy = cellPos.y - projecttile.CellPos.y;
                    // 시야각 밖에 위치
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(projecttile);
                }
            }

            return objects;
        }

        // 이전과 현재 시야각 객체들 비교
        public void Update()
        {
            if (Owner == null || Owner.Room == null)
                return;

            HashSet<GameObject> currentObjects = GatherObjects();

            // 기존엔 없었는데 새로 생긴 애들 Spawn 처리
            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if(added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();

                foreach(GameObject gameObject in added)
                {
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(gameObject.Info);
                    spawnPacket.Objects.Add(info);
                }

                Owner.Session.Send(spawnPacket);
            }
            // 기존엔 있었는데 사라진 애들 Despawn 처리
            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if (removed.Count > 0)
            {
                S_Despawn despawnPacket = new S_Despawn();

                foreach (GameObject gameObject in removed)
                {
                    despawnPacket.ObjectIds.Add(gameObject.Id);
                }

                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;

            // 0.1초에 한번씩 검사
            Owner.Room.PushAfter(100, Update);
        }
    }
}
