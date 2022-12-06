using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameLogic : JobSerializer
    {
        public static GameLogic Instance { get; } = new GameLogic();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            // 가진 모든 방을 돌면서 Update
            foreach(GameRoom room in _rooms.Values)
            {
                room.Update();
            }
        }

        // GameRoom 생성
        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            //gameRoom.Init(mapId);
            gameRoom.Push(gameRoom.Init, mapId,10);

            gameRoom.RoomId = _roomId;
            _rooms.Add(_roomId, gameRoom);
            _roomId++;

            return gameRoom;
        }

        // GameRoom 제거
        public bool ReMove(int roomId)
        {
            return _rooms.Remove(roomId);
        }

        // GameRoom 찾음
        public GameRoom Find(int roomId)
        {
            GameRoom room = null;
            if (_rooms.TryGetValue(roomId, out room))
                return room;

            return null;
        }
    } 
}
