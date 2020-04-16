using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DiscordBotGame.Instructions
{
    [InstructionHandler(Opcode.T)]
    public class TargetInstruction : Instruction
    {
        public override string Handel(Player p, List<Player> players, Command c)
        {
            var dict = new Dictionary<int, Player>();

            foreach (var player in players)
            {
                if (player != p) dict.Add((int) Math.Truncate(player.Position.DistanceTo(p.Position)), player);
            }

            var sorted = dict.ToImmutableSortedDictionary();

            if (sorted.Count == 0)
            {
                return "ERROR no other players found to target";
            }

            var t = sorted.Values.ToImmutableArray()[(int) c.Argument];

            p.Target = t;

            return $"{p.Name} has Targeted {p.Target.Name}";
        }
    }
}