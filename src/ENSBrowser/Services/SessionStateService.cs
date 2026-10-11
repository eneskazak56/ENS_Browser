using System.Text.Json;
using System.IO;
using ENSBrowser.Models;

namespace ENSBrowser.Services;

public sealed class SessionStateService
{
    private readonly string _filePath = Path.Combine(PreferencesStore.DataDirectory, "session.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<SessionState> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new SessionState();

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<SessionState>(stream) ?? new SessionState();
    }

    public async Task SaveAsync(IEnumerable<BrowserTab> tabs, int selectedTabIndex)
    {
        Directory.CreateDirectory(PreferencesStore.DataDirectory);
        var state = new SessionState
        {
            Tabs = tabs.Select(tab => new SessionTab(tab.Id, tab.Title, tab.Url)).ToList(),
            SelectedTabIndex = selectedTabIndex
        };
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, state, JsonOptions);
    }
}
