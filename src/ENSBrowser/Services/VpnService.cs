using System.Diagnostics;

namespace ENSBrowser.Services;

public sealed class VpnService
{
    public async Task<IReadOnlyList<string>> ListProfilesAsync(CancellationToken cancellationToken)
    {
        var output = await RunCommandAsync("/enum", cancellationToken);
        return output
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
            .Select(line => line[(line.IndexOf(':') + 1)..].Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<string> GetStatusAsync(CancellationToken cancellationToken)
        => await RunCommandAsync(string.Empty, cancellationToken);

    public void OpenWindowsSettings()
    {
        Process.Start(new ProcessStartInfo("ms-settings:network-vpn") { UseShellExecute = true });
    }

    public async Task<string> ConnectAsync(string profileName, CancellationToken cancellationToken)
        => await RunRasDialAsync(profileName, cancellationToken);

    public async Task<string> DisconnectAsync(string profileName, CancellationToken cancellationToken)
        => await RunRasDialAsync(profileName, cancellationToken, disconnect: true);

    private static async Task<string> RunRasDialAsync(
        string profileName,
        CancellationToken cancellationToken,
        bool disconnect = false)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Enter the Windows VPN profile name.", nameof(profileName));

        var arguments = disconnect
            ? $"\"{profileName}\" /disconnect"
            : $"\"{profileName}\"";
        return await RunCommandAsync(arguments, cancellationToken);
    }

    private static async Task<string> RunCommandAsync(string arguments, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "rasdial.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output : error);

        return string.IsNullOrWhiteSpace(output) ? "VPN command completed." : output.Trim();
    }
}
