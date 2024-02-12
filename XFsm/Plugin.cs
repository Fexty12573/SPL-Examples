using SharpPluginLoader.Core;
using ImGuiNET;
using SharpPluginLoader.Core.Rendering;

namespace XFsm;

public partial class Plugin : IPlugin
{
    public string Name => "XFsm";
    public string Author => "Fexty";

    private XFsmEditor _editor = null!;

    private bool _showStyleEditor = false;

    public PluginData Initialize()
    {
        return new PluginData
        {
            OnImGuiFreeRender = true,
            OnWinMain = false,
            OnUpdate = false
        };
    }

    public void OnLoad()
    {
        InjectProperties();

        _editor = new XFsmEditor();
    }

    public void OnImGuiFreeRender()
    {
        if (!Renderer.MenuShown)
            return;
        
        if (!ImGui.Begin("XFsm"))
            goto Exit;

        if (ImGui.Button("Open.."))
        {
            var file = ResourceManager.GetResource<AIFSM>(@"quest\q01503\fsm\01503_main", MtDti.Find("rAIFSM")!);
            if (file is not null)
                _editor.SetFsm(file);
        }

        ImGui.SameLine();
        ImGui.Checkbox("Style Editor", ref _showStyleEditor);

        _editor.Render();

        Exit:
        ImGui.End();
    }
}
