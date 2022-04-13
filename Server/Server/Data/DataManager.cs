using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public class DataManager
    {
        public Dictionary<int, Data.Stat> StatDict { get; private set; } = new Dictionary<int, Data.Stat>();

        public void Init()
        {
            StatDict = LoadJson<Data.StatData, int, Data.Stat>("StatData").MakeDict();
        }

        Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }

}
