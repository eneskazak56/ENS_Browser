using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using CefSharp;
using CefSharp.Wpf;
using ENSBrowser.Models;
using ENSBrowser.Services;

namespace ENSBrowser;

public partial class MainWindow : Window
{
    private readonly BrowserStore _store = new();
    private readonly PreferencesStore _preferencesStore = new();
    private readonly SessionStateService _sessionStore = new();
    private readonly DownloadStore _downloadStore = new();
    private readonly PageCommandService _pageCommands = new();
    private PrivacyService _privacyService = new();
    private readonly ObservableCollection<BrowserTab> _tabs = [];
    private BrowserPreferences _preferences = new();
    private bool _darkTheme = true;

    public MainWindow()
    {
        InitializeComponent();
        Tabs.ItemsSource = _tabs;
        Loaded += async (_, _) =>
        {
            try
            {
                _preferences = await _preferencesStore.LoadAsync();
                _privacyService = new PrivacyService(
                    _preferences.TrackingPrevention, _preferences.ClearHistoryOnExit);
                ApplyTheme(_preferences.Theme);
                _privacyService = new PrivacyService(
                    _preferences.TrackingPrevention, _preferences.ClearHistoryOnExit);
                await _store.LoadAsync();
                await _downloadStore.LoadAsync();
                var session = await _sessionStore.LoadAsync();
                if (session.Tabs.Count > 0)
                {
                    foreach (var savedTab in session.Tabs)
                        _tabs.Add(new BrowserTab { Id = savedTab.Id, Title = savedTab.Title, Url = savedTab.Url });
                    Tabs.SelectedIndex = Math.Clamp(session.SelectedTabIndex, 0, _tabs.Count - 1);
                }
                else
                {
                    AddTab();
                    CurrentTab!.Url = _preferences.HomePage;
                }
                AddressBox.Text = string.Empty;
            }
            catch (IOException exception)
            {
                StatusText.Text = $"Could not load local browser state: {exception.Message}";
            }
        };
    }

    private BrowserTab? CurrentTab => Tabs.SelectedItem as BrowserTab;
    private ChromiumWebBrowser? CurrentView => FindVisualChild<ChromiumWebBrowser>(Tabs);

    private void AddTab()
    {
        var tab = new BrowserTab { Id = Guid.NewGuid().ToString("N") };
        _tabs.Add(tab);
        Tabs.SelectedItem = tab;
    }

