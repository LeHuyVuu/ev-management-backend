using Npgsql;

namespace UtilityService.Worker;

public class ForecastAutoRefreshWorker : BackgroundService
{
    private readonly ILogger<ForecastAutoRefreshWorker> _log;
    private readonly string _connString;

    public ForecastAutoRefreshWorker(ILogger<ForecastAutoRefreshWorker> log, IConfiguration cfg)
    {
        _log = log;
        _connString = cfg.GetConnectionString("Pg")
                      ?? cfg.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Missing connection string");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1)); // 1 phút/lần

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connString);
                await conn.OpenAsync(stoppingToken);
                await using var cmd = new NpgsqlCommand("SELECT evm.refresh_simple_forecasts_if_changed();", conn);
                await cmd.ExecuteNonQueryAsync(stoppingToken);
                _log.LogInformation("✅ simple_forecasts refresh check done at {time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "❌ Error refreshing simple_forecasts");
            }
        }
    }
}
