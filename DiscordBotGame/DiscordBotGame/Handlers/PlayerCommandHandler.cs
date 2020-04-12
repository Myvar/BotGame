using System;
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
                "This bot is not ment for general public use and only for con-fuzies, for more help ask Myvar");
            sb.AppendLine("Here is a list of commands:");

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
            if (Program.WorldState.AdminID != user.Id) return "Imposter Alert, your no admin, release the worms";

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
            var sb = new StringBuilder();
            sb.AppendLine("Players");
            foreach (var player in Program.WorldState.Players)
            {
                sb.AppendLine(
                    $"@{player.Name}: [Tokens: {player.Tokens}; HP: {player.Health}; Votes: {player.VoteCount}]");
            }

            return sb.ToString();
        }

        [Handler("attack")]
        public static string Attack(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";
            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            var id = ulong.Parse(segs[0].Split('!').Last().Trim().TrimEnd('>'));
            if (Program.WorldState.Players.All(x => x.DiscordID != id))
            {
                return $"The Target <@!{user.Id}> has not joined the games!";
            }

            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the games!";
            }

            var target = Program.WorldState.Players.First(x => x.DiscordID == id);
            var self = Program.WorldState.Players.First(x => x.DiscordID == user.Id);

            if (self.Dead) return "Your dead, dead people cant do stuff";
            if (target.Dead) return "You sick basterd attacking a dead person";

            if (self.DiscordID == target.DiscordID)
                return
                    "There is help for you: http://www.sadag.org/index.php?option=com_content&view=article&id=1904&Itemid=151";

            if (self.Tokens < 1)
                return "Your out of go go juice, might wane talk to the doc about some blue ones.";

            if ((int) Math.Truncate(target.Position.DistanceTo(self.Position)) <= self.Range)
            {
                target.Health -= 1;
                self.Tokens -= 1;

                if (target.Health < 1)
                {
                    target.Dead = true;
                }

                return "Mom is on speed dial, Safety Squints Engaged, a pray and a hope for luck, FIRE";
            }
            else
            {
                return
                    $"Your Target is {(int) Math.Truncate(target.Position.DistanceTo(self.Position)):F} units away and not in range your max range is: {self.Range}, Moron.";
            }
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
                return $"The Target <@!{user.Id}> has not joined the games!";
            }

            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the games!";
            }

            var target = Program.WorldState.Players.First(x => x.DiscordID == id);
            var self = Program.WorldState.Players.First(x => x.DiscordID == user.Id);

            if (!self.Dead) return "Only the dead can fuck with the living";
            if (target.Dead) return "Nice Try but voting for dead is old news.";


            if (self.DeadVoteCast)
                return "Voting twice will get you jail time.";


            self.DeadVoteCast = true;
            target.VoteCount += 1;

            return $"Cool you voted for <@!{target.DiscordID}>, shivers is running up his spine.";
        }

        [Handler("gift")]
        public static string Gift(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";
            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            if (segs.Count <= 1)
            {
                return "you need to tell me how meany bling to flash, ie: !gift <player> <amount>";
            }

            var amt = int.Parse(segs[1]);

            var id = ulong.Parse(segs[0].Split('!').Last().Trim().TrimEnd('>'));
            if (Program.WorldState.Players.All(x => x.DiscordID != id))
            {
                return $"The Target <@!{id}> has not joined the games!";
            }

            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the games!";
            }

            var target = Program.WorldState.Players.First(x => x.DiscordID == id);
            var self = Program.WorldState.Players.First(x => x.DiscordID == user.Id);

            if (self.Dead) return "Your dead, dead people cant do stuff";
            if (target.Dead) return "You sad sad dude gifting the dead";

            if (self.DiscordID == target.DiscordID)
                return
                    "If you dont have friend, to give you stuff might wane consider working on that first.";

            if (self.Tokens < amt)
                return "Your out of bling, cant give what you dont have";

            if ((int)Math.Truncate(target.Position.DistanceTo(self.Position)) <= self.Range)
            {
                target.Tokens += amt;
                self.Tokens -= amt;

                return $"You gave {amt} tokens to <@!{target.DiscordID}>";
            }
            else
            {
                return
                    $"Your Target is {(int)Math.Truncate(target.Position.DistanceTo(self.Position)):F} units away and not in range your max range is: {self.Range}, Moron.";
            }
        }

        [Handler("move")]
        public static string MoveCommand(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return "The game has not been started yet";
            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the games!";
            }

            var segs = Utils.ParseCmd(cmd);
            segs.RemoveAt(0);

            var dir = Enum.Parse<Direction>(segs[0], true);

            var p = Program.WorldState.Players.First(x => x.DiscordID == user.Id);
            if (p.Dead) return "Your dead, dead people cant do stuff";
            switch (dir)
            {
                case Direction.N:
                    if ((int) p.Position.Y == 0)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X && (int) x.Position.Y - 1 == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.Y -= 1;
                    p.Tokens -= 1;

                    break;
                case Direction.S:
                    if ((int) p.Position.Y == Program.WorldState.WorldSize)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X && (int) x.Position.Y + 1 == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.Y += 1;
                    p.Tokens -= 1;
                    break;
                case Direction.W:
                    if ((int) p.Position.X == 0)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X - 1 && (int) x.Position.Y == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.X -= 1;
                    p.Tokens -= 1;

                    break;
                case Direction.E:
                    if ((int) p.Position.X == Program.WorldState.WorldSize)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X + 1 && (int) x.Position.Y == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.X += 1;
                    p.Tokens -= 1;
                    break;
                case Direction.Ne:
                    if ((int) p.Position.X == Program.WorldState.WorldSize)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X + 1 && (int) x.Position.Y - 1 == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.X += 1;
                    p.Position.Y -= 1;
                    p.Tokens -= 1;
                    break;
                case Direction.Nw:
                    if ((int) p.Position.X == Program.WorldState.WorldSize)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X - 1 && (int) x.Position.Y - 1 == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.X -= 1;
                    p.Position.Y -= 1;
                    p.Tokens -= 1;
                    break;
                case Direction.Se:
                    if ((int) p.Position.X == Program.WorldState.WorldSize)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X - 1 && (int) x.Position.Y + 1 == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.X -= 1;
                    p.Position.Y += 1;
                    p.Tokens -= 1;
                    break;
                case Direction.Sw:
                    if ((int) p.Position.X == Program.WorldState.WorldSize)
                    {
                        return "Yea, that side of the map seems a bit dark and void like, just no.";
                    }

                    if (Program.WorldState.Players.Any(x =>
                        (int) x.Position.X == (int) p.Position.X + 1 && (int) x.Position.Y + 1 == (int) p.Position.Y))
                    {
                        return
                            "Another Player is already Occupying that space, if you want to attack use the attack command, otherwise use give";
                    }

                    if (p.Tokens < 1)
                    {
                        return "You do not have enuf tokens to move";
                    }

                    p.Position.X += 1;
                    p.Position.Y += 1;
                    p.Tokens -= 1;
                    break;
            }


            return $"Moving you {dir}";
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

            if (Program.WorldState.Players.Any(x => x.DiscordID == user.Id)) return "You have already joined";

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
                return $"The player <@!{user.Id}> has not joined the games!";
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
            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the games!";
            }

            var p = Program.WorldState.Players.First(x => x.DiscordID == user.Id);

            if (p.Tokens >= Program.WorldState.RangeUpgradeCost)
            {
                p.Tokens -= Program.WorldState.RangeUpgradeCost;
                p.Range += 1;
                return
                    $"Cool you paid {Program.WorldState.RangeUpgradeCost} tokens to upgrade your range with 1 to {p.Range}";
            }
            else
            {
                return "You cant afford it, poor Mother Fucker";
            }
        }
        
        [Handler("heal")]
        public static string UpgradeHp(string cmd, SocketUser user)
        {
            if (Program.WorldState.Players.All(x => x.DiscordID != user.Id))
            {
                return $"The player <@!{user.Id}> has not joined the games!";
            }

            var p = Program.WorldState.Players.First(x => x.DiscordID == user.Id);

            if (p.Tokens >= Program.WorldState.RangeUpgradeCost && p.Health < 3)
            {
                p.Tokens -= Program.WorldState.RangeUpgradeCost;
                p.Health += 1;
                return
                    $"Cool you paid {Program.WorldState.RangeUpgradeCost} for meds. Your HP:{p.Health}";
            }
            else
            {
                return "You cant afford it(or you already have 3 hp), poor Mother Fucker, die poor with out health";
            }
        }
    }
}