    private void NewTab_Click(object sender, RoutedEventArgs e) => AddTab();

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is not BrowserTab tab)
            return;

        var index = _tabs.IndexOf(tab);
        _tabs.Remove(tab);
        if (_tabs.Count == 0)
            AddTab();
        else
            Tabs.SelectedIndex = Math.Min(index, _tabs.Count - 1);
    }

    private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != Tabs || CurrentTab is null)
            return;
        AddressBox.Text = CurrentTab.Url is "about:blank"
            ? string.Empty
            : CurrentTab.Url;
    }

    private void AddressBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Navigate(AddressBox.Text);
    }

    private void Navigate(string input)
    {
        if (CurrentTab is null || CurrentView is null)
            return;

        var url = NavigationService.ResolveAddress(input, _preferences.SearchEngine);
        CurrentTab.Url = url;
        CurrentTab.Title = "Loading…";
        CurrentView.Load(url);
        StatusText.Text = "Navigating securely…";
    }

    private void Back_Click(object sender, RoutedEventArgs e) => CurrentView?.Back();
    private void Forward_Click(object sender, RoutedEventArgs e) => CurrentView?.Forward();
    private void Reload_Click(object sender, RoutedEventArgs e) => CurrentView?.Reload();

    private void ZoomIn_Click(object sender, RoutedEventArgs e) => ChangeZoom(0.1);
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => ChangeZoom(-0.1);

    private void ChangeZoom(double amount)
    {
        _pageCommands.SetZoom(CurrentView, _pageCommands.GetZoom(CurrentView) + amount);
        ZoomText.Text = $"{_pageCommands.GetZoom(CurrentView):P0}";
    }

    private void ShowFind()
    {
        FindPanel.Visibility = Visibility.Visible;
        FindBox.Focus();
        FindBox.SelectAll();
    }

    private void FindNext_Click(object sender, RoutedEventArgs e)
    {
        _pageCommands.Find(CurrentView, FindBox.Text);
        StatusText.Text = string.IsNullOrWhiteSpace(FindBox.Text) ? "Enter text to find." : $"Searching for “{FindBox.Text}”.";
    }

    private void FindBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            FindNext_Click(sender, e);
        else if (e.Key == Key.Escape)
            CloseFind_Click(sender, e);
    }

    private void CloseFind_Click(object sender, RoutedEventArgs e)
    {
        FindPanel.Visibility = Visibility.Collapsed;
        AddressBox.Focus();
    }

    private async void Bookmark_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentTab is null || CurrentTab.Url is "about:blank" || !Uri.TryCreate(CurrentTab.Url, UriKind.Absolute, out _))
            return;
        await _store.AddBookmarkAsync(CurrentTab.Title, CurrentTab.Url);
        StatusText.Text = "Bookmark saved locally.";
    }

    private async void Theme_Click(object sender, RoutedEventArgs e)
    {
        ApplyTheme(_darkTheme ? "Light" : "Dark");
        _preferences.Theme = _darkTheme ? "Dark" : "Light";
        await _preferencesStore.SaveAsync(_preferences);
        StatusText.Text = _darkTheme ? "Dark theme enabled." : "Light theme enabled.";
    }

    private void ApplyTheme(string theme)
    {
        _darkTheme = !string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase);
        Application.Current.Resources["WindowBackground"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_darkTheme ? "#0F172A" : "#F1F5F9"));
        Application.Current.Resources["PanelBackground"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_darkTheme ? "#172033" : "#FFFFFF"));
        Application.Current.Resources["PrimaryText"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_darkTheme ? "#F8FAFC" : "#0F172A"));
    }

    private void BookmarksMenu_Click(object sender, RoutedEventArgs e)
    {
        if (_store.Bookmarks.Count == 0)
        {
            StatusText.Text = "No bookmarks saved yet.";
            return;
        }

        var menu = new ContextMenu();
        foreach (var bookmark in _store.Bookmarks)
        {
            var item = new MenuItem { Header = bookmark.Title, Tag = bookmark.Url };
            item.Click += (_, _) => Navigate(bookmark.Url);
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }

    private void HistoryMenu_Click(object sender, RoutedEventArgs e)
    {
        if (_store.History.Count == 0)
        {
            StatusText.Text = "No history yet.";
            return;
        }

        var menu = new ContextMenu();
        foreach (var entry in _store.History.Take(25))
        {
            var item = new MenuItem
            {
                Header = $"{entry.Title} ({entry.VisitedAt:MMM d, HH:mm})",
                Tag = entry.Url
            };
            item.Click += (_, _) => Navigate(entry.Url);
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }

    private void DownloadsMenu_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads",
            UseShellExecute = true
        });
    }

    private void LibraryMenu_Click(object sender, RoutedEventArgs e)
    {
        var window = new LibraryWindow(_store, _downloadStore, Navigate) { Owner = this };
        window.ShowDialog();
    }

    private void PrivacyMenu_Click(object sender, RoutedEventArgs e)
    {
        var window = new PrivacyWindow(_privacyService, CurrentView) { Owner = this };
        if (window.ShowDialog() == true)
            StatusText.Text = "Privacy preferences applied.";
    }

    private async void ClearHistoryMenu_Click(object sender, RoutedEventArgs e)
    {
        await _store.ClearHistoryAsync();
        StatusText.Text = "History cleared.";
    }

    private void ServicePlaceholders_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show("These features are intentionally isolated integration points. Add providers behind explicit user consent and safe configuration.", "ENS Browser integrations");

    private void VpnMenu_Click(object sender, RoutedEventArgs e)
    {
        var window = new VpnWindow { Owner = this };
        window.ShowDialog();
    }

    private async void SettingsMenu_Click(object sender, RoutedEventArgs e)
    {
        var window = new SettingsWindow(_preferences) { Owner = this };
        if (window.ShowDialog() != true)
            return;

        ApplyTheme(_preferences.Theme);
        StatusText.Text = "Settings saved.";
        if (CurrentTab is { Url: "about:blank" })
        {
            CurrentTab.Url = _preferences.HomePage;
            if (CurrentView is not null)
                CurrentView.Load(_preferences.HomePage);
        }
        await _preferencesStore.SaveAsync(_preferences);
    }

    private void AiTools_Click(object sender, RoutedEventArgs e)
    {
        var window = new AiToolsWindow { Owner = this };
        window.ShowDialog();
    }

    private void BrowserHost_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Grid host || host.Children.Count != 0 ||
            host.DataContext is not BrowserTab tab)
            return;

        var view = new ChromiumWebBrowser(tab.Url);
        view.DataContext = tab;
        view.RequestHandler = new SafeRequestHandler(message =>
            Dispatcher.BeginInvoke(() => StatusText.Text = message));
        view.DownloadHandler = new BrowserDownloadHandler(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            record =>
            {
                _downloadStore.Items.Insert(0, record);
                _ = _downloadStore.SaveAsync();
                Dispatcher.BeginInvoke(() => StatusText.Text = $"Downloading {record.FileName}…");
            },
            (item, record) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    record.State = item.IsCancelled ? "Cancelled" : item.IsComplete ? "Completed" : "In progress";
                    record.ReceivedBytes = item.ReceivedBytes;
                    record.TotalBytes = item.TotalBytes;
                    if (item.IsComplete)
                        StatusText.Text = $"Downloaded {record.FileName}";
                    _ = _downloadStore.SaveAsync();
                });
            });
        view.IsBrowserInitializedChanged += BrowserView_IsBrowserInitializedChanged;
        view.AddressChanged += BrowserView_AddressChanged;
        view.LoadingStateChanged += BrowserView_LoadingStateChanged;
        view.TitleChanged += BrowserView_TitleChanged;
        host.Children.Add(view);
    }

    private void BrowserView_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ChromiumWebBrowser view && view.IsBrowserInitialized)
            _privacyService.Apply(view);
    }

    private void BrowserView_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not ChromiumWebBrowser view || string.IsNullOrWhiteSpace(view.Address))
            return;

        var tab = view.DataContext as BrowserTab ?? CurrentTab;
        if (tab is null)
            return;

        tab.Url = view.Address;
        if (ReferenceEquals(tab, CurrentTab))
        {
            AddressBox.Text = view.Address;
            StatusText.Text = "Loading…";
        }
        if (_preferences.SaveHistory)
            _ = _store.AddHistoryAsync(tab.Title, view.Address);
    }

    private void BrowserView_LoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
    {
        if (sender is not ChromiumWebBrowser view)
            return;

        Dispatcher.BeginInvoke(() =>
        {
            if (ReferenceEquals(view.DataContext as BrowserTab ?? CurrentTab, CurrentTab))
                StatusText.Text = e.IsLoading ? "Loading…" : "Ready";
        });
    }

    private void BrowserView_TitleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not ChromiumWebBrowser view)
            return;

        var tab = view.DataContext as BrowserTab ?? CurrentTab;
        if (tab is not null)
            tab.Title = string.IsNullOrWhiteSpace(view.Title) ? tab.Url : view.Title;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (KeyboardShortcutService.IsCtrl(e, Key.L))
        {
            AddressBox.Focus();
            AddressBox.SelectAll();
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.T))
        {
            AddTab();
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.W))
        {
            CloseCurrentTab();
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.F))
        {
            ShowFind();
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.R))
        {
            Reload_Click(sender, e);
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrlShift(e, Key.Delete))
        {
            _ = ClearAllBrowsingDataAsync();
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.OemPlus) || KeyboardShortcutService.IsCtrl(e, Key.Add))
        {
            ChangeZoom(0.1);
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.OemMinus) || KeyboardShortcutService.IsCtrl(e, Key.Subtract))
        {
            ChangeZoom(-0.1);
            e.Handled = true;
        }
        else if (KeyboardShortcutService.IsCtrl(e, Key.D0))
        {
            _pageCommands.ResetZoom(CurrentView);
            ZoomText.Text = "100%";
            e.Handled = true;
        }
    }

    private void CloseCurrentTab()
    {
        if (CurrentTab is null)
            return;
        var index = _tabs.IndexOf(CurrentTab);
        _tabs.Remove(CurrentTab);
        if (_tabs.Count == 0)
            AddTab();
        else
            Tabs.SelectedIndex = Math.Min(index, _tabs.Count - 1);
    }

    private async Task ClearAllBrowsingDataAsync()
    {
        await _store.ClearHistoryAsync();
        if (CurrentView is not null)
            await _privacyService.ClearSessionDataAsync(CurrentView);
        StatusText.Text = "Browsing data cleared.";
    }

    /*
    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        var bookmarks = _store.Bookmarks.Count;
        var history = _store.History.Count;
        MessageBox.Show(
            $"Local browser data\n\nBookmarks: {bookmarks}\nHistory entries: {history}\nDownloads: Chromium download handling is enabled.\n\nService placeholders: visual search, maps, shopping, video, VPN.",
            "ENS Browser");
    }
    */

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
                return match;

            var descendant = FindVisualChild<T>(child);
            if (descendant is not null)
                return descendant;
        }

        return null;
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        try
        {
            await _sessionStore.SaveAsync(_tabs, Tabs.SelectedIndex);
            if (_preferences.ClearHistoryOnExit)
                await _store.ClearHistoryAsync();
            await _store.SaveAsync();
            await _downloadStore.SaveAsync();
        }
        catch (IOException)
        {
            // Closing cannot be blocked by optional local-state persistence.
        }
    }
}
