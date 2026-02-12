using System.Xml.Linq;
using ProjectEye.Core.Abstractions;

namespace ProjectEye.Core.Services;

public sealed class LegacyConfigXmlImporter
{
    private readonly IAppConfigStore _appConfigStore;

    public LegacyConfigXmlImporter(IAppConfigStore appConfigStore)
    {
        _appConfigStore = appConfigStore;
    }

    public async Task<bool> TryImportFromFileAsync(string legacyConfigPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(legacyConfigPath))
        {
            return false;
        }

        var current = await _appConfigStore.LoadAsync(cancellationToken).ConfigureAwait(false);
        if (!IsDefaultConfig(current))
        {
            return false;
        }

        var xml = await File.ReadAllTextAsync(legacyConfigPath, cancellationToken).ConfigureAwait(false);
        var doc = XDocument.Parse(xml);

        var imported = new AppConfig
        {
            WarnMinutes = Math.Max(1, GetInt(doc.Root, "General", "WarnTime", current.WarnMinutes)),
            RestSeconds = Math.Max(1, GetInt(doc.Root, "General", "RestTime", current.RestSeconds)),
            TomatoMinutes = Math.Max(1, GetInt(doc.Root, "Tomato", "WorkMinutes", current.TomatoMinutes)),
            SoundEnabled = GetBool(doc.Root, "General", "Sound", current.SoundEnabled),
            Theme = GetString(doc.Root, current.Theme, "Style", "Theme", "ThemeName"),
            Language = NormalizeLanguage(GetString(doc.Root, current.Language, "Style", "Language", "Value"))
        };

        await _appConfigStore.SaveAsync(imported, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static bool IsDefaultConfig(AppConfig config)
    {
        return config.WarnMinutes == 45
               && config.RestSeconds == 20
               && config.TomatoMinutes == 25
               && config.SoundEnabled
               && string.Equals(config.Theme, "Default", StringComparison.OrdinalIgnoreCase)
               && string.Equals(config.Language, "zh-CN", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetInt(XElement? root, string section, string key, int fallback)
    {
        var value = root?.Element(section)?.Element(key)?.Value;
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static bool GetBool(XElement? root, string section, string key, bool fallback)
    {
        var value = root?.Element(section)?.Element(key)?.Value;
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static string GetString(XElement? root, string fallback, params string[] path)
    {
        XElement? current = root;
        foreach (var p in path)
        {
            current = current?.Element(p);
        }

        return string.IsNullOrWhiteSpace(current?.Value) ? fallback : current!.Value;
    }

    private static string NormalizeLanguage(string languageValue)
    {
        return languageValue.Equals("zh", StringComparison.OrdinalIgnoreCase)
            ? "zh-CN"
            : languageValue.Equals("en", StringComparison.OrdinalIgnoreCase)
                ? "en-US"
                : languageValue;
    }
}
