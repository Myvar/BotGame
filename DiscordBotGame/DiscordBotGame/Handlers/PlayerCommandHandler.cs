using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace DiscordBotGame
{
    [Handler]
    public static class PlayerCommandHandler
    {
        public static Random _rng = new Random();

        [Handler("ping")]
        public static string Ping(string cmd, SocketUser user)
        {
            return "Pong";
        }

        [Handler("help")]
        public static string Help(string cmd, SocketUser user)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                "This bot is not meant for general public use and only for CoN-FuZzie's. For more help contact Myvar.");
            sb.AppendLine("Here is a list of commands:");
            sb.AppendLine(
                $"Upgrade cost is {Program.WorldState.RangeUpgradeCost} and Health cost is {Program.WorldState.HealthUpgradeCost}");

            foreach (var handler in GameBotEngine.ImageHandlers)
            {
                sb.AppendLine("~" + handler.Key);
            }


            foreach (var handler in GameBotEngine.TextHandlers)
            {
                sb.AppendLine("~" + handler.Key);
            }

            user.GetOrCreateDMChannelAsync().Result.SendMessageAsync(sb.ToString()).Wait();

            throw new Exception();
        }

        [Handler("reset")]
        public static string Reset(string cmd, SocketUser user)
        {
            if (Program.WorldState.AdminID != user.Id) return "Imposter Alert! You're no admin; release the worms!";

            Program.WorldState.Players.Clear();

            return "Done";
        }

        [Handler("start")]
        public static string Start(string cmd, SocketUser user)
        {
            Program.WorldState.GameStarted = true;
            Program.WorldState.AdminID = user.Id;
            Program.WorldState.ChanelID = Program.CurrentChanelID;
            return "Game has been Started";
        }

        [Handler("player")]
        public static string Player(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";

            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            var id = ulong.Parse(segs[0].Split('!').Last().Trim().TrimEnd('>'));


            return DumpMe("me", Program._client.GetUser(id));
        }

        [Handler("players")]
        public static string Players(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";

            var data = new List<List<string>>();

            data.Add(new List<string>()
            {
                "Player",
                "Health",
                "Range",
                "Tokens",
                "Votes"
            });

            foreach (var player in Program.WorldState.Players)
            {
                data.Add(new List<string>()
                {
                    player.Name + "(" + (player.Dead ? "D" : "A") + ")",
                    player.Health.ToString(),
                    player.Range.ToString(),
                    player.Tokens.ToString(),
                    player.VoteCount.ToString()
                });
            }

            return "```" + Utils.ToTable(data) + "```";
        }

        [Handler("attack")]
        public static string Attack(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";
            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            var id = ulong.Parse(segs[0].Split('!').Last().Trim().TrimEnd('>'));
            return Eval($"eval td{id} a", user);
        }


        [Handler("vote")]
        public static string Vote(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";
            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            var id = ulong.Parse(segs[0].Split('!').Last().Trim().TrimEnd('>'));
            if (Program.WorldState.Players.All(x => x.DiscordID != id))
            {
                return $"The Target <@!{user.Id}> has not joined the game yet!";
            }

            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the game yet!";
            }

            var target = Program.WorldState.Players.First(x => x.DiscordID == id);
            var self = Program.WorldState.Players.First(x => x.DiscordID == user.Id);

            if (!self.Dead) return "Only the dead can fuck with the living.";
            if (target.Dead) return "Nice try, but voting for the dead is old news.";


            if (self.DeadVoteCast)
                return "Voting twice will get you jail time.";


            self.DeadVoteCast = true;
            target.VoteCount += 1;

            return $"<@!{target.DiscordID}> feels shivers is running up their spine.";
        }

        [Handler("gift")]
        public static string Gift(string cmd, SocketUser user)
        {
            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);


            var amt = int.Parse(segs[1]);

            var id = ulong.Parse(segs[0].Split('!').Last().Trim().TrimEnd('>'));
            return Eval($"eval td{id} g{amt}", user);
        }

        [Handler("move")]
        public static string MoveCommand(string cmd, SocketUser user)
        {
            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            var dir = Enum.Parse<Direction>(segs[0], true);
            return Eval("eval " + dir, user);
        }

        [Handler("join")]
        public static string JoinWorld(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";

            Vector3F FindFreeSpot()
            {
                startover:
                var vec = new Vector3F()
                {
                    X = (float) _rng.Next(Program.WorldState.WorldSize),
                    Y = (float) _rng.Next(Program.WorldState.WorldSize),
                    Z = 0f
                };

                if (Program.WorldState.Players.Any(x =>
                    (int) x.Position.X == (int) vec.X && (int) x.Position.Y == (int) vec.Y))
                {
                    goto startover;
                }

                foreach (var statePlayer in Program.WorldState.Players)
                {
                    if (statePlayer.Position.DistanceTo(vec) <= 4)
                    {
                        goto startover;
                    }
                }

                return vec;
            }

            if (Program.WorldState.Players.Any(x => x.DiscordID == user.Id)) return "You have already joined.";

            var player = new Player()
            {
                Name = user.Username,
                DiscordID = user.Id,
                ProfilePicture = user.GetAvatarUrl(),
                Position = FindFreeSpot(),
                Tokens = Program.WorldState.StartTokens,
                Health = Program.WorldState.StartHealth,
                Range = Program.WorldState.StartRange
            };
            Program.WorldState.Players.Add(player);

            return "Cool your in!!";
        }

        [Handler("me")]
        public static string DumpMe(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";
            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the game yet!";
            }

            var p = Program.WorldState.Players.First(x => x.DiscordID == user.Id);


            var sb = new StringBuilder();

            sb.AppendLine(p.Name + ":");
            sb.AppendLine($"Position: [X: {p.Position.X};Y: {p.Position.Y}]");
            sb.AppendLine($"Tokens: {p.Tokens}");
            sb.AppendLine($"Health: {p.Health}");
            sb.AppendLine($"Range: {p.Range}");
            sb.AppendLine($"Votes: {p.VoteCount}");


            return sb.ToString();
        }

        [Handler("upgrade")]
        public static string UpgradeRange(string cmd, SocketUser user)
        {
            return Eval($"eval u", user);
        }

        [Handler("eval")]
        public static string Eval(string cmd, SocketUser user)
        {
            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the game yet!";
            }

            var p = Program.WorldState.Players.First(x => x.DiscordID == user.Id);


            return GCodeEngine.Eval(p, Program.WorldState, cmd.Remove(0, 4));
        }

        [Handler("heal")]
        public static string UpgradeHp(string cmd, SocketUser user)
        {
            return Eval($"eval h", user);
        }
    }
}