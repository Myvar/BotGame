using System;
using System.Collections.Generic;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.G)]
    public class GiftInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            var amt = (int) c.Argument;

            if (p.Target == null)
            {
                return "ERROR You have not targeted a player";
            }

            if (p.Target.Dead)
            {
                return $"ERROR {p.Target.Name} is dead";
            }

            if (p.Target == p)
            {
                return $"ERROR {p.Name} can not attack {p.Name}";
            }

            if (p.Tokens < amt)
            {
                return $"ERROR {p.Name} can not afford to give {amt} tokens";
            }

            if (!((int) Math.Truncate(p.Target.Position.DistanceTo(p.Position)) >= p.Range))
            {
                return $"ERROR {p.Target.Name} is out of range of {p.Name}";
            }

            p.Target.Tokens += amt;
            p.Tokens -= amt;


            return $"{p.Name} gave {amt} tokens to {p.Target.Name}";
        }
    }
}