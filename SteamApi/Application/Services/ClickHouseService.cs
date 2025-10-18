using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using System.Data;

namespace SteamApi.Application.Services
{
    public interface IClickHouseWriter
    {
        Task EnsureSchemaAsync(CancellationToken ct);
        Task WriteSnapshotAsync(DateTime snapshotUtc, IEnumerable<ClickHouseGameRow> rows, CancellationToken ct);
        Task<IEnumerable<GenreDynamicsRow>> GetGenreDynamicsAsync(DateOnly startMonth, DateOnly endMonth, CancellationToken ct);
        Task<IEnumerable<GenreAgg>> GetTopGenresAsync(DateOnly startMonth, DateOnly endMonth, int top, CancellationToken ct);
    }

    public record ClickHouseGameRow(int AppId, string Name, string Genre, int Followers, DateTime ReleaseDateUtc);
    public record GenreDynamicsRow(string Genre, string Month, int GamesCount, double AvgFollowers);

    public class ClickHouseWriter : IClickHouseWriter
    {
        private readonly ClickHouseConnection _conn;

        public ClickHouseWriter(ClickHouseConnection conn)
        {
            _conn = conn;
        }

        public async Task EnsureSchemaAsync(CancellationToken ct)
        {
            await _conn.OpenAsync(ct);
            await using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS game_snapshots (
                snapshot_utc DateTime,
                app_id Int32,
                name String,
                genre String,
                followers Int32,
                release_date DateTime
            ) ENGINE = MergeTree ORDER BY (snapshot_utc, app_id)";
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task WriteSnapshotAsync(DateTime snapshotUtc, IEnumerable<ClickHouseGameRow> rows, CancellationToken ct)
        {
            await _conn.OpenAsync(ct);
            using var bulk = new ClickHouseBulkCopy(_conn)
            {
                DestinationTableName = "game_snapshots",
                BatchSize = 1000
            };

            var table = new DataTable();
            table.Columns.Add("snapshot_utc", typeof(DateTime));
            table.Columns.Add("app_id", typeof(int));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("genre", typeof(string));
            table.Columns.Add("followers", typeof(int));
            table.Columns.Add("release_date", typeof(DateTime));

            foreach (var r in rows)
            {
                table.Rows.Add(snapshotUtc, r.AppId, r.Name, r.Genre, r.Followers, r.ReleaseDateUtc);
            }

            await bulk.WriteToServerAsync(table, ct);
        }

        public async Task<IEnumerable<GenreDynamicsRow>> GetGenreDynamicsAsync(DateOnly startMonth, DateOnly endMonth, CancellationToken ct)
        {
            await _conn.OpenAsync(ct);
            await using var cmd = _conn.CreateCommand();
            
            var startDate = startMonth.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
            var endDate = endMonth.AddMonths(1).ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
            
            cmd.CommandText = $@"
                SELECT 
                    genre,
                    formatDateTime(snapshot_utc, '%Y-%m') as month,
                    count(DISTINCT app_id) as games_count,
                    avg(followers) as avg_followers
                FROM game_snapshots 
                WHERE snapshot_utc >= '{startDate}' AND snapshot_utc < '{endDate}'
                GROUP BY genre, month
                ORDER BY month, games_count DESC";
            
            var results = new List<GenreDynamicsRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            
            while (await reader.ReadAsync(ct))
            {
                results.Add(new GenreDynamicsRow(
                    Genre: reader.GetString("genre"),
                    Month: reader.GetString("month"),
                    GamesCount: reader.GetInt32("games_count"),
                    AvgFollowers: reader.GetDouble("avg_followers")
                ));
            }
            
            return results;
        }

        public async Task<IEnumerable<GenreAgg>> GetTopGenresAsync(DateOnly startMonth, DateOnly endMonth, int top, CancellationToken ct)
        {
            await _conn.OpenAsync(ct);
            await using var cmd = _conn.CreateCommand();
            
            var startDate = startMonth.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
            var endDate = endMonth.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
            
            cmd.CommandText = $@"
                SELECT 
                    genre,
                    count(DISTINCT app_id) as games_count,
                    avg(followers) as avg_followers
                FROM game_snapshots 
                WHERE release_date >= '{startDate}' AND release_date < '{endDate}'
                GROUP BY genre
                ORDER BY games_count DESC
                LIMIT {top}";
            
            var results = new List<GenreAgg>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            
            while (await reader.ReadAsync(ct))
            {
                results.Add(new GenreAgg(
                    Genre: reader.GetString(0),
                    Games: (int)Convert.ToUInt64(reader.GetValue(1)),
                    AvgFollowers: reader.GetDouble(2)
                ));
            }
            
            return results;
        }
    }
}


