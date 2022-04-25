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
        public string connectionString;
        // TODO : 나중에 다음 Json을 bin에 config파일 자동 생성할 수 있도록 하기
        /*
         {
            "dataPath": "../../../../../Client/Assets/Resources/Data",
            "connectionString": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameDB;"
            }
         */

        // TODO : 최대 동접 / 포트 번호도 이곳에서 관리
    }

    class ConfigManager
    {
        public static ServerConfig Config { get; private set; }
        public static void LoadConfig()
        {
            string text = File.ReadAllText("config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}
