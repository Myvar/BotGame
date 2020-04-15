using System;
using System.Collections.Generic;
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
                // The curve's parametric equations.
                float X(float t)
                {
                    double sin_t = Math.Sin(t);
                    return (float) (16 * sin_t * sin_t * sin_t);
                }

                float Y(float t)
                {
                    return (float) (
                        13 * Math.Cos(t) -
                        5 * Math.Cos(2 * t) -
                        2 * Math.Cos(3 * t) -
                        Math.Cos(4 * t));
                }

                void SetTransformation(Graphics gr,
                    RectangleF world_rect, RectangleF device_rect,
                    bool invert_x, bool invert_y)
                {
                    PointF[] device_points =
                    {
                        // Upper left.
                        new PointF(device_rect.Left, device_rect.Top),
                        // Upper right.
                        new PointF(device_rect.Right, device_rect.Top),
                        // Lower left.
                        new PointF(device_rect.Left, device_rect.Bottom),
                    };

                    if (invert_x)
                        for (int i = 0; i < 3; i++)
                        {
                            device_points[0].X = device_rect.Right;
                            device_points[1].X = device_rect.Left;
                            device_points[2].X = device_rect.Right;
                        }

                    if (invert_y)
                    {
                        device_points[0].Y = device_rect.Bottom;
                        device_points[1].Y = device_rect.Bottom;
                        device_points[2].Y = device_rect.Top;
                    }

                    gr.Transform = new Matrix(world_rect, device_points);
                }

                void SetTransformationWithoutDisortion(Graphics gr,
                    RectangleF world_rect, RectangleF device_rect,
                    bool invert_x, bool invert_y)
                {
                    // Get the aspect ratios.
                    float world_aspect = world_rect.Width / world_rect.Height;
                    float device_aspect = device_rect.Width / device_rect.Height;

                    // Asjust the world rectangle to maintain the aspect ratio.
                    float world_cx = world_rect.X + world_rect.Width / 2f;
                    float world_cy = world_rect.Y + world_rect.Height / 2f;
                    if (world_aspect > device_aspect)
                    {
                        // The world coordinates are too short and width.
                        // Make them taller.
                        float world_height = world_rect.Width / device_aspect;
                        world_rect = new RectangleF(
                            world_rect.Left,
                            world_cy - world_height / 2f,
                            world_rect.Width,
                            world_height);
                    }
                    else
                    {
                        // The world coordinates are too tall and thin.
                        // Make them wider.
                        float world_width = device_aspect * world_rect.Height;
                        world_rect = new RectangleF(
                            world_cx - world_width / 2f,
                            world_rect.Top,
                            world_width,
                            world_rect.Height);
                    }

                    // Map the new world coordinates into the device coordinates.
                    SetTransformation(gr, world_rect, device_rect,
                        invert_x, invert_y);
                }

                void drawHeart(float x, float y, float w, float h)
                {
                    // Generate the points.
                    const int num_points = 100;
                    var points = new List<PointF>();
                    float dt = (float) (2 * Math.PI / num_points);
                    for (float t = 0; t <= 2 * Math.PI; t += dt)
                        points.Add(new PointF(X(t), Y(t)));

                    // Get the coordinate bounds.
                    float wxmin = points[0].X;
                    float wxmax = wxmin;
                    float wymin = points[0].Y;
                    float wymax = wymin;
                    foreach (PointF point in points)
                    {
                        if (wxmin > point.X) wxmin = point.X;
                        if (wxmax < point.X) wxmax = point.X;
                        if (wymin > point.Y) wymin = point.Y;
                        if (wymax < point.Y) wymax = point.Y;
                    }

                    // Make the world coordinate rectangle.
                    RectangleF world_rect = new RectangleF(
                        wxmin, wymin, wxmax - wxmin, wymax - wymin);

                    // Make the device coordinate rectangle with a margin.
                    const int margin = 5;
                    RectangleF device_rect = new RectangleF(
                        margin, margin,
                        w - 2 * margin,
                        h - 2 * margin);


                    SetTransformationWithoutDisortion(g,
                        world_rect, device_rect, false, true);
                    // Draw the curve.
                    g.TranslateTransform(x, y, MatrixOrder.Append);
                    g.FillPolygon(Brushes.Red, points.ToArray());
                    g.DrawPolygon(Pens.Pink, points.ToArray());

                    g.ResetTransform();
                }

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

                var biggerPlayer = Program.WorldState.Players[0];


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

                        /*g.DrawString(player.Health.ToString(), new Font(FontFamily.GenericMonospace, fontStatsSize),
                            new SolidBrush(Color.Red),
                            2 + player.Position.X * cellSize,
                            player.Position.Y * cellSize);*/

                        var heartSize = cellSize / Program.WorldState.StartHealth;
                        for (float i = 0; i < player.Health; i++)
                        {
                            drawHeart((player.Position.X * cellSize) + (heartSize * i),
                                (player.Position.Y * cellSize) + 2, heartSize, heartSize);
                        }

                        var f = new Font(FontFamily.GenericMonospace, fontStatsSize);

                        g.DrawString(player.Tokens.ToString(), f,
                            new SolidBrush(Color.Yellow),
                            2 + player.Position.X * cellSize,
                            (cellSize / 2) + player.Position.Y * cellSize);

                        var txtSize = g.MeasureString(player.VoteCount.ToString(), f);
                        g.DrawString(player.VoteCount.ToString(), f,
                            new SolidBrush(Color.Cyan),
                            2f + ((player.Position.X * cellSize) + (cellSize)) - txtSize.Width - 2,
                            (cellSize / 2) + (player.Position.Y * cellSize));


                        if (DateTime.Now - player.AttackTimeCoolDown < TimeSpan.FromHours(4))
                        {
                            g.DrawRectangle(new Pen(Color.Red, 6f),
                                player.Position.X * cellSize,
                                player.Position.Y * cellSize,
                                cellSize,
                                cellSize);
                        }
                        
                        if (player.VoteCount > biggerPlayer.VoteCount) biggerPlayer = player;
                    }
                }

                if (biggerPlayer.VoteCount > 0)
                    g.DrawRectangle(new Pen(Color.Orange, 5f),
                        biggerPlayer.Position.X * cellSize,
                        biggerPlayer.Position.Y * cellSize,
                        cellSize,
                        cellSize);

                g.DrawImage(compus, (img.Width - compus.Width) / 2f, (img.Height - compus.Height) / 2f);

                return img;
            }
        }
    }
}