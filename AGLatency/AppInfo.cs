using System.Reflection;

namespace AGLatency;

internal static class AppInfo
{
    public static string Version { get; } =
        Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "unknown";

    public const string Name = "AGLatency Report Tool";
    public const string Url = "https://github.com/suyouquan/AGLatency";

    public static string Title => $"{Name}, Version {Version}";
}