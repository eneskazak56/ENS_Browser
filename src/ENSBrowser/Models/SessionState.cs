namespace ENSBrowser.Models;

public sealed record SessionTab(string Id, string Title, string Url);

public sealed class SessionState
{
    public List<SessionTab> Tabs { get; set; } = [];
    public int SelectedTabIndex { get; set; }
}
