using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Resources;

namespace ShellLogger;

public class Plugin : IPlugin
{
    public string Name => "Shell Logger";
    public string Author => "Fexty";

    public void OnLoad()
    {
        var pattern = Pattern.FromString("4D 8B F9 4D 8B E8 4C 8B F2 48 8B F9 48 85 C9");
        var result = PatternScanner.FindFirst(pattern);
        if (result == 0)
        {
            Log.Error("Failed to find Shell Create Hook");
            return;
        }

        Log.Debug($"Shell Create Hook: 0x{result - 29:X}");
        _shellCreateHook = Hook.Create<CreateShellDelegate>(result - 29, OnShellCreate);
    }

    private nint OnShellCreate(nint shlp, nint parent1, nint parent2, nint shellParams, int index1, int index2, byte register)
    {
        if (shlp != 0)
        {
            var source = parent1 != 0
                ? new Entity(parent1)
                : parent2 != 0
                    ? new Entity(parent2)
                    : null;

            var shell = new ShellParam(shlp);

            string sourceStr;
            if (source is not null)
            {
                var dtiName = source.GetDti()?.Name ?? "Unknown";
                var objectName = source.Is("cUnit") ? source.Name : "Unknown";
                sourceStr = $"from {dtiName} ({objectName})";
            }
            else
            {
                sourceStr = "without Parent";
            }

            Log.Info($"Creating Shell {shell.FilePath} {sourceStr}");
        }

        return _shellCreateHook.Original(shlp, parent1, parent2, shellParams, index1, index2, register);
    }

    private Hook<CreateShellDelegate> _shellCreateHook = null!;
    private delegate nint CreateShellDelegate(nint shlp, nint parent1, nint parent2, 
        nint shellParams, int index1, int index2, byte register);
}
