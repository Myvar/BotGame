using System.Collections.Generic;

namespace DiscordBotGame.Instructions
{
    public abstract class Instruction
    {
        public abstract string Handel(Player p, List<Player> players, Command c);
    }
}