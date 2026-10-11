using System.Windows;
using CefSharp.Wpf;
using ENSBrowser.Services;

namespace ENSBrowser;

public partial class PrivacyWindow : Window
{
    private readonly PrivacyService _privacyService;
    private readonly ChromiumWebBrowser? _currentView;

    public PrivacyWindow(PrivacyService privacyService, ChromiumWebBrowser? currentView)
    {
        InitializeComponent();
        _privacyService = privacyService;
        _currentView = currentView;
        TrackingBox.IsChecked = privacyService.TrackingPreventionEnabled;
        ClearOnExitBox.IsChecked = privacyService.ClearBrowsingDataOnExit;
    }

    public bool ClearOnExit => ClearOnExitBox.IsChecked == true;

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        _privacyService.TrackingPreventionEnabled = TrackingBox.IsChecked == true;
        _privacyService.ClearBrowsingDataOnExit = ClearOnExitBox.IsChecked == true;
        if (_currentView is not null)
            _privacyService.Apply(_currentView);
        StatusText.Text = "Privacy preferences applied.";
        DialogResult = true;
    }

    private async void ClearData_Click(object sender, RoutedEventArgs e)
    {
        if (_currentView?.IsBrowserInitialized != true)
        {
            StatusText.Text = "The browser engine is not ready yet.";
            return;
        }

        await _privacyService.ClearSessionDataAsync(_currentView);
        StatusText.Text = "Chromium cookies and session data were cleared.";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
