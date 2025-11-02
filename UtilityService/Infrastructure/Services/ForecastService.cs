using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using UtilityService.Infrastructure.Repositories;

namespace UtilityService.Infrastructure.Services;

public class ForecastService
{
    private readonly MLContext _ml;
    private readonly ForecastRepository _repo;

    // Cho phép test nhanh khi lịch sử ngắn
    private const int MinWeeksForTraining = 4; // Dev/demo: 4; Prod nên >= 12

    public ForecastService(MLContext ml, ForecastRepository repo)
    {
        _ml = ml;
        _repo = repo;
    }

    // Chuẩn hóa chuỗi tuần (hàm dự phòng; hiện đã fill tuần ở SQL)
    private static List<(DateTime WeekStart, float Y)> NormalizeWeeklySeries(List<(DateTime WeekStart, int Orders)> raw)
    {
        var res = new List<(DateTime, float)>();
        if (raw.Count == 0) return res;

        var start = raw.First().WeekStart;
        var end = raw.Last().WeekStart;
        var dict = raw.ToDictionary(x => x.WeekStart, x => (float)x.Orders);

        for (var w = start; w <= end; w = w.AddDays(7))
            res.Add((w, dict.TryGetValue(w, out var v) ? v : 0f));

        return res;
    }

    // Dự báo cho 1 dealer + 1 phiên bản xe (có fallback nếu thiếu MKL/thiếu lịch sử)
    public async Task<List<(DateTime WeekStart, float Forecast)>> ForecastWeekly(
        Guid dealerId, Guid vehicleVersionId, int horizon)
    {
        var hist = await _repo.GetWeeklyOrders(dealerId, vehicleVersionId);
        if (hist.Count == 0)
            throw new InvalidOperationException("Không có lịch sử orders để dự báo.");

        var series = NormalizeWeeklySeries(hist);
        var lastWeek = series.Last().WeekStart;

        // Ngưỡng tối thiểu để train SSA
        var minWeeks = Math.Max(MinWeeksForTraining, Math.Min(12, horizon + 2));

        try
        {
            // Nếu lịch sử quá ngắn → fallback (tránh ném lỗi)
            if (series.Count < minWeeks)
                return FallbackForecast(series, horizon, lastWeek);

            // ===== ML.NET SSA (giữ nguyên như trước) =====
            var values = series.Select(s => new ModelInput { Value = s.Y }).ToList();
            var dv = _ml.Data.LoadFromEnumerable(values);

            var windowSize = Math.Max(4, Math.Min(8, values.Count / 4));
            var pipeline = _ml.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ForecastOutput.Forecast),
                inputColumnName: nameof(ModelInput.Value),
                windowSize: windowSize,
                seriesLength: values.Count,
                trainSize: Math.Max(horizon + 4, values.Count - horizon),
                horizon: horizon);

            var model = pipeline.Fit(dv);
            var engine = model.CreateTimeSeriesEngine<ModelInput, ForecastOutput>(_ml);
            var yhat = engine.Predict().Forecast;

            var result = new List<(DateTime, float)>();
            for (int i = 0; i < horizon; i++)
            {
                var wk = lastWeek.AddDays(7 * (i + 1));
                var v = (i < yhat.Length ? yhat[i] : yhat[^1]);
                result.Add((wk, Math.Max(0f, v)));
            }

