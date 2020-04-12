using System;
using System.Reflection;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBotGame
{
    public static class GameBotEngine
    {
        public static Dictionary<string, Func<string, SocketUser, Bitmap>> ImageHandlers { get; set; } =
            new Dictionary<string, Func<string, SocketUser, Bitmap>>();

        public static Dictionary<string, Func<string, SocketUser, string>> TextHandlers { get; set; } =
            new Dictionary<string, Func<string, SocketUser, string>>();

        static GameBotEngine()
        {
            foreach (var type in typeof(GameBotEngine).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<HandlerAttribute>() != null)
                {
                    foreach (var method in type.GetMethods())
                    {
                        var att = method.GetCustomAttribute<HandlerAttribute>();

                        if (att != null)
                        {
                            if (!method.IsStatic)
                            {
                                Logger.Error($"The Method {method.Name} is not static");
                                continue;
                            }

                            if (method.ReturnType == typeof(string))
                            {
                                TextHandlers.Add(att.Cmd, (x, u) =>
                                {
                                    return (string) method.Invoke(null, BindingFlags.Static, null, new object[] {x, u},
                                        CultureInfo.CurrentCulture);
                                });
                            }
                            else if (method.ReturnType == typeof(Bitmap))
                            {
                                ImageHandlers.Add(att.Cmd, (x, u) =>
                                {
                                    return (Bitmap) method.Invoke(null, BindingFlags.Static, null, new object[] {x, u},
                                        CultureInfo.CurrentCulture);
                                });
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateWorld()
        {
            var missedCycles = (int) ((DateTime.Now - Program.WorldState.LastUpdateTimeStamp) /
                                      Program.WorldState.TokenHandOutInterval);
            if (missedCycles > 0)
            {
                Program.WorldState.LastUpdateTimeStamp = DateTime.Now;
                //total cycles since last update
                //say no one has sad anything on the server for days
                //then if we dont update we will miss meany cycles 
                //to avoid this we devide the elipsed time with the hand out interval to get the total cycles since the last update
                //then we multiply the andouts acording to that

                var biggerPlayer = Program.WorldState.Players[0];

                foreach (var player in Program.WorldState.Players)
                {
                    if (player.VoteCount > biggerPlayer.VoteCount) biggerPlayer = player;

                    player.Tokens += missedCycles * Program.WorldState.TokensPerCycle;
                  
                    player.DeadVoteCast = false;
                    if (player.Tokens > 10) player.Tokens = 10;
                }
                var sb = new StringBuilder();
                if (biggerPlayer.VoteCount > 0)
                {
                    biggerPlayer.Tokens += 1;
                    sb.AppendLine($"<@!{biggerPlayer.DiscordID}> Wone the vote");
                }

                foreach (var player in Program.WorldState.Players)
                {
                    player.VoteCount = 0;
                }

                sb.AppendLine("A new Cycle has Completed and tokens dished out");
                foreach (var player in Program.WorldState.Players)
                {
                    sb.Append($"<@!{player.DiscordID}>");
                }


                Program.Chanel.SendMessageAsync(sb.ToString()).Wait();
            }
        }

        public static (string msg, Bitmap img) HandelCommand(string cmd, SocketUser user)
        {
            if (!cmd.StartsWith("~"))
            {
                return (null, null);
            }

            
            UpdateWorld(); //every time a message of any kind command or not i sent in normal chat on the server tick the update
            File.WriteAllText("state.json", JsonConvert.SerializeObject(Program.WorldState, Formatting.Indented));

            cmd = cmd.Remove(0, 1);

            var segs = Utils.ParseCmd(cmd);

            var name = segs[0];

            if (TextHandlers.ContainsKey(name))
            {
                return (TextHandlers[name](cmd, user), null);
            }

            else if (ImageHandlers.ContainsKey(name))
            {
                return ("", ImageHandlers[name](cmd, user));
            }
            else
            {
                return ("404 Handler Not Found", null);
            }
        }
    }
}