namespace DiscordBotGame
{
    public class Player
    {
        public Vector3F Position { get; set; }
        public string Name { get; set; }
        public bool Dead { get; set; }
        public bool DeadVoteCast { get; set; }
        public string ProfilePicture { get; set; }
        public ulong DiscordID { get; set; }

        public int Tokens { get; set; }
        public int Health { get; set; }
        public int Range { get; set; }
        public int VoteCount { get; set; }
    }
}