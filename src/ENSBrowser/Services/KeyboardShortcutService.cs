using System.Windows.Input;

namespace ENSBrowser.Services;

public static class KeyboardShortcutService
{
    public static bool Is(KeyEventArgs args, Key key, ModifierKeys modifiers)
        => args.Key == key && Keyboard.Modifiers == modifiers;

    public static bool IsCtrl(KeyEventArgs args, Key key) => Is(args, key, ModifierKeys.Control);
    public static bool IsCtrlShift(KeyEventArgs args, Key key) => Is(args, key, ModifierKeys.Control | ModifierKeys.Shift);
    public static bool IsAlt(KeyEventArgs args, Key key) => Is(args, key, ModifierKeys.Alt);
}
