using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SharedDB
{
    // Account 서버에서 인증을 받았다는 토큰
    [Table("Token")]
    public class TokenDb
    {
        // PK
        public int TokenDbId { get; set; }
        public int AccountDbId { get; set; }
        // Account 서버에서 랜덤으로 부여받은 토큰
        public int Token { get; set; }
        // 만료 날짜
        public DateTime Expired { get; set; }
    }

    // 여러 서버의 각각의 정보를 담은 DB
    [Table("ServerInfo")]
    public class ServerDb
    {
        // PK
        public int ServerDbId { get; set; } 
        // 서버 이름
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        // 서버 혼잡도
        public int BusyScore { get; set; }
    }

}
