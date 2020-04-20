using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DiscordBotGame.Instructions;

namespace DiscordBotGame
{
    /*
     n<c> = move north
    s<c> = move south
    w<c> = move west
    e<c> = move east
    ne<c> = move north east
    nw<c> = move north west
    se<c> = move south east
    sw<c> = move south west

    t<index> = target player, index of list of players in decending
    order of distance eg: t0 will target closest plaer t1 second closest etc

    td<discord-id> = target player by discord id

    g<x> = give targeted player x amount of tokens
    a = attack targeted player
    h = heal
    u = upgrade range

    then you can chain them to make little programs for eg:
    ~exec ne3 e t0 g2

    is the same as typing
    ~move ne
    ~move ne
    ~move ne
    ~move e
    ~gift @Rhys 2

    what you guys think ?
    i think this will just help prevent command span
     */

    public static class GCodeEngine
    {
        private static Dictionary<Opcode, Instruction> _instructions { get; set; } =
            new Dictionary<Opcode, Instruction>();

        static GCodeEngine()
        {
            foreach (var type in typeof(Instruction).Assembly.GetTypes())
            {
                if (type.BaseType == typeof(Instruction))
                {
                    _instructions.Add(type.GetCustomAttribute<InstructionHandlerAttribute>().Opcode,
                        (Instruction) Activator.CreateInstance(type));
                }
            }
        }

        public static string Eval(Player p, WorldState ws, string src)
        {
            if (p.Dead) return "Your are dead";
        
            var commands = new List<Command>();

            //(?<opcode>[A-Za-z]([A-Za-z])?)(?<argument>[0-9]+)?
            var rex = new Regex("(\\s+)?(?<opcode>[A-Za-z]([A-Za-z])?)(?<argument>[0-9]+)?(\\s+)?");

            var matches = rex.Matches(src);

            foreach (Match match in matches)
            {
                var c = new Command()
                {
                    Instruction = Enum.Parse<Opcode>(match.Groups["opcode"].Value.Trim(), true)
                };

                if (match.Groups["argument"].Value.Trim() != "")
                {
                    c.Argument = ulong.Parse(match.Groups["argument"].Value.Trim());
                }

                commands.Add(c);
            }

            var log = new StringBuilder();
            log.Append("```");

            foreach (var c in commands)
            {//252114593629863946
                log.AppendLine(_instructions[c.Instruction].Handel(p, ws.Players, c));
            }

            log.AppendLine("Done");
            log.Append("```");
            return log.ToString();
        }
    }
}