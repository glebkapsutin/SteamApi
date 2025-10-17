using System.ComponentModel.DataAnnotations;

namespace SteamApi.Domain
{
    public class Game
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public int? SteamAppId { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public int? Followers { get; set; }
        public string? StoreUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? ShortDescription { get; set; }
        public bool Windows { get; set; }
        public bool Mac { get; set; }
        public bool Linux { get; set; }

        public ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
    }
}


