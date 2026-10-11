using System.Windows;
using System.Windows.Controls;
using ENSBrowser.Models;
using ENSBrowser.Services;

namespace ENSBrowser;

public partial class SettingsWindow : Window
{
    private readonly PreferencesStore _store = new();
    private readonly BrowserPreferences _preferences;

    public SettingsWindow(BrowserPreferences preferences)
    {
        InitializeComponent();
        _preferences = preferences;
        HomePageBox.Text = preferences.HomePage;
        SearchEngineBox.SelectedItem = SearchEngineBox.Items
            .Cast<ComboBoxItem>().FirstOrDefault(item => (string)item.Content == preferences.SearchEngine)
            ?? SearchEngineBox.Items.Cast<ComboBoxItem>().First(item => (string)item.Content == "DuckDuckGo");
        ThemeBox.SelectedItem = ThemeBox.Items
            .Cast<ComboBoxItem>().FirstOrDefault(item => (string)item.Content == preferences.Theme)
            ?? ThemeBox.Items[0];
        SaveHistoryBox.IsChecked = preferences.SaveHistory;
        ClearHistoryBox.IsChecked = preferences.ClearHistoryOnExit;
        TrackingPreventionBox.IsChecked = preferences.TrackingPrevention;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!Uri.TryCreate(HomePageBox.Text.Trim(), UriKind.Absolute, out var homePage) ||
            homePage.Scheme is not ("http" or "https"))
        {
            MessageBox.Show("Enter a valid HTTP or HTTPS home page.", "Invalid home page",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _preferences.HomePage = homePage.ToString();
        _preferences.SearchEngine = (SearchEngineBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Bing";
        _preferences.Theme = (ThemeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Dark";
        _preferences.SaveHistory = SaveHistoryBox.IsChecked == true;
        _preferences.ClearHistoryOnExit = ClearHistoryBox.IsChecked == true;
        _preferences.TrackingPrevention = TrackingPreventionBox.IsChecked == true;
        await _store.SaveAsync(_preferences);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
