using Npgsql;
using NpgsqlTypes;
using UtilityService.Models;

namespace UtilityService.Infrastructure.Repositories;

public class ForecastRepository
{
    private readonly string _connStr;

    private const string SqlBase = """
    SELECT dealer_id, dealer_name, region,
           vehicle_version_id, version_name, week_start,
           baseline_ema, discount_rate_active, has_promo, forecast_units
    FROM evm_ai.forecast_simple_ema(12, 0.30, 0.80, 0.10)
    """;

    public ForecastRepository(IConfiguration cfg)
    {
        _connStr = cfg.GetConnectionString("Pg")
                 ?? cfg.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing connection string");
    }

    public async Task<(IReadOnlyList<ForecastRow> rows, int total)> GetForecastAsync(
        ForecastQuery q, CancellationToken ct = default)
    {
        var rows = new List<ForecastRow>();
        await using var conn = new NpgsqlConnection(_connStr);
        await conn.OpenAsync(ct);

        var sqlData = $"""
        WITH base AS ( {SqlBase} )
        SELECT dealer_id, dealer_name, region,
               vehicle_version_id, version_name, week_start,
               baseline_ema, discount_rate_active, has_promo, forecast_units
        FROM base
        WHERE (@dealer_id::uuid IS NULL OR dealer_id = @dealer_id)
          AND (@vehicle_version_id::uuid IS NULL OR vehicle_version_id = @vehicle_version_id)
          AND (@from_week::date IS NULL OR week_start >= @from_week)
          AND (@to_week::date IS NULL OR week_start <= @to_week)
        ORDER BY week_start, dealer_name, version_name
        OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
        """;

        var sqlCount = $"""
        WITH base AS ( {SqlBase} )
        SELECT COUNT(*)
        FROM base
        WHERE (@dealer_id::uuid IS NULL OR dealer_id = @dealer_id)
          AND (@vehicle_version_id::uuid IS NULL OR vehicle_version_id = @vehicle_version_id)
          AND (@from_week::date IS NULL OR week_start >= @from_week)
          AND (@to_week::date IS NULL OR week_start <= @to_week);
        """;

        var offset = q.Page <= 1 ? 0 : (q.Page - 1) * q.PageSize;

        // Data
        await using (var cmd = new NpgsqlCommand(sqlData, conn))
        {
            cmd.Parameters.Add("@dealer_id", NpgsqlDbType.Uuid).Value = (object?)q.DealerId ?? DBNull.Value;
            cmd.Parameters.Add("@vehicle_version_id", NpgsqlDbType.Uuid).Value = (object?)q.VehicleVersionId ?? DBNull.Value;
            cmd.Parameters.Add("@from_week", NpgsqlDbType.Date).Value = (object?)q.FromWeek ?? DBNull.Value;
            cmd.Parameters.Add("@to_week", NpgsqlDbType.Date).Value = (object?)q.ToWeek ?? DBNull.Value;
            cmd.Parameters.Add("@offset", NpgsqlDbType.Integer).Value = offset;
            cmd.Parameters.Add("@limit", NpgsqlDbType.Integer).Value = q.PageSize;

            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                rows.Add(new ForecastRow
                {
                    dealer_id = rd.GetGuid(0),
                    dealer_name = rd.GetString(1),
                    region = rd.IsDBNull(2) ? null : rd.GetString(2),
                    vehicle_version_id = rd.GetGuid(3),
                    version_name = rd.GetString(4),
                    week_start = rd.GetDateTime(5),
                    baseline_ema = rd.GetDecimal(6),
                    discount_rate_active = rd.GetDecimal(7),
                    has_promo = rd.GetInt32(8),
                    forecast_units = rd.GetDecimal(9)
                });
            }
        }

        // Count
        int total;
        await using (var cmd2 = new NpgsqlCommand(sqlCount, conn))
        {
            cmd2.Parameters.Add("@dealer_id", NpgsqlDbType.Uuid).Value = (object?)q.DealerId ?? DBNull.Value;
            cmd2.Parameters.Add("@vehicle_version_id", NpgsqlDbType.Uuid).Value = (object?)q.VehicleVersionId ?? DBNull.Value;
            cmd2.Parameters.Add("@from_week", NpgsqlDbType.Date).Value = (object?)q.FromWeek ?? DBNull.Value;
            cmd2.Parameters.Add("@to_week", NpgsqlDbType.Date).Value = (object?)q.ToWeek ?? DBNull.Value;

            total = Convert.ToInt32(await cmd2.ExecuteScalarAsync(ct));
        }

        return (rows, total);
    }
}
