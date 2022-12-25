using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public class NetworkManager
{
	public int AccountId { get; set; }
	public int Token { get; set; }

	ServerSession _session = new ServerSession();

	public void Send(IMessage sendBuff)
	{
		_session.Send(sendBuff);
	}

	public void ConnectToGame(ServerInfo info)
	{
		IPAddress ipAddr = IPAddress.Parse(info.IpAddress);
		IPEndPoint endPoint = new IPEndPoint(ipAddr, info.Port);

		Connector connector = new Connector();

		connector.Connect(endPoint,
			() => { return _session; },
			1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}	
	}

}
