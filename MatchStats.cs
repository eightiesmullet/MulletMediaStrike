using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Mullet_Media_Strike_6._9
{
    public class MatchStats
    {
        public int kills;
        public int assists;
        public int deaths;
        public int mvps;
        public int score;

        public MatchStats(string json, string providerSteamid, string playerSteamid, string activity, string phase)
        {
            var jsonObj = JObject.Parse(json);
            var matchStats = jsonObj["player"]?["match_stats"];
            if (providerSteamid == playerSteamid && activity == "playing" && phase == "live")
            {
                kills = matchStats?["kills"]?.Value<int>() ?? 0;
                assists = matchStats?["assists"]?.Value<int>() ?? 0;
                deaths = matchStats?["deaths"]?.Value<int>() ?? 0;
            }

        }
    }
}
