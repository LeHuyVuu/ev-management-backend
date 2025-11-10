using Npgsql;
using NpgsqlTypes;

namespace UtilityService.Infrastructure.Repositories;

public class ForecastRepository
{
    private readonly string _cs;
    public ForecastRepository(IConfiguration cfg)
{
    _cs = cfg.GetConnectionString("Pg") 
       ?? cfg.GetConnectionString("PG") 
       ?? cfg.GetConnectionString("DefaultConnection") 
       ?? throw new InvalidOperationException("Connection string not found for Pg/PG/DefaultConnection");
}


    // Lấy chuỗi orders theo tuần (đã fill tuần trống), tuần bắt đầu Thứ Hai.
    // Loại trừ cancelled để tránh nhiễu học.
    public async Task<List<(DateTime WeekStart, int Orders)>> GetWeeklyOrders(Guid dealerId, Guid vehicleVersionId)
    {
        await using var con = new NpgsqlConnection(_cs);
        await con.OpenAsync();

        var sql = @"
WITH raw AS (
    SELECT
        date_trunc('week', o.delivery_date::timestamp)::date AS wk,
        COUNT(*)::int AS cnt
    FROM evm.orders o
    JOIN evm.contracts c ON c.contract_id = o.contract_id
    JOIN evm.quotes   q ON q.quote_id   = c.quote_id
    WHERE o.dealer_id = @dealerId
      AND q.vehicle_version_id = @vehicleVersionId
      AND o.delivery_date IS NOT NULL
      AND o.status NOT IN ('cancelled')
    GROUP BY 1
),
range AS (
    SELECT (SELECT MIN(wk) FROM raw) AS min_wk,
           (SELECT MAX(wk) FROM raw) AS max_wk
),
weeks AS (
    SELECT gs::date AS wk
    FROM range r,
         generate_series(r.min_wk, r.max_wk, interval '7 day') AS gs
)
SELECT w.wk AS week_start,
       COALESCE(r.cnt, 0)::int AS orders_cnt
FROM weeks w
LEFT JOIN raw r ON r.wk = w.wk
ORDER BY w.wk;";

        await using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.Add(new NpgsqlParameter("dealerId", NpgsqlDbType.Uuid) { Value = dealerId });
        cmd.Parameters.Add(new NpgsqlParameter("vehicleVersionId", NpgsqlDbType.Uuid) { Value = vehicleVersionId });

        var list = new List<(DateTime, int)>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
            list.Add((rd.GetDateTime(0), rd.GetInt32(1)));

        return list;
    }

    // Tồn kho brand tổng cho 1 vehicle_version
    public async Task<int> GetBrandInventory(Guid vehicleVersionId)
    {
        await using var con = new NpgsqlConnection(_cs);
        await con.OpenAsync();

        const string sql = @"SELECT stock_quantity FROM evm.brand_inventory WHERE vehicle_version_id = @vid LIMIT 1;";
        await using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.Add(new NpgsqlParameter("vid", NpgsqlDbType.Uuid) { Value = vehicleVersionId });

        var obj = await cmd.ExecuteScalarAsync();
        return (obj == null || obj is DBNull) ? 0 : Convert.ToInt32(obj);
    }

