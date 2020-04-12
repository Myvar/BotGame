using System;

namespace DiscordBotGame
{
    [AttributeUsage(AttributeTargets.All)]
    public class HandlerAttribute : Attribute
    {
        public HandlerAttribute(string cmd)
        {
            Cmd = cmd;
        }

        public HandlerAttribute()
        {
            
        }

        public string Cmd { get; set; }
    }
}