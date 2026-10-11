namespace ENSBrowser.Services;

public static class NavigationService
{
    public static string ResolveAddress(string input, string searchEngine = "Bing")
    {
        var value = input.Trim();
        if (string.IsNullOrWhiteSpace(value))
            return "https://duckduckgo.com";

        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute) &&
            (absolute.Scheme is "http" or "https"))
            return absolute.ToString();

        if (value.Contains('.') && !value.Contains(' '))
            return $"https://{value}";

        var endpoint = searchEngine switch
        {
            "Google" => "https://www.google.com/search?q=",
            "DuckDuckGo" => "https://duckduckgo.com/?q=",
            "Brave" => "https://search.brave.com/search?q=",
            _ => "https://duckduckgo.com/?q="
        };
        return endpoint + Uri.EscapeDataString(value);
    }
}
