using System;
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

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;
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
                    await message.Channel.SendFileAsync(mem, "map.png", msg);
                }
            }
        }
    }
}