using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Rendering;

namespace XFsm;
public partial class Plugin
{
    private Patch _patch;
    private Hook<D3DPERF_GetStatus>? _hook;
    private delegate uint D3DPERF_GetStatus();

    public void OnWinMain_()
    {
        if (!Renderer.IsDirectX12)
        {
            var d3d9 = LoadLibrary("d3d9.dll");
            if (d3d9 == 0)
            {
                Log.Error("Failed to load d3d9.dll");
                return;
            }

            var getStatus = GetProcAddress(d3d9, "D3DPERF_GetStatus");
            if (getStatus == 0)
            {
                Log.Error("Failed to get D3DPERF_GetStatus");
                return;
            }

            _hook = Hook.Create<D3DPERF_GetStatus>(getStatus, () => 0);
            _patch = new Patch((nint)0x142597357, [0x48, 0x31, 0xc0, 0x90, 0x90, 0x90, 0x90], true);
        }

        Log.Info("Attempting to inject RenderDoc...");

        KeyBindings.AddKeybind("DoCapture", new Keybind<Key>(Key.P, [Key.LeftControl, Key.LeftShift]));
        if (!ImGuiNodeEditor.InternalCalls.InjectRenderDoc(@"C:\Program Files\RenderDoc\renderdoc.dll"))
        {
            Log.Error("Failed to inject RenderDoc");
            return;
        }

        Log.Info("RenderDoc injected");
    }

    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint LoadLibrary(string path);

    [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetProcAddress(nint module, string name);
}
