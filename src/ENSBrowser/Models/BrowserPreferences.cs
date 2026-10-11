namespace ENSBrowser.Models;

public sealed class BrowserPreferences
{
    public string HomePage { get; set; } = "https://duckduckgo.com";
    public string SearchEngine { get; set; } = "DuckDuckGo";
    public string Theme { get; set; } = "Dark";
    public bool SaveHistory { get; set; } = true;
    public bool ClearHistoryOnExit { get; set; }
    public bool TrackingPrevention { get; set; } = true;
    public bool BlockPasswordAutofill { get; set; } = true;
}
