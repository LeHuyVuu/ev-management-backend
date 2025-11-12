namespace IdentityService.Kafka.Event
{
    public class DealerAccountStatusEvent
    {
        public Guid DealerId { get; set; }
        public string Status { get; set; } = "";
    }
}
