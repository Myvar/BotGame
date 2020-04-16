using System.Collections.Generic;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.H)]
    public class HealInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            if (p.Tokens >= Program.WorldState.HealthUpgradeCost && p.Health < 3)
            {
                p.Tokens -= Program.WorldState.HealthUpgradeCost;
                p.Health += 1;
                return $"{p.Name} has Healed";
            }

            return $"ERROR {p.Name} afford to Heal.";
        }
    }
}