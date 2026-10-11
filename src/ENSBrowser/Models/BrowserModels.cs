using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ENSBrowser.Models;

public sealed record Bookmark(string Title, string Url);
public sealed record HistoryEntry(string Title, string Url, DateTimeOffset VisitedAt);

public sealed class BrowserTab : INotifyPropertyChanged
{
    public required string Id { get; init; }
    private string _title = "New tab";
    private string _url = "https://www.bing.com";

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Url
    {
        get => _url;
        set => SetField(ref _url, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
