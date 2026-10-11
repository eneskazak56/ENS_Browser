using CefSharp;
using CefSharp.Wpf;

namespace ENSBrowser.Services;

public sealed class PageCommandService
{
    public void Find(ChromiumWebBrowser? view, string text)
    {
        if (view?.IsBrowserInitialized != true || string.IsNullOrWhiteSpace(text))
            return;

        view.ExecuteScriptAsync($"window.find({System.Text.Json.JsonSerializer.Serialize(text)})");
    }

    public void SetZoom(ChromiumWebBrowser? view, double zoomFactor)
    {
        if (view?.IsBrowserInitialized != true)
            return;

        view.ZoomLevel = Math.Log(Math.Clamp(zoomFactor, 0.25, 5.0), 1.2);
    }

    public double GetZoom(ChromiumWebBrowser? view) =>
        view?.IsBrowserInitialized == true ? Math.Pow(1.2, view.ZoomLevel) : 1.0;

    public void ResetZoom(ChromiumWebBrowser? view) => SetZoom(view, 1.0);
}
