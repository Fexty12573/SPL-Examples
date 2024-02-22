using SharpPluginLoader.Core;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IconFonts;
using XFsm.ImGuiNodeEditor;
using ImGuiNET;
using SharpPluginLoader.Core.Memory;
using Microsoft.CodeAnalysis;
using SharpPluginLoader.Core.MtTypes;
using SharpPluginLoader.Core.Rendering;

namespace XFsm;

using NodeEditor = InternalCalls;
using FA6 = FontAwesome6;

public class XFsmEditor
{
    private AIFSM? _fsm;
    private nint _ctx;

    private readonly List<XFsmNode> _nodes = [];
    private readonly List<XFsmLink> _links = [];

    private TextureHandle _blueprintHeaderBg;
    private uint _headerBgWidth;
    private uint _headerBgHeight;

    private readonly Random _random = new();
    private float _lR = 2f;
    private float _lA = 100f;
    private bool _applyLayout = false;
    private float _maxArea = 130f;

    private float _rF = 1000f;
    private float _l = 500f;
    private float _sF = 0.001f;

    #region Allocators
    private MtAllocator _nodeAllocator = null!;
    private MtAllocator _clusterAllocator = null!;
    #endregion

    public void SetFsm(AIFSM fsm)
    {
        if (_ctx == 0)
        {
            NodeEditor.SetCurrentImGuiContext(ImGui.GetCurrentContext());
            _ctx = NodeEditor.CreateEditor();
            
            NodeEditor.SetCurrentEditor(_ctx);

            ref var style = ref NodeEditor.GetStyle();
            style.NodeRounding = 9f;
            style.NodeBorderWidth = 1f;
            style.HoveredNodeBorderWidth = 3.5f;
            style.HoveredNodeBorderOffset = 0f;
            style.SelectedNodeBorderWidth = 3.5f;
            style.SelectedNodeBorderOffset = 0f;
            style.NodePadding = new Vector4(12, 12, 12, 12);

            style.Colors[(int)StyleColor.NodeBg] = ImGui.ColorConvertU32ToFloat4(0xFF202020);
            style.Colors[(int)StyleColor.NodeBorder] = ImGui.ColorConvertU32ToFloat4(0xFFFFFFFF);
            
            NodeEditor.SetCurrentEditor(0);

            _blueprintHeaderBg = Renderer.LoadTexture("nativePC/plugins/CSharp/XFsm/Assets/BlueprintBackground.png", out _headerBgWidth, out _headerBgHeight);
            if (_blueprintHeaderBg == TextureHandle.Invalid)
            {
                Log.Error("Failed to load BlueprintBackground.png");
            }

            _nodeAllocator = NodeEditor.GetAllocator(MtDti.Find("cAIFSMNode"));
            Ensure.NotNull(_nodeAllocator);

            _clusterAllocator = NodeEditor.GetAllocator(MtDti.Find("cAIFSMCluster"));
            Ensure.NotNull(_clusterAllocator);
        }

        if (fsm.RootCluster is null)
            return;

        _fsm = fsm;

        _nodes.Clear();
        _links.Clear();

        // Create nodes
        foreach (var node in fsm.RootCluster.Nodes)
        {
            _nodes.Add(new XFsmNode(node));
        }

        // Create links
        foreach (var node in _nodes)
        {
            foreach (ref var link in node.BackingNode.Links)
            {
                var target = GetNodeById(link.DestinationNodeId);
                if (target is null)
                    continue;

                _links.Add(new XFsmLink(node.OutputPin, target.InputPin, ref link));
            }
        }

        Log.Info($"Successfully loaded FSM into Editor");
        Log.Info($"Nodes: {_nodes.Count}, Links: {_links.Count}");
    }

