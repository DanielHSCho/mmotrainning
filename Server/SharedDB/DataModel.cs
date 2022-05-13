using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedDB
{
    [Table("Token")]
    public class TokenDb
    {
        // PK
        public int TokenDbId { get; set; }
        public int AccountDbId { get; set; }
        public int Token { get; set; }
        public DateTime Expired { get; set; }
    }

    // Note : Redis로 개선할수도 있음
    [Table("ServerInfo")]
    public class ServerDb
    {
        // PK
        public int ServerDbId { get; set; }
        public string Name { get; set; }
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int BusyScore { get; set; }
    }
}
