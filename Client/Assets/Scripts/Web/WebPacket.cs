using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ŭ�� -> ����
public class CreateAccountPacketReq
{
    public string AccountName;
    public string Password;
}

// ���� -> Ŭ��
public class CreateAccountPacketRes
{
    public bool CreateOk;
}

// Ŭ�� -> ����
public class LoginAccountPacketReq
{
    public string AccountName;
    public string Password;
}

public class ServerInfo
{
    public string Name;
    public string Ip;
    // ���� ȥ�� ����
    public int CrowdedLevel;
}


// ���� -> Ŭ��
public class LoginAccountPacketRes
{
    public bool LoginOk;
    // ���� ����Ʈ
    public List<ServerInfo> ServerList = new List<ServerInfo>();
}


