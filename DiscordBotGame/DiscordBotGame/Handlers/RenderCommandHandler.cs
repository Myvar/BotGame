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
            using (var compus = Bitmap.FromFile(Path.GetFullPath("./dir.png")))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.Black);

                

                var scale = 0.2f;

                g.ScaleTransform(1f - scale, 1f - scale);
                g.TranslateTransform((img.Width * scale) / 2f, (img.Height * scale) / 2f);

                var worldSize = Program.WorldState.WorldSize;
                var padding = 2;
                var cellSize = img.Width / (worldSize + padding);


                for (int x = 0; x < (img.Width / cellSize) - 1; x++)
                {
                    for (int y = 0; y < (img.Width / cellSize) - 1; y++)
                    {
                        g.FillRectangle(Brushes.White,
                            (x * (cellSize + padding)) - (cellSize / 2f),
                            (y * (cellSize + padding)) - (cellSize / 2f),
                            cellSize, cellSize);
                    }
                }
                g.DrawImage(compus, img.Width - 200, img.Height - 200, 250, 250);
                //one sec mic is muted
                foreach (var player in Program.WorldState.Players)
                {
                    if (player.Dead) continue;
                    using (var stream = _Client.OpenRead(player.ProfilePicture))
                    using (var bitmap = new Bitmap(stream))
                    {
                        var p = new GraphicsPath();
                        p.AddEllipse((player.Position.X * (cellSize + padding)) - (cellSize / 2f),
                            (player.Position.Y * (cellSize + padding)) - (cellSize / 2f),
                            cellSize, cellSize);

                        g.SetClip(p);
                        g.DrawImage(bitmap,
                            (player.Position.X * (cellSize + padding)) - (cellSize / 2f),
                            (player.Position.Y * (cellSize + padding)) - (cellSize / 2f),
                            cellSize, cellSize);
                        g.DrawEllipse(new Pen(Color.DarkCyan, 10),
                            (player.Position.X * (cellSize + padding)) - (cellSize / 2f),
                            (player.Position.Y * (cellSize + padding)) - (cellSize / 2f),
                            cellSize, cellSize);
                        g.ResetClip();

                        g.FillRectangle(
                            new SolidBrush(Color.FromArgb(100, Color.Black)),
                            (player.Position.X * (cellSize + padding)) - (cellSize / 2f),
                            (player.Position.Y * (cellSize + padding)) - (cellSize / 2f),
                            cellSize, cellSize);
                        g.DrawString(player.Health.ToString(), new Font(FontFamily.GenericMonospace, 18),
                            new SolidBrush(Color.Red), 2 + (player.Position.X * (cellSize + padding)) - (cellSize / 2f),
                            (player.Position.Y * (cellSize + padding)) - (cellSize / 2f));

                        g.DrawString(player.Tokens.ToString(), new Font(FontFamily.GenericMonospace, 18),
                            new SolidBrush(Color.Yellow),
                            2 + (player.Position.X * (cellSize + padding)) - (cellSize / 2f),
                            (player.Position.Y * (cellSize + padding)));
                        
                        
                    }
                }

                return img;
            }
        }
    }
}