    public void Render()
    {
        if (_fsm is null)
        {
            ImGui.TextColored(new Vector4(1, .33f, .33f, 1), $"{FA6.Xmark} No FSM loaded");
            return;
        }
        
        NodeEditor.SetCurrentEditor(_ctx);

        if (ImGui.Button("Randomize Positions"))
        {
            _applyLayout = false;

            foreach (var node in _nodes)
            {
                node.Position = new Vector2(
                    (_random.NextSingle() - 0.5f) * _maxArea,
                    (_random.NextSingle() - 0.5f) * _maxArea
                );

                NodeEditor.SetNodePosition(node.Id, node.Position);
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Navigate to Content"))
        {
            NodeEditor.NavigateToContent();
        }

        ImGui.SameLine();

        var availX = ImGui.GetContentRegionAvail().X;

        ImGui.SetNextItemWidth(availX * 0.2f);
        ImGui.DragFloat("Lrep", ref _lR, 0.1f);

        ImGui.SameLine();

        ImGui.SetNextItemWidth(availX * 0.2f);
        ImGui.DragFloat("Latt", ref _lA, 0.1f);

        ImGui.SameLine();

        ImGui.SetNextItemWidth(availX * 0.15f);
        ImGui.DragFloat("Max Area", ref _maxArea);

        ImGui.Separator();

        ImGui.Text("Custom Algorithm");

        ImGui.SameLine();

        ImGui.SetNextItemWidth(availX * 0.2f);
        ImGui.DragFloat("Repulsion Force", ref _rF, 0.1f);

        ImGui.SameLine();

        ImGui.SetNextItemWidth(availX * 0.2f);
        ImGui.DragFloat("Spring Length", ref _l, 0.5f);

        ImGui.SameLine();
        
        ImGui.SetNextItemWidth(availX * 0.2f);
        ImGui.DragFloat("Spring Force", ref _sF, 0.1f);

        if (ImGui.Button("Apply Layout"))
        {
            SpringEmbedder.Layout(
                _nodes, CollectionsMarshal.AsSpan(_links),
                _rF, _l, _sF
            );

            foreach (var node in _nodes)
            {
                NodeEditor.SetNodePosition(node.Id, node.Position);
            }
        }

        NodeEditor.Begin("XFSM Editor");

        var style = ImGui.GetStyle();
        var drawList = ImGui.GetWindowDrawList();
        var builder = new NodeBuilder(_blueprintHeaderBg, _headerBgWidth, _headerBgHeight);

        foreach (var node in _nodes)
        {
            builder.Begin(node.Id);
            builder.Header();
            {
                ImGui.Spring(0);
                ImGui.TextUnformatted(node.Name);
                ImGui.Spring(1);
                ImGui.Dummy(new Vector2(0, 28));
            }
            builder.EndHeader();

            builder.Input(node.InputPin.Id);
            {
                ImGui.TextUnformatted(node.InputPin.Name);
                ImGui.Spring(0);
            }
            builder.EndInput();

            builder.Middle();
            {
                ImGui.Spring(1, 0);
                ImGui.TextUnformatted("Middle");
                ImGui.Spring(1, 0);
            }

            builder.Output(node.OutputPin.Id);
            {
                ImGui.TextUnformatted(node.OutputPin.Name);
                ImGui.Spring(0);
            }
            builder.EndOutput();
            
            builder.End();
        }

        foreach (var link in _links)
        {
            NodeEditor.Link(link.Id, link.Source.Id, link.Target.Id);
        }

        //if (NodeEditor.BeginCreate(new Vector4(1, 1, 1, 1), 2.0f))
        //{
        //    if (NodeEditor.QueryNewLink(out var startPinId, out var endPinId))
        //    {
        //        var startPin = GetPinById(startPinId);
        //        var endPin = GetPinById(endPinId);

        //        if (startPin is XFsmInputPin && endPin is XFsmOutputPin)
        //        {
        //            (startPin, endPin) = (endPin, startPin);
        //        }

        //        if (startPin is not null && endPin is not null)
        //        {
        //            if (startPin == endPin)
        //            {
        //                NodeEditor.RejectNewItemEx(new Vector4(1, 0, 0, 1), 2.0f);
        //            }
        //            else if (startPin.GetType() == endPin.GetType())
        //            {
        //                ShowLabel($"{FA6.Cross} Incompatible Pin Kind", new MtColor(45, 32, 32, 180));
        //                NodeEditor.RejectNewItemEx(new Vector4(1, 0, 0, 1), 2.0f);
        //            }
        //            else
        //            {
        //                ShowLabel($"{FA6.Plus} Create Link", new MtColor(32, 45, 32, 180));
        //                if (NodeEditor.AcceptNewItemEx(new Vector4(0.5f, 1, 0.5f, 1), 4.0f))
        //                {
        //                    _links.Add(new XFsmLink((XFsmOutputPin)startPin, (XFsmInputPin)endPin, ref Unsafe.NullRef<AIFSMLink>()));
        //                }
        //            }
        //        }
        //    }

        //    NodeEditor.EndCreate();
        //}

        NodeEditor.End();
        NodeEditor.SetCurrentEditor(0);

        return;

        void ShowLabel(string label, MtColor color)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetTextLineHeight());
            var size = ImGui.CalcTextSize(label);

            var padding = style.FramePadding;
            var spacing = style.ItemSpacing;

            ImGui.SetCursorPos(ImGui.GetCursorPos() + spacing with { Y = -spacing.Y });

            var rectMin = ImGui.GetCursorScreenPos() - padding;
            var rectMax = ImGui.GetCursorScreenPos() + size + padding;

            drawList.AddRectFilled(rectMin, rectMax, color, size.Y * 0.15f);
            ImGui.TextUnformatted(label);
        }
    }

