using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using Discord.WebSocket;

namespace DiscordBotGame
{
    [Handler]
    public static class RenderCommandHandler
    {
        public static WebClient _Client = new WebClient();

        [Handler("map")]
        public static Bitmap ShowMap(string cmd, SocketUser user)
        {
            if (!Program.WorldState.GameStarted) return null;
            var img = new Bitmap(1024, 1024);
            using (var g = Graphics.FromImage(img))
            using (var compus = Bitmap.FromFile(Path.GetFullPath("./face.png")))
            {
               // g.SmoothingMode = SmoothingMode.HighQuality;

                g.Clear(Color.White);

                var worldSize = (float) Program.WorldState.WorldSize;

                var cellSize = img.Width / (worldSize);

                var xOffset = 0f;

                var fontIndexSize = (cellSize / 3f) * 72f / g.DpiX;

                for (int x = 0; x < worldSize; x++)
                {
                    g.DrawLine(new Pen(Color.Black, 2f), xOffset, 0, xOffset, img.Height);


                    g.DrawString(x.ToString(), new Font(FontFamily.GenericMonospace, fontIndexSize),
                        new SolidBrush(Color.Gray),
                        xOffset + 2,
                        2);

                    xOffset += cellSize;
                }

                var yOffset = 0f;

                for (int x = 0; x < worldSize; x++)
                {
                    g.DrawLine(new Pen(Color.Black, 2f), 0, yOffset, img.Width, yOffset);


                    if (x > 0)
                        g.DrawString(x.ToString(), new Font(FontFamily.GenericMonospace, fontIndexSize),
                            new SolidBrush(Color.Gray),
                            2,
                            yOffset + 2);

                    yOffset += cellSize;
                }


               
                
                foreach (var player in Program.WorldState.Players)
                {
                    if (player.Dead) continue;

                    using (var stream = _Client.OpenRead(player.ProfilePicture))
                    using (var bitmap = new Bitmap(stream))
                    {
                        g.DrawImage(bitmap, player.Position.X * cellSize,
                            player.Position.Y * cellSize,
                            cellSize,
                            cellSize);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(70, Color.Black)),
                            player.Position.X * cellSize,
                            player.Position.Y * cellSize,
                            cellSize,
                            cellSize);

                        var fontStatsSize = (cellSize / 2f) * 72f / g.DpiX;

                        g.DrawString(player.Health.ToString(), new Font(FontFamily.GenericMonospace, fontStatsSize),
                            new SolidBrush(Color.Red),
                            2 + player.Position.X * cellSize,
                            player.Position.Y * cellSize);

                        g.DrawString(player.Tokens.ToString(), new Font(FontFamily.GenericMonospace, fontStatsSize),
                            new SolidBrush(Color.Yellow),
                            2 + player.Position.X * cellSize,
                            (cellSize / 2) + player.Position.Y * cellSize);
                    }
                }
                g.DrawImage(compus, (img.Width - compus.Width) / 2f, (img.Height - compus.Height) / 2f);

                return img;
            }
        }
    }
}