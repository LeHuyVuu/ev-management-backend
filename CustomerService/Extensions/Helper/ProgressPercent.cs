namespace CustomerService.Extensions.Helper;

public class ProgressPercent
{
    public static decimal Get(decimal totalPrice, decimal amountPaid)
    {
        if (totalPrice <= 0m) return 0m;
        var paid = Math.Clamp(amountPaid, 0m, totalPrice);
        return Math.Round(paid / totalPrice * 100m, 2, MidpointRounding.AwayFromZero);
    }
}