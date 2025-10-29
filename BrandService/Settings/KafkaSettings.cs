namespace BrandService.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = null!;
    public Dictionary<string, string> Topics { get; set; } = new();
}