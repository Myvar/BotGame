using System;
using System.Collections.Generic;

namespace DiscordBotGame
{
    public class WorldState
    {
        public bool GameStarted { get; set; }
        public ulong AdminID { get; set; }
        public ulong ChanelID { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public DateTime LastUpdateTimeStamp = DateTime.Now;

        public TimeSpan TokenHandOutInterval = TimeSpan.FromDays(1); // release config
        //public TimeSpan TokenHandOutInterval = TimeSpan.FromMinutes(5); //debug config
        
        public int WorldSize = 15;
        public int StartTokens = 3;
        public int StartHealth = 3;
        public int StartRange = 1;
        public int TokensPerCycle = 1;
        public int RangeUpgradeCost = 4;
        public int HealthUpgradeCost = 4;
        //Health Upgrade is redundant as far as I can tell seen as the offensive options available heavily outway the defensive option of healing. For the same cost you can kill and move.
        //I suggest we should change the cost or the amount of health that you get, but untill further testing we won't know how balanced the current cost/health is
    }
}