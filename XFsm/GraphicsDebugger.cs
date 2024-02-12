using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Memory;

namespace XFsm;
public partial class Plugin
{
    public void OnWinMain()
    {
        //_patch = new Patch((nint)0x142597361, [0x48, 0xE9, 0xC7, 0x00, 0x00, 0x00], true);
        Log.Info("Attempting to inject RenderDoc...");

        KeyBindings.AddKeybind("DoCapture", new Keybind<Key>(Key.P, [Key.LeftControl, Key.LeftShift]));
        if (!ImGuiNodeEditor.InternalCalls.InjectRenderDoc(@"C:\Program Files\RenderDoc\renderdoc.dll"))
        {
            Log.Error("Failed to inject RenderDoc");
            return;
        }

        Log.Info("RenderDoc injected");
    }

    public void OnUpdate(float deltaTime)
    {
        if (KeyBindings.IsPressed("DoCapture"))
        {
            ImGuiNodeEditor.InternalCalls.RenderDocTriggerCapture();
        }
    }
}
