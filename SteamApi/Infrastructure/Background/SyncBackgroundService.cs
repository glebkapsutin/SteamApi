using Microsoft.Extensions.Hosting;
using SteamApi.Application.Services;

namespace SteamApi.Infrastructure.Background
{
    public class SyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public SyncBackgroundService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Еженощная синхронизация (каждые 6 часов для примера)
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<ISteamSyncService>();
                    var now = DateTime.UtcNow;
                    var month = new DateOnly(now.Year, now.Month, 1);
                    await sync.SyncUpcomingAsync(month, stoppingToken);
                }
                catch
                {
                    // swallow/log
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}


