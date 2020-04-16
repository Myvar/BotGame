using System;

namespace DiscordBotGame.Instructions
{
    public class InstructionHandlerAttribute : Attribute
    {
        public InstructionHandlerAttribute(Opcode opcode)
        {
            Opcode = opcode;
        }

        public Opcode Opcode { get; set; }
    }
}