using System.Collections.Generic;

public class CreateAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class CreateAccountPacketRes
{
    // TODO : 구체적인 실패 사유가 있다면 int로
    public bool CreateOk { get; set; }
}

public class LoginAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class ServerInfo
{
    public string Name { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int BusyScore { get; set; }
}

public class LoginAccountPacketRes
{
    public bool LoginOk { get; set; }
    public int AccountId { get; set; }
    public int Token { get; set; }
    public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
}

// TODO : 대기열
// 대기열은 웹서버일지 게임서버일지 고민한 후 구현
