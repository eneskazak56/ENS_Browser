using System.Windows;
using ENSBrowser.Services;

namespace ENSBrowser;

public partial class VpnWindow : Window
{
    private readonly VpnService _vpnService = new();

    public VpnWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshProfilesAsync();
    }

    private async void RefreshProfiles_Click(object sender, RoutedEventArgs e) => await RefreshProfilesAsync();

    private async Task RefreshProfilesAsync()
    {
        try
        {
            ProfilesBox.ItemsSource = await _vpnService.ListProfilesAsync(CancellationToken.None);
            if (ProfilesBox.Items.Count > 0)
                ProfilesBox.SelectedIndex = 0;
            StatusBox.Text = ProfilesBox.Items.Count == 0
                ? "No rasdial profiles were found. Create one in Windows Settings."
                : $"{ProfilesBox.Items.Count} VPN profile(s) found.";
        }
        catch (Exception exception)
        {
            StatusBox.Text = $"Could not enumerate VPN profiles: {exception.Message}";
        }
    }

    private void ProfilesBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ProfilesBox.SelectedItem is string profile)
            ProfileBox.Text = profile;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _vpnService.OpenWindowsSettings();
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "VPN settings");
        }
    }

    private async void Connect_Click(object sender, RoutedEventArgs e) => await RunAsync(false);
    private async void Disconnect_Click(object sender, RoutedEventArgs e) => await RunAsync(true);

    private async Task RunAsync(bool disconnect)
    {
        try
        {
            var result = disconnect
                ? await _vpnService.DisconnectAsync(ProfileBox.Text.Trim(), CancellationToken.None)
                : await _vpnService.ConnectAsync(ProfileBox.Text.Trim(), CancellationToken.None);
            StatusBox.Text = result;
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "VPN command failed");
        }
    }

    private async void Status_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusBox.Text = await _vpnService.GetStatusAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            StatusBox.Text = exception.Message;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
