using Microsoft.EntityFrameworkCore;
using SteamApi.Infrastructure;

namespace SteamApi.Application.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;

        public GameService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Domain.Game>> GetGamesForMonthAsync(GamesQuery query, CancellationToken ct)
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

            return await q.OrderBy(g => g.ReleaseDate).ToListAsync(ct);
        }

        public async Task<IDictionary<DateOnly, int>> GetCalendarAsync(GamesQuery query, CancellationToken ct)
        {
            var games = await GetGamesForMonthAsync(query, ct);
            return games
                .GroupBy(g => g.ReleaseDate!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}


