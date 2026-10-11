using System.Text.Json;
using System.IO;
using ENSBrowser.Models;

namespace ENSBrowser.Services;

public sealed class PreferencesStore
{
    public static string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ENSBrowser");

    private readonly string _filePath = Path.Combine(DataDirectory, "preferences.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<BrowserPreferences> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new BrowserPreferences();

        await using var stream = File.OpenRead(_filePath);
        var preferences = await JsonSerializer.DeserializeAsync<BrowserPreferences>(stream) ?? new BrowserPreferences();
        if (string.Equals(preferences.SearchEngine, "Bing", StringComparison.OrdinalIgnoreCase))
        {
            preferences.SearchEngine = "DuckDuckGo";
            if (preferences.HomePage.Contains("bing.com", StringComparison.OrdinalIgnoreCase))
                preferences.HomePage = "https://duckduckgo.com";
        }
        return preferences;
    }

    public async Task SaveAsync(BrowserPreferences preferences)
    {
        Directory.CreateDirectory(DataDirectory);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, preferences, JsonOptions);
    }
}
