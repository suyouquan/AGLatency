namespace AGLatency.Config
{
    public sealed class AppSettings
    {
        public ProcessingSettings Processing { get; set; } = new();
    }

    public sealed class ProcessingSettings
    {
        public int MaxDOP { get; set; } = 8;
        public int QueueHighWatermark { get; set; } = 5000;
    }
}