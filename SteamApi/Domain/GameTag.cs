namespace SteamApi.Domain
{
    public class GameTag
    {
        public Guid GameId { get; set; }
        public Game Game { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}


