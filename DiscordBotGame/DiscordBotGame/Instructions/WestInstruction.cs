using System.Collections.Generic;
using System.Linq;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.W)]
    public class WestInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            for (int i = 0; i < (int) (c.Argument == 0 ? 1 : c.Argument); i++)
            {
                if ((int) p.Position.X == 0)
                {
                    return "ERROR West is out of bounds";
                }

                if (Program.WorldState.Players.Any(x =>
                    (int) x.Position.X == (int) p.Position.X - 1 && (int) x.Position.Y == (int) p.Position.Y && !x.Dead)
                )
                {
                    return
                        "ERROR Another Player is already occupying that space";
                }

                if (p.Tokens < 1)
                {
                    return "ERROR You do not have enough tokens to move.";
                }


                p.Position.X -= 1;
                p.Tokens -= 1;
            }

            return $"{p.Name} Has Move{(c.Argument == 0 ? "" : "d " + c.Argument + " times")} West";
        }
    }
}