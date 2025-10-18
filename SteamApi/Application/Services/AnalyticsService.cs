using Microsoft.EntityFrameworkCore;
using SteamApi.Infrastructure;

namespace SteamApi.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _db;
        private readonly IClickHouseWriter _ch;

        public AnalyticsService(AppDbContext db, IClickHouseWriter ch)
        {
            _db = db;
            _ch = ch;
        }

        public async Task<IReadOnlyList<GenreAgg>> GetTopGenresAsync(DateOnly month, int top, CancellationToken ct)
        {
            try
            {
                // Сначала пытаемся получить данные из ClickHouse
                var start = new DateOnly(month.Year, month.Month, 1);
                var end = start.AddMonths(1);
                
                var startDate = start.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
                var endDate = end.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
                
                // Получаем данные из ClickHouse
                Console.WriteLine($"Getting top genres from ClickHouse for {start} to {end}");
                var clickHouseResults = await _ch.GetTopGenresAsync(start, end, top, ct);
                Console.WriteLine($"ClickHouse returned {clickHouseResults.Count()} results");
                if (clickHouseResults.Any())
                {
                    return clickHouseResults.ToList();
                }
                
                // Fallback к PostgreSQL если ClickHouse пуст
                var query = _db.GameTags
                    .AsNoTracking()
                    .Where(gt => gt.Game.ReleaseDate != null && gt.Game.ReleaseDate >= start && gt.Game.ReleaseDate < end)
                    .GroupBy(gt => gt.Tag.Name)
                    .Select(g => new GenreAgg(
                        g.Key,
                        g.Count(),
                        g.Average(x => (double)(x.Game.Followers ?? 0))
                    ))
                    .OrderByDescending(x => x.Games)
                    .ThenByDescending(x => x.AvgFollowers)
                    .Take(top);

                return await query.ToListAsync(ct);
            }
            catch (Exception ex)
            {
                // Логируем ошибку для диагностики
                Console.WriteLine($"Error in GetTopGenresAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Возвращаем пустой список вместо исключения
                return new List<GenreAgg>();
            }
        }

        public async Task<object> GetDynamicsAsync(CancellationToken ct)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startMonth = new DateOnly(now.AddMonths(-2).Year, now.AddMonths(-2).Month, 1);
                var endMonth = new DateOnly(now.Year, now.Month, 1);
                
                // Чтение динамики из ClickHouse
                var dynamicsData = await _ch.GetGenreDynamicsAsync(startMonth, endMonth, ct);
                
                var months = new List<string>();
                var current = startMonth;
                while (current <= endMonth)
                {
                    months.Add(current.ToString("yyyy-MM"));
                    current = current.AddMonths(1);
                }
                
                // Группировка по жанрам
                var genres = dynamicsData
                    .GroupBy(x => x.Genre)
                    .OrderByDescending(g => g.Sum(x => x.GamesCount))
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList();
                
                var result = new
                {
                    months = months.ToArray(),
                    genres = genres.Select(genre => new
                    {
                        genre,
                        counts = months.Select(month => 
                            dynamicsData.FirstOrDefault(d => d.Genre == genre && d.Month == month)?.GamesCount ?? 0).ToArray(),
                        avgFollowers = months.Select(month => 
                            (int)Math.Round(dynamicsData.FirstOrDefault(d => d.Genre == genre && d.Month == month)?.AvgFollowers ?? 0)).ToArray()
                    })
                };
                
                return result;
            }
            catch (Exception ex)
            {
                // Если ClickHouse недоступен, возвращаем пустые данные
                Console.WriteLine($"Error in GetDynamicsAsync: {ex.Message}");
                
                var now = DateTime.UtcNow;
                var startMonth = new DateOnly(now.AddMonths(-2).Year, now.AddMonths(-2).Month, 1);
                var endMonth = new DateOnly(now.Year, now.Month, 1);
                
                var months = new List<string>();
                var current = startMonth;
                while (current <= endMonth)
                {
                    months.Add(current.ToString("yyyy-MM"));
                    current = current.AddMonths(1);
                }
                
                return new
                {
                    months = months.ToArray(),
                    genres = new object[0]
                };
            }
        }
    }
}