            return result;
        }
        // Fallback khi thiếu thư viện native (MklImports/libiomp5) hoặc lỗi native tương tự
        catch (DllNotFoundException)
        {
            return FallbackForecast(series, horizon, lastWeek);
        }
        catch (TypeInitializationException)
        {
            return FallbackForecast(series, horizon, lastWeek);
        }
        catch (BadImageFormatException)
        {
            return FallbackForecast(series, horizon, lastWeek);
        }
    }

    // Fallback: dùng trung bình các tuần gần nhất khi không train được SSA
    private static List<(DateTime WeekStart, float Forecast)> FallbackForecast(
        List<(DateTime WeekStart, float Y)> series, int horizon, DateTime lastWeek)
    {
        // Lấy trung bình của tối đa 8 tuần cuối (>=1)
        int k = Math.Min(8, Math.Max(1, series.Count));
        float avg = series.TakeLast(k).Average(x => x.Y);

        // Nếu toàn 0, dùng giá trị tuần cuối
        if (avg <= 0f) avg = series.Last().Y;

        var list = new List<(DateTime, float)>();
        for (int i = 1; i <= horizon; i++)
            list.Add((lastWeek.AddDays(7 * i), Math.Max(0f, avg)));
        return list;
    }

    // Tổng nhu cầu toàn hệ thống (gộp dealers có lịch sử)
    public async Task<List<(DateTime WeekStart, double ForecastTotal)>> ForecastWeeklyTotal(
        Guid vehicleVersionId, int horizon)
    {
        var dealers = await _repo.GetDealersForVehicleVersion(vehicleVersionId);
        if (dealers.Count == 0) return new();

        var agg = new Dictionary<DateTime, double>();
        foreach (var d in dealers)
        {
            try
            {
                var fc = await ForecastWeekly(d, vehicleVersionId, horizon);
                foreach (var (wk, y) in fc)
                {
                    if (!agg.ContainsKey(wk)) agg[wk] = 0;
                    agg[wk] += y;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[weekly-total] skip dealer={d}: {ex.Message}");
            }
        }

        return agg.OrderBy(x => x.Key).Select(x => (x.Key, x.Value)).ToList();
    }

    // Phân bổ theo tỷ lệ nhu cầu dự báo (giữ lại holdback ở brand inventory)
    public async Task<List<AllocationItem>> AllocateByForecast(
        Guid vehicleVersionId, int horizonWeeks = 4, double holdbackRatio = 0.10)
    {
        var stock = await _repo.GetBrandInventory(vehicleVersionId);
        if (stock <= 0) return new();

        var dealers = await _repo.GetDealersForVehicleVersion(vehicleVersionId);
        if (dealers.Count == 0) return new();

        var need = new List<(Guid DealerId, double Need)>();
        foreach (var d in dealers)
        {
            try
            {
                var fc = await ForecastWeekly(d, vehicleVersionId, horizonWeeks);
                need.Add((d, fc.Sum(x => (double)x.Forecast)));
            }
            catch
            {
                need.Add((d, 0));
            }
        }

        var totalNeed = need.Sum(x => x.Need);
        var allocatable = (int)Math.Floor(stock * (1 - holdbackRatio));

        var items = new List<AllocationItem>();
        if (allocatable <= 0 || totalNeed <= 0) return items;

        int sumAlloc = 0;
        foreach (var n in need)
        {
            var share = n.Need <= 0 ? 0 : n.Need / totalNeed;
            var qty = (int)Math.Floor(share * allocatable);
            sumAlloc += qty;
            items.Add(new AllocationItem
            { DealerId = n.DealerId, VehicleVersionId = vehicleVersionId, Quantity = qty });
        }

        int diff = allocatable - sumAlloc;
        for (int i = 0; i < items.Count && diff > 0; i++, diff--)
            items[i].Quantity++;

        return items;
    }

    // ===== Models nội bộ =====
    public class ModelInput { public float Value { get; set; } }
    public class ForecastOutput { [VectorType] public float[] Forecast { get; set; } = Array.Empty<float>(); }
    public class AllocationItem { public Guid DealerId { get; set; } public Guid VehicleVersionId { get; set; } public int Quantity { get; set; } }

    public class MlDebugPayload
    {
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = "";
        public Guid VehicleVersionId { get; set; }
        public string VehicleLabel { get; set; } = "";
        public int Horizon { get; set; }
        public int RequiredWeeks { get; set; }
        public int SeriesLength { get; set; }
        public int WindowSize { get; set; }
        public int TrainSize { get; set; }
        public string LastHistoryWeek { get; set; } = "";
        public List<TimePoint> InputSeries { get; set; } = new();
        public List<string> FutureWeeks { get; set; } = new();

        public class TimePoint
        {
            public string WeekStart { get; set; } = "";
            public int Orders { get; set; }
        }
    }

    public class MlDebugPagedResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPairs { get; set; }
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        public List<MlDebugPayload> Data { get; set; } = new();
    }

    // Lấy payload debug cho 1 cặp
    public async Task<MlDebugPayload> GetMlDebugAsync(Guid dealerId, Guid vehicleVersionId, int horizon)
    {
        var requiredWeeks = Math.Max(MinWeeksForTraining, Math.Min(12, horizon + 2));
        var hist = await _repo.GetWeeklyOrders(dealerId, vehicleVersionId, requiredWeeks);
        if (hist.Count == 0)
            throw new InvalidOperationException("Không có lịch sử orders để debug.");

        var series = hist.Select(s => (s.WeekStart, Y: (float)s.Orders)).ToList();
        var valuesCount = series.Count;
        var windowSize = Math.Max(4, Math.Min(8, valuesCount / 4));
        var trainSize = Math.Max(horizon + 4, valuesCount - horizon);

        var lastWeek = series.Last().WeekStart;
        var futureWeeks = new List<string>();
        for (int i = 0; i < horizon; i++)
            futureWeeks.Add(lastWeek.AddDays(7 * (i + 1)).ToString("yyyy-MM-dd"));

        var meta = await _repo.GetDealerVehicleMetaAsync(dealerId, vehicleVersionId);

        return new MlDebugPayload
        {
            DealerId = dealerId,
            DealerName = meta.DealerName,
            VehicleVersionId = vehicleVersionId,
            VehicleLabel = meta.VehicleLabel,
            Horizon = horizon,
            RequiredWeeks = requiredWeeks,
            SeriesLength = valuesCount,
            WindowSize = windowSize,
            TrainSize = trainSize,
            LastHistoryWeek = lastWeek.ToString("yyyy-MM-dd"),
            InputSeries = series.Select(x => new MlDebugPayload.TimePoint
            {
                WeekStart = x.WeekStart.ToString("yyyy-MM-dd"),
                Orders = (int)x.Y
            }).ToList(),
            FutureWeeks = futureWeeks
        };
    }

    // Lấy tất cả cặp (dealer, vehicleVersion) có lịch sử theo trang + search
    public async Task<MlDebugPagedResult> GetAllMlDebugPagedAsync(
        int page, int pageSize, int horizon, string? search, Guid? dealerId, Guid? vehicleVersionId)
    {
        var pairsPaged = await _repo.GetAllDealerVehiclePairsPagedAsync(
            search: search,
            dealerId: dealerId,
            vehicleVersionId: vehicleVersionId,
            page: page,
            pageSize: pageSize
        );

        var result = new MlDebugPagedResult
        {
            Page = page,
            PageSize = pageSize,
            TotalPairs = pairsPaged.Total,
            Data = new List<MlDebugPayload>()
        };

        foreach (var p in pairsPaged.Items)
        {
            try
            {
                var payload = await GetMlDebugAsync(p.DealerId, p.VehicleVersionId, horizon);
                if (!string.IsNullOrEmpty(p.DealerName)) payload.DealerName = p.DealerName;
                if (!string.IsNullOrEmpty(p.VehicleLabel)) payload.VehicleLabel = p.VehicleLabel;
                result.Data.Add(payload);
            }
            catch
            {
                // bỏ qua cặp không đủ dữ liệu
            }
        }

        return result;
    }
}