    private void ShowSidePanel()
    {
        ImGui.Begin("Properties");



        ImGui.End();
    }

    private void ShowStyleEditor()
    {
        ImGui.Begin("Node Editor Style");
        {
            ref var style = ref NodeEditor.GetStyle();
            ImGui.DragFloat4("Node Padding", ref style.NodePadding);
            ImGui.DragFloat("Node Rounding", ref style.NodeRounding);
            ImGui.DragFloat("Node Border Width", ref style.NodeBorderWidth);
            ImGui.DragFloat("Hovered Node Border Width", ref style.HoveredNodeBorderWidth);
            ImGui.DragFloat("Hovered Node Border Offset", ref style.HoveredNodeBorderOffset);
            ImGui.DragFloat("Selected Node Border Width", ref style.SelectedNodeBorderWidth);
            ImGui.DragFloat("Selected Node Border Offset", ref style.SelectedNodeBorderOffset);
            ImGui.DragFloat("Pin Rounding", ref style.PinRounding);
            ImGui.DragFloat("Pin Border Width", ref style.PinBorderWidth);
            ImGui.DragFloat("Link Strength", ref style.LinkStrength);
            ImGui.DragFloat2("Source Direction", ref style.SourceDirection);
            ImGui.DragFloat2("Target Direction", ref style.TargetDirection);
            ImGui.DragFloat("Scroll Duration", ref style.ScrollDuration);
            ImGui.DragFloat("Flow Marker Distance", ref style.FlowMarkerDistance);
            ImGui.DragFloat("Flow Speed", ref style.FlowSpeed);
            ImGui.DragFloat("Flow Duration", ref style.FlowDuration);
            ImGui.DragFloat2("Pivot Alignment", ref style.PivotAlignment);
            ImGui.DragFloat2("Pivot Size", ref style.PivotSize);
            ImGui.DragFloat2("Pivot Scale", ref style.PivotScale);
            ImGui.DragFloat("Pin Corners", ref style.PinCorners);
            ImGui.DragFloat("Pin Radius", ref style.PinRadius);
            ImGui.DragFloat("Pin Arrow Size", ref style.PinArrowSize);
            ImGui.DragFloat("Pin Arrow Width", ref style.PinArrowWidth);
            ImGui.DragFloat("Group Rounding", ref style.GroupRounding);
            ImGui.DragFloat("Group Border Width", ref style.GroupBorderWidth);

            for (var i = 0; i < (int)StyleColor.Count; i++)
            {
                ImGui.ColorEdit4($"Color{i}", ref style.Colors[i]);
            }
        }
        ImGui.End();
    }

