using System.Text.Json;
using System.IO;
using ENSBrowser.Models;

namespace ENSBrowser.Services;

public sealed class BrowserStore
{
    private readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ENSBrowser",
        "browser-state.json");

    public List<Bookmark> Bookmarks { get; private set; } = [];
    public List<HistoryEntry> History { get; private set; } = [];

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
            return;

        await using var stream = File.OpenRead(_filePath);
        var state = await JsonSerializer.DeserializeAsync<BrowserState>(stream);
        Bookmarks = state?.Bookmarks ?? [];
        History = state?.History ?? [];
    }

    public async Task SaveAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, new BrowserState(Bookmarks, History),
            new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task AddBookmarkAsync(string title, string url)
    {
        if (Bookmarks.Any(item => string.Equals(item.Url, url, StringComparison.OrdinalIgnoreCase)))
            return;

        Bookmarks.Insert(0, new Bookmark(title, url));
        await SaveAsync();
    }

    public async Task AddHistoryAsync(string title, string url)
    {
        if (url is "about:blank" || History.FirstOrDefault()?.Url == url)
            return;

        History.Insert(0, new HistoryEntry(title, url, DateTimeOffset.Now));
        History = History.Take(200).ToList();
        await SaveAsync();
    }

    public async Task RemoveBookmarkAsync(string url)
    {
        Bookmarks = Bookmarks
            .Where(item => !string.Equals(item.Url, url, StringComparison.OrdinalIgnoreCase))
            .ToList();
        await SaveAsync();
    }

    public async Task ClearHistoryAsync()
    {
        History.Clear();
        await SaveAsync();
    }

    public async Task RemoveHistoryAsync(string url)
    {
        History = History
            .Where(item => !string.Equals(item.Url, url, StringComparison.OrdinalIgnoreCase))
            .ToList();
        await SaveAsync();
    }

    private sealed record BrowserState(List<Bookmark> Bookmarks, List<HistoryEntry> History);
}
