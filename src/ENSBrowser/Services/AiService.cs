using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace ENSBrowser.Services;

public sealed class AiService
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(45) };

    public async Task<string> AskAsync(string endpoint, string prompt, CancellationToken cancellationToken)
    {
        var apiKey = Environment.GetEnvironmentVariable("ENS_AI_API_KEY");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var payload = new
        {
            model = Environment.GetEnvironmentVariable("ENS_AI_MODEL") ?? "default",
            messages = new[] { new { role = "user", content = prompt } }
        };
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"AI service returned {(int)response.StatusCode}: {body}");

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.TryGetProperty("choices", out var choices) &&
            choices.GetArrayLength() > 0 &&
            choices[0].TryGetProperty("message", out var message) &&
            message.TryGetProperty("content", out var content))
            return content.GetString() ?? body;

        return body;
    }
}
