using Microsoft.EntityFrameworkCore;
using SteamApi.Infrastructure;
using SteamApi.Application.DTOs;

namespace SteamApi.Application.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;

        public GameService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<GameDto>> GetGamesForMonthAsync(GamesQuery query, CancellationToken ct)
        {
            try
            {
                var start = new DateOnly(query.Month.Year, query.Month.Month, 1);
                var end = start.AddMonths(1);

                var platformFilter = query.Platforms?.Select(p => p.ToLowerInvariant()).ToHashSet();
                var tagFilter = query.Tags?.Select(t => t.ToLowerInvariant()).ToHashSet();

                var q = _db.Games
                    .AsNoTracking()
                    .Include(g => g.GameTags).ThenInclude(gt => gt.Tag)
                    .Where(g => g.ReleaseDate != null && g.ReleaseDate >= start && g.ReleaseDate < end);

                if (platformFilter != null && platformFilter.Count > 0)
                {
                    q = q.Where(g =>
                        (platformFilter.Contains("windows") && g.Windows) ||
                        (platformFilter.Contains("mac") && g.Mac) ||
                        (platformFilter.Contains("linux") && g.Linux));
                }

                if (tagFilter != null && tagFilter.Count > 0)
                {
                    q = q.Where(g => g.GameTags.Any(gt => tagFilter.Contains(gt.Tag.Name.ToLower())));
                }

                var games = await q.OrderBy(g => g.ReleaseDate).ToListAsync(ct);
                
                return games.Select(g => new GameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    SteamAppId = g.SteamAppId,
                    ReleaseDate = g.ReleaseDate?.ToString("yyyy-MM-dd"),
                    Followers = g.Followers,
                    StoreUrl = g.StoreUrl,
                    ImageUrl = g.ImageUrl,
                    ShortDescription = g.ShortDescription,
                    Windows = g.Windows,
                    Mac = g.Mac,
                    Linux = g.Linux,
                    Tags = g.GameTags.Select(gt => gt.Tag.Name).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                // Логируем ошибку для диагностики
                Console.WriteLine($"Error in GetGamesForMonthAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IDictionary<DateOnly, int>> GetCalendarAsync(GamesQuery query, CancellationToken ct)
        {
            var start = new DateOnly(query.Month.Year, query.Month.Month, 1);
            var end = start.AddMonths(1);

            var platformFilter = query.Platforms?.Select(p => p.ToLowerInvariant()).ToHashSet();
            var tagFilter = query.Tags?.Select(t => t.ToLowerInvariant()).ToHashSet();

            var q = _db.Games
                .AsNoTracking()
                .Include(g => g.GameTags).ThenInclude(gt => gt.Tag)
                .Where(g => g.ReleaseDate != null && g.ReleaseDate >= start && g.ReleaseDate < end);

            if (platformFilter != null && platformFilter.Count > 0)
            {
                q = q.Where(g =>
                    (platformFilter.Contains("windows") && g.Windows) ||
                    (platformFilter.Contains("mac") && g.Mac) ||
                    (platformFilter.Contains("linux") && g.Linux));
            }

            if (tagFilter != null && tagFilter.Count > 0)
            {
                q = q.Where(g => g.GameTags.Any(gt => tagFilter.Contains(gt.Tag.Name.ToLower())));
            }

            var games = await q.OrderBy(g => g.ReleaseDate).ToListAsync(ct);
            return games
                .GroupBy(g => g.ReleaseDate!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}


