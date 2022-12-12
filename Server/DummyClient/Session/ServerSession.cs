using Google.Protobuf.Protocol;
using Google.Protobuf;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class ServerSession : PacketSession
{
	public int DummyId { get; set; }
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

		Send(new ArraySegment<byte>(sendBuffer));
	}

	public override void OnConnected(EndPoint endPoint)
	{
		Console.WriteLine($"OnConnected : {endPoint}");
	}

	public override void OnDisconnected(EndPoint endPoint)
	{
		Console.WriteLine($"OnDisconnected : {endPoint}");
	}

	public override void OnRecvPacket(ArraySegment<byte> buffer)
	{
		PacketManager.Instance.OnRecvPacket(this, buffer);
	}

	public override void OnSend(int numOfBytes)
	{
		//Console.WriteLine($"Transferred bytes: {numOfBytes}");
	}
}