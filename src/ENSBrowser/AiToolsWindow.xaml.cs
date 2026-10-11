using System.Windows;
using ENSBrowser.Services;

namespace ENSBrowser;

public partial class AiToolsWindow : Window
{
    private readonly AiService _aiService = new();

    public AiToolsWindow()
    {
        InitializeComponent();
        PromptBox.Text = "Summarize the page I am reading.";
    }

    private async void Ask_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PromptBox.Text))
        {
            ResultBox.Text = "Enter a prompt first.";
            return;
        }

        try
        {
            ResultBox.Text = "Thinking…";
            ResultBox.Text = await _aiService.AskAsync(EndpointBox.Text.Trim(), PromptBox.Text.Trim(), CancellationToken.None);
        }
        catch (Exception exception)
        {
            ResultBox.Text = $"AI request failed: {exception.Message}\n\nSet ENS_AI_API_KEY and optionally ENS_AI_MODEL before launching the app.";
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
