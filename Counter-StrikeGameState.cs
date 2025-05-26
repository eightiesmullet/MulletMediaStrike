using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Mullet_Media_Strike_6._9
{
    internal class Counter_StrikeGameState
    {
        public string providerSteamid;
        public string playerSteamid;
        public string activity;
        public string phase;
        public string gameMode;
        public double phaseEndsIn;
        public int playerHealth;
        public string mapPhase;
        public string currentRound;
        public MatchStats matchStats;

        public Counter_StrikeGameState(string json)
        {
            var jsonObj = JObject.Parse(json);
            providerSteamid = jsonObj["provider"]?["steamid"]?.ToString() ?? "";
            playerSteamid = jsonObj["player"]?["steamid"]?.ToString() ?? "";
            activity = jsonObj["player"]?["activity"]?.ToString() ?? "";
            phase = jsonObj["round"]?["phase"]?.ToString() ?? "";
            gameMode = jsonObj["map"]?["mode"]?.ToString() ?? "";
            phaseEndsIn = jsonObj["map"]?["phase_ends_in"]?.Value<double>() ?? 0;
            playerHealth = jsonObj["state"]?["health"]?.Value<int>() ?? 0;
            mapPhase = jsonObj["map"]?["phase"]?.ToString() ?? "";
            currentRound = jsonObj["map"]?["round"]?.ToString() ?? "";
            matchStats = new MatchStats(json, providerSteamid, playerSteamid, activity, phase);

        }   
    }
}