    private void ShowNode(XFsmNode node)
    {
        NodeEditor.BeginNode(node.Id);
        ImGui.Text(node.Name);


        var outputPinName = $"{node.OutputPin.Name} {FA6.SquareCaretRight}";
        NodeEditor.PushStyleVarVec2(StyleVar.PivotAlignment, new Vector2(0f, .5f));
        NodeEditor.BeginPin(node.InputPin.Id, PinKind.Input);
        ImGui.TextColored(new Vector4(1f, 1f, 0.3f, 1f), $"{FA6.SquareCaretRight} {node.InputPin.Name}");
        NodeEditor.EndPin();

        NodeEditor.PopStyleVar();

        ImGui.SameLine();

        NodeEditor.PushStyleVarVec2(StyleVar.PivotAlignment, new Vector2(1f, .5f));

        // Right-align the output pin
        var textLength = ImGui.CalcTextSize(outputPinName).X;
        var nodeNameLength = ImGui.CalcTextSize(node.Name).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - textLength);


        NodeEditor.BeginPin(node.OutputPin.Id, PinKind.Output);
        ImGui.TextColored(new Vector4(0.3f, 1f, 1f, 1f), $"{node.OutputPin.Name} {FA6.SquareCaretRight}");
        NodeEditor.EndPin();

        NodeEditor.PopStyleVar();

        // Draw info about the node
        // ...

        NodeEditor.EndNode();
    }

    private XFsmNode? GetNodeById(int id)
    {
        return _nodes.FirstOrDefault(x => x.Id == id);
    }

    private XFsmPin? GetPinById(nint id)
    {
        foreach (var node in _nodes)
        {
            if (node.InputPin.Id == id)
                return node.InputPin;

            if (node.OutputPin.Id == id)
                return node.OutputPin;
        }

        return null;
    }

    public static nint MakeLinkId(XFsmNode source, XFsmNode target)
    {
        return (nint)((long)source.Id << 32 | (uint)target.Id);
    }

    public static int MakeOutputPinId(AIFSMNode parent)
    {
        return (1 << 29) | parent.Id;
    }

    public static int MakeInputPinId(AIFSMNode parent)
    {
        return (1 << 30) | parent.Id;
    }
}

public class XFsmNode
{
    public int Id => BackingNode.Id;
    public string Name { get; set; }
    public Vector2 Position { get; set; }

    public XFsmInputPin InputPin { get; }
    public XFsmOutputPin OutputPin { get; }

    public AIFSMNode BackingNode { get; }


    public XFsmNode(AIFSMNode node)
    {
        BackingNode = node;
        Name = node.Name;
        InputPin = new XFsmInputPin(this);
        OutputPin = new XFsmOutputPin(this);
    }
}

public class XFsmPin(string name, int id)
{
    public int Id { get; } = id;
    public string Name { get; set; } = name;
}

public class XFsmInputPin(XFsmNode parent) : XFsmPin("Input", XFsmEditor.MakeInputPinId(parent.BackingNode))
{
    public XFsmNode Parent { get; } = parent;
}

public class XFsmOutputPin(XFsmNode parent) : XFsmPin("Output", XFsmEditor.MakeOutputPinId(parent.BackingNode))
{
    public XFsmNode Parent { get; } = parent;
}

public class XFsmLink(XFsmOutputPin source, XFsmInputPin target, ref AIFSMLink link)
{
    public nint Id { get; } = XFsmEditor.MakeLinkId(source.Parent, target.Parent);
    public XFsmOutputPin Source { get; } = source;
    public XFsmInputPin Target { get; } = target;
    public AIFSMLink BackingLink { get; } = link;
}
