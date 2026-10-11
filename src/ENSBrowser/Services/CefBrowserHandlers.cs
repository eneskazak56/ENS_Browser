using CefSharp;
using CefSharp.Handler;
using ENSBrowser.Models;
using System.IO;

namespace ENSBrowser.Services;

public sealed class SafeRequestHandler : RequestHandler
{
    private readonly Action<string> _blocked;

    public SafeRequestHandler(Action<string> blocked) => _blocked = blocked;

    protected override bool OnBeforeBrowse(
        IWebBrowser chromiumWebBrowser,
        IBrowser browser,
        IFrame frame,
        IRequest request,
        bool userGesture,
        bool isRedirect)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https" or "about"))
        {
            _blocked("Blocked unsupported navigation scheme.");
            return true;
        }

        return false;
    }
}

public sealed class BrowserDownloadHandler : DownloadHandler
{
    private readonly string _downloadFolder;
    private readonly Action<DownloadRecord> _started;
    private readonly Action<DownloadItem, DownloadRecord> _updated;
    private readonly Dictionary<int, DownloadRecord> _records = [];

    public BrowserDownloadHandler(
        string downloadFolder,
        Action<DownloadRecord> started,
        Action<DownloadItem, DownloadRecord> updated)
    {
        _downloadFolder = downloadFolder;
        _started = started;
        _updated = updated;
    }

    protected override bool OnBeforeDownload(
        IWebBrowser chromiumWebBrowser,
        IBrowser browser,
        DownloadItem downloadItem,
        IBeforeDownloadCallback callback)
    {
        var fileName = Path.GetFileName(
            string.IsNullOrWhiteSpace(downloadItem.SuggestedFileName)
                ? "download"
                : downloadItem.SuggestedFileName);
        var path = Path.Combine(_downloadFolder, fileName);
        var record = new DownloadRecord
        {
            Id = Guid.NewGuid().ToString("N"),
            FileName = fileName,
            FilePath = path,
            SourceUrl = downloadItem.Url
        };
        _records[downloadItem.Id] = record;
        _started(record);
        using (callback)
            callback.Continue(path, showDialog: false);
        return true;
    }

    protected override void OnDownloadUpdated(
        IWebBrowser chromiumWebBrowser,
        IBrowser browser,
        DownloadItem downloadItem,
        IDownloadItemCallback callback)
    {
        if (_records.TryGetValue(downloadItem.Id, out var record))
            _updated(downloadItem, record);
    }
}
