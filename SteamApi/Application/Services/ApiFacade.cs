using SteamApi.Application.Common;

namespace SteamApi.Application.Services
{
    public interface IApiFacade
    {
        Task<Result<object>> GetGames(string month, string[]? platform, string[]? tag, CancellationToken ct);
        Task<Result<object>> GetCalendar(string month, string[]? platform, string[]? tag, CancellationToken ct);
        Task<Result<object>> GetTopGenres(string month, int top, CancellationToken ct);
        Task<Result<object>> GetDynamics(CancellationToken ct);
        Task<Result<object>> Sync(string month, CancellationToken ct);
    }

    public class ApiFacade : IApiFacade
    {
        private readonly IGameService _games;
        private readonly IAnalyticsService _analytics;
        private readonly ISteamSyncService _sync;

        public ApiFacade(IGameService games, IAnalyticsService analytics, ISteamSyncService sync)
        {
            _games = games;
            _analytics = analytics;
            _sync = sync;
        }

        public async Task<Result<object>> GetGames(string month, string[]? platform, string[]? tag, CancellationToken ct)
        {
            if (!DateOnly.TryParse(month, out var m)) return Result<object>.Fail("month must be YYYY-MM");
            var q = new GamesQuery(new DateOnly(m.Year, m.Month, 1), platform, tag);
            var list = await _games.GetGamesForMonthAsync(q, ct);
            return Result<object>.Ok(list);
        }

        public async Task<Result<object>> GetCalendar(string month, string[]? platform, string[]? tag, CancellationToken ct)
        {
            if (!DateOnly.TryParse(month, out var m)) return Result<object>.Fail("month must be YYYY-MM");
            var q = new GamesQuery(new DateOnly(m.Year, m.Month, 1), platform, tag);
            var dict = await _games.GetCalendarAsync(q, ct);
            var days = dict.OrderBy(kv => kv.Key).Select(kv => new { date = kv.Key.ToString("yyyy-MM-dd"), count = kv.Value });
            return Result<object>.Ok(new { month = new DateOnly(m.Year, m.Month, 1).ToString("yyyy-MM"), days });
        }

        public async Task<Result<object>> GetTopGenres(string month, int top, CancellationToken ct)
        {
            if (!DateOnly.TryParse(month, out var m)) return Result<object>.Fail("month must be YYYY-MM");
            var data = await _analytics.GetTopGenresAsync(new DateOnly(m.Year, m.Month, 1), top, ct);
            return Result<object>.Ok(data.Select(x => new { genre = x.Genre, games = x.Games, avgFollowers = (int)Math.Round(x.AvgFollowers) }));
        }

        public async Task<Result<object>> GetDynamics(CancellationToken ct)
        {
            var data = await _analytics.GetDynamicsAsync(ct);
            return Result<object>.Ok(data);
        }

        public async Task<Result<object>> Sync(string month, CancellationToken ct)
        {
            if (!DateOnly.TryParse(month, out var m)) return Result<object>.Fail("month must be YYYY-MM");
            var added = await _sync.SyncUpcomingAsync(new DateOnly(m.Year, m.Month, 1), ct);
            return Result<object>.Ok(new { added });
        }
    }
}


