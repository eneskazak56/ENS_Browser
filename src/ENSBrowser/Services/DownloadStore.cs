using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;
using ENSBrowser.Models;

namespace ENSBrowser.Services;

public sealed class DownloadStore
{
    private readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ENSBrowser",
        "downloads.json");

    public ObservableCollection<DownloadRecord> Items { get; } = [];

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
            return;

        await using var stream = File.OpenRead(_filePath);
        var records = await JsonSerializer.DeserializeAsync<List<DownloadRecord>>(stream) ?? [];
        Items.Clear();
        foreach (var record in records.Take(100))
            Items.Add(record);
    }

    public async Task AddAsync(DownloadRecord record)
    {
        Items.Insert(0, record);
        while (Items.Count > 100)
            Items.RemoveAt(Items.Count - 1);
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, Items.ToList(), new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task ClearAsync()
    {
        Items.Clear();
        await SaveAsync();
    }
}
