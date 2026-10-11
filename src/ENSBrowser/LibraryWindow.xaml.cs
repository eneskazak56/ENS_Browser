using System.Diagnostics;
using System.IO;
using System.Windows;
using ENSBrowser.Models;
using ENSBrowser.Services;

namespace ENSBrowser;

public partial class LibraryWindow : Window
{
    private readonly BrowserStore _browserStore;
    private readonly DownloadStore _downloadStore;
    private readonly Action<string> _openUrl;
    private string _mode = "bookmarks";

    public LibraryWindow(BrowserStore browserStore, DownloadStore downloadStore, Action<string> openUrl)
    {
        InitializeComponent();
        _browserStore = browserStore;
        _downloadStore = downloadStore;
        _openUrl = openUrl;
        Refresh();
    }

    private void Bookmarks_Click(object sender, RoutedEventArgs e)
    {
        _mode = "bookmarks";
        Refresh();
    }

    private void History_Click(object sender, RoutedEventArgs e)
    {
        _mode = "history";
        Refresh();
    }

    private void Downloads_Click(object sender, RoutedEventArgs e)
    {
        _mode = "downloads";
        Refresh();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => Refresh();

    private void Refresh()
    {
        ItemsList.ItemsSource = _mode switch
        {
            "history" => _browserStore.History.Select(entry => new LibraryItem(
                entry.Title, entry.Url, entry.VisitedAt.LocalDateTime.ToString("g"), entry.Url)),
            "downloads" => _downloadStore.Items.Select(download => new LibraryItem(
                download.FileName, download.FilePath, download.StartedAt.LocalDateTime.ToString("g"), download.FilePath)),
            _ => _browserStore.Bookmarks.Select(bookmark => new LibraryItem(
                bookmark.Title, bookmark.Url, string.Empty, bookmark.Url))
        };
    }

    private void OpenSelected_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not LibraryItem item)
            return;

        if (_mode == "downloads")
        {
            if (File.Exists(item.Location))
                Process.Start(new ProcessStartInfo(item.Location) { UseShellExecute = true });
            return;
        }

        _openUrl(item.Location);
        Close();
    }

    private async void RemoveSelected_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsList.SelectedItem is not LibraryItem item)
            return;

        if (_mode == "bookmarks")
            await _browserStore.RemoveBookmarkAsync(item.Location);
        else if (_mode == "history")
            await _browserStore.RemoveHistoryAsync(item.Location);
        else
        {
            var download = _downloadStore.Items.FirstOrDefault(x => x.FilePath == item.Location);
            if (download is not null)
                _downloadStore.Items.Remove(download);
            await _downloadStore.SaveAsync();
        }
        Refresh();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private sealed record LibraryItem(string Title, string Location, string When, string Key);
}
