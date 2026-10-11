using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ENSBrowser.Models;

public sealed class DownloadRecord : INotifyPropertyChanged
{
    private string _state = "Started";
    private long _receivedBytes;
    private long _totalBytes;

    public required string Id { get; init; }
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required string SourceUrl { get; init; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.Now;

    public string State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    public long ReceivedBytes
    {
        get => _receivedBytes;
        set
        {
            if (SetField(ref _receivedBytes, value))
                OnPropertyChanged(nameof(ProgressPercent));
        }
    }

    public long TotalBytes
    {
        get => _totalBytes;
        set
        {
            if (SetField(ref _totalBytes, value))
                OnPropertyChanged(nameof(ProgressPercent));
        }
    }

    public double ProgressPercent => TotalBytes <= 0 ? 0 : ReceivedBytes * 100d / TotalBytes;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed record PrivacySnapshot(
    bool PasswordAutosaveDisabled,
    bool AutofillDisabled,
    bool TrackingPreventionEnabled,
    bool ClearBrowsingDataOnExit);
