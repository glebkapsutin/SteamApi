using SteamApi.Domain;
using SteamApi.Application.DTOs;

namespace SteamApi.Application.Services
{
    public record GamesQuery(DateOnly Month, string[]? Platforms, string[]? Tags);

    public interface IGameService
    {
        Task<IReadOnlyList<GameDto>> GetGamesForMonthAsync(GamesQuery query, CancellationToken ct);
        Task<IDictionary<DateOnly, int>> GetCalendarAsync(GamesQuery query, CancellationToken ct);
    }

    public interface ISteamSyncService
    {
        Task<int> SyncUpcomingAsync(DateOnly month, CancellationToken ct);
    }

    public record GenreAgg(string Genre, int Games, double AvgFollowers);

    public interface IAnalyticsService
    {
        Task<IReadOnlyList<GenreAgg>> GetTopGenresAsync(DateOnly month, int top, CancellationToken ct);
        Task<object> GetDynamicsAsync(CancellationToken ct);
    }
}


