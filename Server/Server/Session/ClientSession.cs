using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.Game;
using Server.Data;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;

		// 관리하는 내 플레이어
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		object _lock = new object();
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		// 클라에서 최근 마지막으로 응답을 준 시간
		long _pingpongTick = 0;
		public void Ping()
        {
			if(_pingpongTick > 0)
            {
				long delta = (System.Environment.TickCount64 - _pingpongTick);
				// 마지막 응답부터 30초가 지남
				if(delta > 30 * 1000)
                {
                    Console.WriteLine("Disconnected by PingCheck");
					Disconnect();
					return;
                }
            }

			// 응답 통과
			S_Ping pingPacket = new S_Ping();
			Send(pingPacket);

			GameLogic.Instance.PushAfter(5000, Ping);
        }

		public void HandlePong()
        {
			_pingpongTick = System.Environment.TickCount64;
        }

        #region Network
		// Send할 목록을 예약만 함
        public void Send(IMessage packet)
		{
			// packet id 추출
			string msgName = packet.Descriptor.Name.Replace("_", String.Empty); // '_' 제거한 이름 추출
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName); // 이름이 같은 enum 값을 반환

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			lock(_lock)
            {
				// 일단 예약만 하고 넘겨줌
				_reserveQueue.Add(sendBuffer);
			}
			//Send(new ArraySegment<byte>(sendBuffer));
        }

		// 실제 Network IO 보내는 부분
		public void FlushSend()
        {
			List<ArraySegment<byte>> sendList = null;
			lock(_lock)
            {
				// 복사만 해주고 초기화함
				if (_reserveQueue.Count == 0)
					return;

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
            }

			Send(sendList);
        }

        public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

            // 클라에게 연결 됐다고 알려줌
            {
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
            }

			GameLogic.Instance.PushAfter(5000, Ping);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameLogic.Instance.Push(() =>
			{
				if (MyPlayer == null)
					return;

				GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
			});
			
			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
        #endregion
    }
}
