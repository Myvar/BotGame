using System.Collections.Generic;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.U)]
    public class UpgradeInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            if (p.Tokens >= Program.WorldState.RangeUpgradeCost)
            {
                p.Tokens -= Program.WorldState.RangeUpgradeCost;
                p.Range += 1;
                return $"{p.Name} has Upgraded";
            }

            return $"ERROR {p.Name} afford to Upgrade.";
        }
    }
}