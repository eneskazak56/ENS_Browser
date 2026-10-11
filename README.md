# ENS Browser

ENS Browser is a practical Windows desktop browser MVP built with WPF and a bundled CefSharp Chromium engine.

## Features

- Real embeddable Chromium-based engine through CefSharp.Wpf 152.0.100.
- Tabbed browsing, address/search bar, back/forward/reload, and new-tab controls.
- HTTP/HTTPS navigation policy for browser-controlled navigations; unsupported schemes are blocked.
- Local bookmarks and bounded history stored under `%LOCALAPPDATA%\ENSBrowser`.
- Preferences and restorable tab state are stored as `preferences.json` and `session.json` under `%LOCALAPPDATA%\ENSBrowser`.
- Bookmark/history menus, history clearing, and a Downloads-folder shortcut.
- Download handling routed to the user's Windows Downloads folder.
- A library window for searching/handling bookmarks, history, and download records.
- Session restore for open tabs and selected tab, with persisted home page and search engine preferences.
- Find-in-page, page zoom controls, keyboard shortcuts, and a clear-browsing-data shortcut.
- Privacy controls for tracking prevention, cache/cookie clearing, autofill, and password autosave.
- Light/dark theme toggle.
- Settings window for the home page, search engine, theme, history retention, and tracking prevention.
- Session restore for open tabs and the selected tab across launches.
- Clearly labeled integration points for AI tools, downloads, visual search, maps, shopping, video, and VPN connections.
- AI tools dialog for OpenAI-compatible endpoints, with `ENS_AI_API_KEY` and optional `ENS_AI_MODEL` environment configuration.
- Windows VPN dialog that opens VPN settings and uses the OS `rasdial.exe` command for an existing profile.
- Runs without administrator privileges.

## Build and run

Requirements:

1. Windows 10/11.
2. .NET 8 SDK.
3. A Windows x64 environment. CefSharp's Chromium runtime is bundled with the Release output.

```powershell
dotnet restore ENSBrowser.sln
dotnet build ENSBrowser.sln --configuration Release
dotnet run --project src\ENSBrowser\ENSBrowser.csproj
```

For a runnable deployment folder:

```powershell
dotnet publish src\ENSBrowser\ENSBrowser.csproj --configuration Release --runtime win-x64 --self-contained false --output publish
```

Run `publish\ENSBrowser.exe` on a Windows x64 machine; no Edge or WebView2 runtime is required.

### AI tools

The AI dialog accepts any OpenAI-compatible chat-completions endpoint. Set the key outside the app before launch; the app never writes it to browser state:

```powershell
$env:ENS_AI_API_KEY = "your-key"
$env:ENS_AI_MODEL = "your-model"
.\publish\ENSBrowser.exe
```

The endpoint can be changed in the dialog for a local provider or another compatible service.

### VPN

Create/configure a VPN connection in Windows first, then open **Menu > VPN connections**, enter its Windows profile name, and choose **Connect** or **Disconnect**. ENS Browser does not implement encryption itself; the Windows VPN service owns credentials and the tunnel.

## Security and privacy notes

The application does not bundle credentials or call external AI/VPN services. Provider configuration should be added behind an explicit settings surface and stored using Windows Credential Manager or DPAPI, not in source or plain JSON. Chromium still communicates with websites the user opens; private-by-default local state is not the same as network anonymity. A production release should add consent-driven cookie/session controls, permissions UI, updater signing, crash reporting policy, and a VPN provider implementation.

## Keyboard shortcuts

| Shortcut | Action |
| --- | --- |
| `Ctrl+L` | Focus the address bar |
| `Ctrl+T` / `Ctrl+W` | New tab / close current tab |
| `Ctrl+R` | Reload |
| `Ctrl+F` | Find in page |
| `Ctrl++` / `Ctrl+-` / `Ctrl+0` | Zoom in / out / reset |
| `Ctrl+Shift+Delete` | Clear local browsing data |

## Local data

The browser stores preferences, session state, browser history/bookmarks, and download metadata below `%LOCALAPPDATA%\ENSBrowser`. Website data remains in the bundled Chromium profile and can be cleared from the Privacy controls window.
