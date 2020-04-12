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
    }
}