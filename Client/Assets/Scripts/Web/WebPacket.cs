using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 클라 -> 서버
public class CreateAccountPacketReq
{
    public string AccountName;
    public string Password;
}

// 서버 -> 클라
public class CreateAccountPacketRes
{
    public bool CreateOk;
}

// 클라 -> 서버
public class LoginAccountPacketReq
{
    public string AccountName;
    public string Password;
}

public class ServerInfo
{
    public string Name;
    public string Ip;
    // 서버 혼잡 정도
    public int CrowdedLevel;
}


// 서버 -> 클라
public class LoginAccountPacketRes
{
    public bool LoginOk;
    // 서버 리스트
    public List<ServerInfo> ServerList = new List<ServerInfo>();
}


