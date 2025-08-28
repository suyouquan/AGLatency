using System;
using System.IO;
using System.Text.Json;
using System.Configuration; // requires System.Configuration.ConfigurationManager NuGet

namespace AGLatency.Config
{
    public interface ISettingsProvider
    {
        AppSettings Load();
    }

    public sealed class JsonSettingsProvider : ISettingsProvider
    {
        private readonly string _path;
        public JsonSettingsProvider(string? path = null)
        {
            _path = path ?? Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            Logger.LogMessage($"Settings path: {_path}");
        }

        public AppSettings Load()
        {
            // 1) Try JSON first
            if (File.Exists(_path))
            {
                Logger.LogMessage($"Loading settings from JSON file: {_path}");

                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new AppSettings();
            }

            // 2) Fallback to app.config (AGLatency.dll.config)
            var settings = new AppSettings();
            try
            {
                Logger.LogMessage("Loading settings from app.config");

                var s = ConfigurationManager.AppSettings["Processing.MaxDegreeOfParallelism"]
                        ?? ConfigurationManager.AppSettings["MaxDegreeOfParallelism"];
                

                if (int.TryParse(s, out var m))
                {
                    settings.Processing ??= new ProcessingSettings();
                    settings.Processing.MaxDOP = m;
                    Logger.LogMessage($"Loaded MaxDegreeOfParallelism from app.config: {m}");
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