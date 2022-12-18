using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 클라 -> 서버
public class CreateAccountPacketReq
{ 
    public string AccountName { get; set; }
    public string Password { get; set; }
}

// 서버 -> 클라
public class CreateAccountPacketRes
{
    public bool CreateOk { get; set; }
}

// 클라 -> 서버
public class LoginAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class ServerInfo
{ 
    public string Name { get; set; }
    public string Ip { get; set; }
    // 서버 혼잡 정도
    public int CrowdedLevel { get; set; }
}


// 서버 -> 클라
public class LoginAccountPacketRes
{
    public bool LoginOk { get; set; }
    // 서버 리스트
    public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
}