using System;
using System.Collections.Generic;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.A)]
    public class AttackInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            if (p.Target == null)
            {
                return "ERROR You have not targeted a player";
            }

            if (p.Target.Dead)
            {
                return $"ERROR {p.Target.Name} is dead";
            }


            if (p.Tokens <= 0)
            {
                return $"ERROR {p.Name} does not have enough tokens to attack {p.Target.Name}";
            }

            if (DateTime.Now - p.AttackTimeCoolDown < TimeSpan.FromHours(4))
            {
                return $"ERROR {p.Name} has a {(DateTime.Now - p.AttackTimeCoolDown).TotalHours} attack cooldown";
            }

            if (p.Target == p)
            {
                return $"ERROR {p.Name} can not attack {p.Name}";
            }

            if (((int) Math.Truncate(p.Target.Position.DistanceTo(p.Position)) > p.Range))
            {
                return $"ERROR {p.Target.Name} is out of range of {p.Name}";
            }

            p.Target.Health -= 1;
            p.Tokens -= 1;

            if (p.Target.Health < 1)
            {
                p.Target.Dead = true;
            }

            p.AttackTimeCoolDown = DateTime.Now;

            return $"{p.Name} attacked {p.Target.Name}";
        }
    }
}