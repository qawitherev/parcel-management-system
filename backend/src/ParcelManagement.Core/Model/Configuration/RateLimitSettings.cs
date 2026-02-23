namespace ParcelManagement.Core.Model.Configuration
{
    public class RateLimitSettings
    {
        public SlidingWindowSettings Global { get; set; } = new();
        public SlidingWindowSettings User { get; set; } = new();
        public SlidingWindowSettings StrictEndpoint { get; set; } = new();
    }

    public class SlidingWindowSettings
    {
        public int PermitLimit { get; set; }
        public int WindowMinutes { get; set; }
        public int SegmentsPerWindow { get; set; }
        public int QueueLimit { get; set; }
    }
}
