namespace CustomerService.Extensions.Helper;

public class InstallmentCodec
{
    // Build: installment?v=1&m=<months>&pct=<percent>
    public static string Build(int v, int months, int pct)
    {
        if (v <= 0) throw new ArgumentOutOfRangeException(nameof(v));
        if (months <= 0) throw new ArgumentOutOfRangeException(nameof(months));
        if (pct <= 0 || pct > 100) throw new ArgumentOutOfRangeException(nameof(pct));
        return $"installment?v={v}&m={months}&pct={pct}";
    }

    // Parse ngược: lấy {v, months, pct} từ chuỗi
    public static (int v, int months, int pct) Parse(string methodPayment)
    {
        if (string.IsNullOrWhiteSpace(methodPayment) || !methodPayment.StartsWith("installment?", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Unsupported method_payment format");

        var query = methodPayment.Substring(methodPayment.IndexOf('?') + 1);
        var nv = System.Web.HttpUtility.ParseQueryString(query);
        int v = int.Parse(nv["v"] ?? "1");
        int m = int.Parse(nv["m"] ?? throw new FormatException("Missing months (m)"));
        int pct = int.Parse(nv["pct"] ?? throw new FormatException("Missing pct"));

        if (v <= 0 || m <= 0 || pct <= 0 || pct > 100) throw new FormatException("Invalid values");
        return (v, m, pct);
    }
}