    // Danh sách dealer có lịch sử giao cho vehicle_version (loại cancelled)
    public async Task<List<Guid>> GetDealersForVehicleVersion(Guid vehicleVersionId)
    {
        await using var con = new NpgsqlConnection(_cs);
        await con.OpenAsync();

        var sql = @"
SELECT DISTINCT o.dealer_id
FROM evm.orders o
JOIN evm.contracts c ON c.contract_id = o.contract_id
JOIN evm.quotes   q ON q.quote_id   = c.quote_id
WHERE q.vehicle_version_id = @vid
  AND o.delivery_date IS NOT NULL
  AND o.status NOT IN ('cancelled');";
        await using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.Add(new NpgsqlParameter("vid", NpgsqlDbType.Uuid) { Value = vehicleVersionId });

        var list = new List<Guid>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) list.Add(rd.GetGuid(0));
        return list;
    }

    public async Task<List<(DateTime WeekStart, int Orders)>> GetWeeklyOrders(
        Guid dealerId, Guid vehicleVersionId, int requiredWeeks)
    {
        await using var con = new NpgsqlConnection(_cs);
        await con.OpenAsync();

        var sql = @"
WITH raw AS (
    SELECT date_trunc('week', o.delivery_date::timestamp)::date AS wk,
           COUNT(*)::int AS cnt
    FROM evm.orders o
    JOIN evm.contracts c ON c.contract_id = o.contract_id
    JOIN evm.quotes   q ON q.quote_id   = c.quote_id
    WHERE o.dealer_id = @dealerId
      AND q.vehicle_version_id = @vehicleVersionId
      AND o.delivery_date IS NOT NULL
      AND o.status NOT IN ('cancelled')
    GROUP BY 1
),
bounds AS (
    SELECT MIN(wk) AS min_wk, MAX(wk) AS max_wk FROM raw
),
series AS (
    SELECT CASE WHEN (SELECT max_wk FROM bounds) IS NULL THEN NULL
                ELSE LEAST((SELECT min_wk FROM bounds),
                           (SELECT max_wk FROM bounds) - (@weeks - 1) * interval '7 day')::date
           END AS start_wk,
           (SELECT max_wk FROM bounds) AS end_wk
),
weeks AS (
    SELECT gs::date AS wk
    FROM series s,
         LATERAL (SELECT generate_series(s.start_wk, s.end_wk, interval '7 day')) g(gs)
    WHERE s.start_wk IS NOT NULL
)
SELECT w.wk AS week_start, COALESCE(r.cnt, 0)::int AS orders_cnt
FROM weeks w
LEFT JOIN raw r ON r.wk = w.wk
ORDER BY w.wk;";

        await using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.Add(new NpgsqlParameter("dealerId", NpgsqlDbType.Uuid) { Value = dealerId });
        cmd.Parameters.Add(new NpgsqlParameter("vehicleVersionId", NpgsqlDbType.Uuid) { Value = vehicleVersionId });
        cmd.Parameters.Add(new NpgsqlParameter("weeks", NpgsqlDbType.Integer) { Value = Math.Max(2, requiredWeeks) });

        var list = new List<(DateTime, int)>();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
            list.Add((rd.GetDateTime(0), rd.GetInt32(1)));
        return list;
    }

    // ===== ADD: DTO & Queries phục vụ debug-all (pagination + search) =====
    public class DealerVehiclePairRow
    {
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = "";
        public Guid VehicleVersionId { get; set; }
        public string VehicleLabel { get; set; } = ""; // Brand Model - Version
    }

    public class DealerVehiclePairPaged
    {
        public int Total { get; set; }
        public List<DealerVehiclePairRow> Items { get; set; } = new();
    }

    // Lấy label hiển thị cho 1 cặp (dealer, vehicleVersion)
    public async Task<(string DealerName, string VehicleLabel)> GetDealerVehicleMetaAsync(Guid dealerId, Guid vehicleVersionId)
    {
        await using var con = new NpgsqlConnection(_cs);
        await con.OpenAsync();

        var sql = @"
SELECT d.name AS dealer_name,
       (veh.brand || ' ' || veh.model_name || ' - ' || vv.version_name) AS vehicle_label
FROM evm.dealers d
CROSS JOIN evm.vehicle_versions vv
JOIN evm.vehicles veh ON veh.vehicle_id = vv.vehicle_id
WHERE d.dealer_id = @dealerId AND vv.vehicle_version_id = @vvId
LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("dealerId", dealerId);
        cmd.Parameters.AddWithValue("vvId", vehicleVersionId);

        await using var rd = await cmd.ExecuteReaderAsync();
        if (await rd.ReadAsync())
        {
            return (
                rd.IsDBNull(0) ? "" : rd.GetString(0),
                rd.IsDBNull(1) ? "" : rd.GetString(1)
            );
        }
        return ("", "");
    }

    // ===== CHỈ SỬA METHOD NÀY =====
    // Lấy danh sách cặp (dealer, vehicleVersion) có lịch sử, có tìm kiếm, phân trang
    public async Task<DealerVehiclePairPaged> GetAllDealerVehiclePairsPagedAsync(
        string? search, Guid? dealerId, Guid? vehicleVersionId, int page, int pageSize)
    {
        await using var con = new NpgsqlConnection(_cs);
        await con.OpenAsync();

        var sqlBase = @"
WITH pairs AS (
  SELECT DISTINCT
         o.dealer_id,
         q.vehicle_version_id
  FROM evm.orders o
  JOIN evm.contracts c ON c.contract_id = o.contract_id
  JOIN evm.quotes   q ON q.quote_id   = c.quote_id
  WHERE o.delivery_date IS NOT NULL
    AND o.status NOT IN ('cancelled')
),
meta AS (
  SELECT
     p.dealer_id,
     d.name AS dealer_name,
     p.vehicle_version_id,
     (veh.brand || ' ' || veh.model_name || ' - ' || vv.version_name) AS vehicle_label
  FROM pairs p
  JOIN evm.dealers d           ON d.dealer_id             = p.dealer_id
  JOIN evm.vehicle_versions vv ON vv.vehicle_version_id   = p.vehicle_version_id
  JOIN evm.vehicles veh        ON veh.vehicle_id          = vv.vehicle_id
  WHERE
    ( @dealerId::uuid IS NULL OR p.dealer_id = @dealerId::uuid )
    AND ( @vehicleVersionId::uuid IS NULL OR p.vehicle_version_id = @vehicleVersionId::uuid )
    AND (
      NULLIF(BTRIM(@search::text), '') IS NULL
      OR d.name         ILIKE ('%' || @search::text || '%')
      OR veh.brand      ILIKE ('%' || @search::text || '%')
      OR veh.model_name ILIKE ('%' || @search::text || '%')
      OR vv.version_name ILIKE ('%' || @search::text || '%')
    )
)
SELECT COUNT(*) OVER() AS total_count,
       dealer_id, dealer_name, vehicle_version_id, vehicle_label
FROM meta
ORDER BY dealer_name, vehicle_label
OFFSET @offset ROWS
LIMIT  @limit;";

        await using var cmd = new NpgsqlCommand(sqlBase, con);

        // Ép kiểu để Postgres không bị 42P08 khi NULL
        cmd.Parameters.Add(new NpgsqlParameter("dealerId", NpgsqlDbType.Uuid)
        {
            Value = dealerId ?? (object)DBNull.Value
        });
        cmd.Parameters.Add(new NpgsqlParameter("vehicleVersionId", NpgsqlDbType.Uuid)
        {
            Value = vehicleVersionId ?? (object)DBNull.Value
        });
        cmd.Parameters.Add(new NpgsqlParameter("search", NpgsqlDbType.Text)
        {
            Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search
        });
        cmd.Parameters.Add(new NpgsqlParameter("offset", NpgsqlDbType.Integer)
        {
            Value = Math.Max(0, (page - 1) * pageSize)
        });
        cmd.Parameters.Add(new NpgsqlParameter("limit", NpgsqlDbType.Integer)
        {
            Value = pageSize
        });

        var res = new DealerVehiclePairPaged();
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            if (res.Total == 0)
                res.Total = rd.GetInt32(0);

            res.Items.Add(new DealerVehiclePairRow
            {
                DealerId = rd.GetGuid(1),
                DealerName = rd.IsDBNull(2) ? "" : rd.GetString(2),
                VehicleVersionId = rd.GetGuid(3),
                VehicleLabel = rd.IsDBNull(4) ? "" : rd.GetString(4)
            });
        }
        return res;
    }
}
