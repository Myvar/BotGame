using System.Collections.Generic;
using System.Linq;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.TD)]
    public class TargetByIdInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            var t = players.First(x => x.DiscordID == c.Argument);

            if (t == null)
            {
                return $"ERROR no user found with id: {c.Argument}";
            }

            p.Target = t;

            return $"{p.Name} has Targeted {p.Target.Name}";
        }
    }
}