using SharpPluginLoader.Core;
using System.Numerics;
using XFsm.ImGuiNodeEditor;
using ImGuiNET;

namespace XFsm;

using NodeEditor = InternalCalls;

public class XFsmEditor
{
    private AIFSM? _fsm;
    private nint _ctx;

    public XFsmEditor()
    {
        _ctx = NodeEditor.CreateEditor();
    }

    public void SetFsm(AIFSM fsm)
    {
        _fsm = fsm;
    }

    public void Render()
    {
        if (_fsm is null)
        {
            ImGui.TextColored(new Vector4(1, .33f, .33f, 1), "No FSM loaded");
            return;
        }
        
    }
}

public class XFsmNode(AIFSMNode node)
{
    public int Id => Node.Id;
    public AIFSMNode Node { get; } = node;
}
