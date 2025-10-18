namespace SteamApi.Application.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? SteamAppId { get; set; }
        public string? ReleaseDate { get; set; }
        public int? Followers { get; set; }
        public string? StoreUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? ShortDescription { get; set; }
        public bool Windows { get; set; }
        public bool Mac { get; set; }
        public bool Linux { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
