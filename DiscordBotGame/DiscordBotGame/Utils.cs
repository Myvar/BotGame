using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DiscordBotGame
{
    public static class Utils
    {
        public static List<string> ParseCmd(string cmd)
        {
            var re = new List<string>();

            cmd = " " + cmd + " ";

            var sb = new StringBuilder();

            bool flag = false;

            for (int i = 1; i < cmd.Length - 1; i++)
            {
                var b = cmd[i - 1];
                var c = cmd[i];
                var a = cmd[i + 1];

                if (flag)
                {
                    sb.Append(c);
                    if (c == '"' && b != '\\')
                    {
                        flag = false;
                        re.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        flag = true;
                    }
                    else if (c == ' ')
                    {
                        re.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }

                    sb.Append(c);
                }
            }

            re.Add(sb.ToString());
            return re;
        }

        public static string PadBoth(this string str, int length)
        {
            int spaces = length - str.Length;
            int padLeft = spaces / 2 + str.Length;
            return str.PadLeft(padLeft).PadRight(length);
        }
        
        public static string ToTable(List<List<string>> data)
        {
            var sb = new StringBuilder();

            var maxWidth = new List<int>();

            for (int i = 0; i < data[0].Count; i++)
            {
                maxWidth.Add(0);
            }

            foreach (var d in data)
            {
                for (var i = 0; i < d.Count; i++)
                {
                    var x = d[i];

                    if (x.Length > maxWidth[i]) maxWidth[i] = x.Length;
                }
            }

            for (var index = 0; index < maxWidth.Count; index++)
            {
                maxWidth[index] += 4;
            }

            foreach (var d in data)
            {
                for (var i = 0; i < d.Count; i++)
                {
                    var x = d[i];
                    d[i] = x.PadBoth((maxWidth[i] ));
                }
            }

            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];

                foreach (var x in d)
                {
                    sb.Append(x + "|");
                }

                if (i == 0)
                {
                    sb.AppendLine();
                    sb.Append("".PadLeft(maxWidth.Sum() + maxWidth.Count, '-'));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}