using SharpPluginLoader.Core;
using ImGuiNET;

namespace XFsm;

public partial class Plugin : IPlugin
{
    public string Name => "XFsm";
    public string Author => "Fexty";

    public PluginData Initialize()
    {
        return new PluginData
        {
            OnImGuiFreeRender = true
        };
    }

    public void OnLoad()
    {
        InjectProperties();
    }

    public void OnImGuiFreeRender()
    {
        if (!ImGui.Begin("XFsm", ImGuiWindowFlags.MenuBar))
            goto Exit;

        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open..", "Ctrl+O")) { }
                if (ImGui.MenuItem("Save", "Ctrl+S")) { }
                if (ImGui.MenuItem("Close", "Ctrl+W")) { }
                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }

        

    Exit:
        ImGui.End();
    }
}
