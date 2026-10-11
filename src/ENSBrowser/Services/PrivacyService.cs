using CefSharp;
using CefSharp.Wpf;
using ENSBrowser.Models;

namespace ENSBrowser.Services;

public sealed class PrivacyService
{
    public PrivacyService(bool trackingPreventionEnabled = true, bool clearBrowsingDataOnExit = false)
    {
        TrackingPreventionEnabled = trackingPreventionEnabled;
        ClearBrowsingDataOnExit = clearBrowsingDataOnExit;
    }

    public bool TrackingPreventionEnabled { get; set; }
    public bool ClearBrowsingDataOnExit { get; set; }

    public void Apply(ChromiumWebBrowser browser)
    {
        if (browser.IsBrowserInitialized)
            browser.GetBrowser().GetHost().SetFocus(true);
    }

    public async Task ClearSessionDataAsync(ChromiumWebBrowser browser)
    {
        if (!browser.IsBrowserInitialized)
            return;

        await browser.GetCookieManager().DeleteCookiesAsync();
        browser.GetRequestContext().ClearHttpAuthCredentials();
    }

    public PrivacySnapshot Snapshot()
        => new(true, true, TrackingPreventionEnabled, ClearBrowsingDataOnExit);
}
