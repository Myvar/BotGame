using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Color = Discord.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace DiscordBotGame
{
    class Program
    {
        public static WorldState WorldState { get; set; } = new WorldState();

        public static DiscordSocketClient _client;

        // Discord.Net heavily utilizes TAP for async, so we create
        // an asynchronous context from the beginning.
        static void Main(string[] args)
        {
            if (File.Exists("state.json"))
            {
                WorldState = JsonConvert.DeserializeObject<WorldState>(File.ReadAllText("state.json"));
            }

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.ReactionAdded += ClientOnReactionAdded;
        }


        public async Task RunCmd(string cmd, IUser um, IUserMessage x)
        {
            (string msg, Bitmap img) = GameBotEngine.HandelCommand(cmd, um as SocketUser);

            if (msg == null && img == null) return;


            if (img == null)
            {
                await x.Channel.SendMessageAsync(msg);
            }
            else
            {
                using (var mem = new MemoryStream())
                {
                    img.Save(mem, ImageFormat.Png);
                    mem.Position = 0;
                    img.Dispose();
                    var messages = await x.Channel.GetMessagesAsync(50).FlattenAsync();

                    var map = messages.First();

                    if (map.Author.Id == _client.CurrentUser.Id && map.Attachments.Count != 0)
                    {
                        await map.DeleteAsync();
                    }
                    else
                    {
                        foreach (var m in messages)
                        {
                            if (m.Author.Id == _client.CurrentUser.Id)
                            {
                                if (m.Attachments.Count != 0)
                                {
                                    await m.DeleteAsync();
                                }
                            }
                        }
                    }

                    await x.DeleteAsync();

                    var res = await x.Channel.SendFileAsync(mem, "map.png", msg);

                    await res.AddReactionAsync(new Emoji("🌀"));
                }
            }
        }

        public async Task MainAsync()
        {
            // Tokens should be considered secret data, and never hard-coded.
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
            await _client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        public static ulong CurrentChanelID;
        public static ISocketMessageChannel Chanel;

        private async Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> x, ISocketMessageChannel chanel,
            SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;
            var um = await x.DownloadAsync();
            if (reaction.Emote.Name == new Emoji("🌀").Name)
            {
                if (um.Author.Id == _client.CurrentUser.Id && um.Attachments.Count != 0)
                {
                    await um.RemoveAllReactionsAsync();
                    await um.AddReactionAsync(new Emoji("⬅️"));
                    await um.AddReactionAsync(new Emoji("⬆️"));
                    await um.AddReactionAsync(new Emoji("➡️"));
                    await um.AddReactionAsync(new Emoji("⬇️"));

                    await um.AddReactionAsync(new Emoji("↖️"));
                    await um.AddReactionAsync(new Emoji("↗️"));
                    await um.AddReactionAsync(new Emoji("↘️"));
                    await um.AddReactionAsync(new Emoji("↙️"));
                }
            }
            else
            {
                await um.RemoveReactionAsync(reaction.Emote, um.Author);

                var dict = new Dictionary<string, string>()
                {
                    {new Emoji("⬅️️").Name, "#move w"},
                    {new Emoji("⬆").Name, "#move n"},
                    {new Emoji("➡️").Name, "#move e"},
                    {new Emoji("⬇️").Name, "#move s"},

                    {new Emoji("↖️").Name, "#move nw"},
                    {new Emoji("↗️").Name, "#move ne"},
                    {new Emoji("↘️").Name, "#move se"},
                    {new Emoji("↙️").Name, "#move sw"},
                };

                RunCmd(dict[reaction.Emote.Name], reaction.User.Value, um);
            }
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Channel is SocketDMChannel)
            {
                await message.Channel.SendMessageAsync("DMS are not aloud");
                return;
            }


            CurrentChanelID = message.Channel.Id;
            Chanel = message.Channel;
            (string msg, Bitmap img) = GameBotEngine.HandelCommand(message.Content, message.Author);

            if (msg == null && img == null) return;


            if (img == null)
            {
                await message.Channel.SendMessageAsync(msg);
            }
            else
            {
                using (var mem = new MemoryStream())
                {
                    img.Save(mem, ImageFormat.Png);
                    mem.Position = 0;
                    img.Dispose();
                    var messages = await message.Channel.GetMessagesAsync(10).FlattenAsync();

                    var map = messages.First();

                    if (map.Author.Id == _client.CurrentUser.Id && map.Attachments.Count != 0)
                    {
                        await map.DeleteAsync();
                    }
                    else
                    {
                        foreach (var m in messages)
                        {
                            if (m.Author.Id == _client.CurrentUser.Id)
                            {
                                if (m.Attachments.Count != 0)
                                {
                                    await m.DeleteAsync();
                                }
                            }
                        }
                    }

                    await message.DeleteAsync();

                    var res = await message.Channel.SendFileAsync(mem, "map.png", msg);

                    await res.AddReactionAsync(new Emoji("🌀"));
                }
            }
        }
    }
}