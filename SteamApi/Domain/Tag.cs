using System.ComponentModel.DataAnnotations;

namespace SteamApi.Domain
{
    public class Tag
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
    }
}


