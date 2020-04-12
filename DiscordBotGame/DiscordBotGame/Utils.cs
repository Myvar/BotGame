using System.Collections.Generic;
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
    }
}