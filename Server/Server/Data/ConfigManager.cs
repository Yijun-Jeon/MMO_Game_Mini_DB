using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    [Serializable]
    public class ServerConfig
    {
        public string dataPath;
        // 최대 동접 유저 수.. 등등
        public string connectionString;
    }

    public class ConfigManager
    {
        public static ServerConfig Config { get; set; }

        public static void LoadConfig()
        {
            // 경로는 기본적으로 실행파일과 같은 위치에 넣어두는 경우가 많음
            string text = File.ReadAllText("config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}
