using System;
using System.Configuration; // requires System.Configuration.ConfigurationManager NuGet

namespace AGLatency.Config
{
    public interface ISettingsProvider
    {
        AppSettings Load();
    }

    public sealed class SettingsProvider : ISettingsProvider
    {

        public SettingsProvider()
        {

        }

        public AppSettings Load()
        {
           
            var settings = new AppSettings();
            settings.Processing ??= new ProcessingSettings();
            try
            {
                Logger.LogMessage("Loading settings from app.config");

                var s = ConfigurationManager.AppSettings["Processing.MaxDegreeOfParallelism"]
                        ?? ConfigurationManager.AppSettings["MaxDegreeOfParallelism"];
                if (int.TryParse(s, out var m))
                {
                    settings.Processing.MaxDOP = m;
                    Logger.LogMessage($"Loaded MaxDegreeOfParallelism from app.config: {m}");
                }

                var batchSize = ConfigurationManager.AppSettings["Processing.BatchSize"]
                        ?? ConfigurationManager.AppSettings["BatchSize"];
                if (int.TryParse(batchSize, out var b))
                {
                    settings.Processing.BatchSize = b;
                    Logger.LogMessage($"Loaded BatchSize from app.config: {b}");
                }

                var queueHW = ConfigurationManager.AppSettings["Processing.QueueHighWatermark"]
                        ?? ConfigurationManager.AppSettings["QueueHighWatermark"];
                if (int.TryParse(queueHW, out var q))
                {
                    settings.Processing.QueueHighWatermark = q;
                    Logger.LogMessage($"Loaded QueueHighWatermark from app.config: {q}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, System.Threading.Thread.CurrentThread);
            }

            // 3) Defaults if not present
            return settings;
        }
    }
}