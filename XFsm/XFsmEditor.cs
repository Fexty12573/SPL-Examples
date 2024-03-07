using SharpPluginLoader.Core;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GTranslate.Translators;
using GTranslate;
using IconFonts;
using XFsm.ImGuiNodeEditor;
using ImGuiNET;
using SharpPluginLoader.Core.Memory;
using Microsoft.CodeAnalysis;
using SharpPluginLoader.Core.MtTypes;
using SharpPluginLoader.Core.Rendering;
using System.Reflection;

namespace XFsm;

using NodeEditor = InternalCalls;
using FA6 = FontAwesome6;

public class XFsmEditor
{
    private AIFSM? _fsm;
    private bool _isWeaponFsm;
    private nint _ctx;
    private readonly BingTranslator _translator = new();

    private bool _translating = false;
    private float _translationProgress = 0f;

    private readonly List<XFsmNode> _nodes = [];
    private readonly List<XFsmLink> _links = [];

    private readonly Dictionary<int, XFsmConditionTreeInfo> _treeInfoMap = [];

    private bool _showStyleEditor = false;
    private bool _showTranslatedNames = false;
    private bool _inputIntWasActive;

    private XFsmPin? _newNodePin = null;
    private XFsmPin? _contextPin = null;
    private XFsmNode? _contextNode = null;

    private TextureHandle _blueprintHeaderBg;
    private uint _headerBgWidth;
    private uint _headerBgHeight;

    private readonly Random _random = new();
    private float _maxArea = 130f;

    private float _rF = 1000f;
    private float _l = 500f;
    private float _sF = 0.001f;

    private readonly Dictionary<nint, string> _translatedNodeNames = [];
    private readonly Dictionary<nint, string> _translatedLinkNames = [];

    private static readonly string[] OperatorTypeNames = Enum.GetNames<OperatorType>();

    #region Allocators

    private MtAllocator _nodeAllocator = null!;
    private MtAllocator _clusterAllocator = null!;

    #endregion

    #region DTIs

    private readonly List<MtDti> _parameterDtis = [];
    private MtDti? _actionSetDti;
    private MtDti? _linkMotionDti;

    private string _actionContainerName = "";
    private string _motionContainerName = "";

    #endregion

    #region Filters

    private bool _highlightMatchingNodes = false;
    private int _nodeMatchValueInt1 = 0;
    private int _nodeMatchValueInt2 = 0;
    private int _nodeMatchValueInt3 = 0;
    private int _nodeMatchValueInt4 = 0;
    private string _nodeMatchValueString = "";

    private bool _highlightMatchingLinks = false;
    private XFsmNode? _linkMatchSourceNode = null;
    private XFsmNode? _linkMatchTargetNode = null;
    private int _linkMatchValueInt1 = -1;
    private int _linkMatchValueInt2 = -1;

    private bool _visualizeFlow = false;
    private XFsmNode? _flowSourceNode = null;
    private int _flowSourceNodeId = -1;

    #endregion

    public void SetFsm(AIFSM fsm)
    {
        if (_ctx == 0)
        {
            NodeEditor.SetCurrentImGuiContext(ImGui.GetCurrentContext());
            _ctx = NodeEditor.CreateEditor();
            
            NodeEditor.SetCurrentEditor(_ctx);

            ref var style = ref NodeEditor.GetStyle();
            style.LinkStrength = 170f;
            style.NodeRounding = 9f;
            style.NodeBorderWidth = 1f;
            style.HoveredNodeBorderWidth = 3.5f;
            style.HoveredNodeBorderOffset = 2f;
            style.SelectedNodeBorderWidth = 5f;
            style.SelectedNodeBorderOffset = 2f;
            style.NodePadding = new Vector4(12, 12, 12, 12);
            style.HighlightConnectedLinks = 1f;

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

            var paramDti = MtDti.Find("cAICopiableParameter");
            Ensure.NotNull(paramDti);

            foreach (var dti in paramDti.AllChildren)
            {
                _parameterDtis.Add(dti);
            }

            _parameterDtis.Sort((a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
        }

        if (fsm.RootCluster is null)
        {
            ImGuiExtensions.NotificationError("Failed to load FSM into Editor, missing root cluster");
            Log.Error("Failed to load FSM into Editor, missing root cluster");
            return;
        }

        if (fsm.ConditionTree is null)
        {
            ImGuiExtensions.NotificationError("Failed to load FSM into Editor, missing condition tree");
            Log.Error("Failed to load FSM into Editor, missing condition tree");
            return;
        }

        _fsm = fsm;

        _nodes.Clear();
        _links.Clear();
        _treeInfoMap.Clear();

        _isWeaponFsm = fsm.OwnerObjectName.StartsWith("cFSMPl_W");

        if (_isWeaponFsm)
        {
            var wpStr = fsm.OwnerObjectName[7..];
            _actionSetDti = MtDti.Find($"nPlFSM::ActionSet_{wpStr}");
            _linkMotionDti = MtDti.Find($"nPlFSM::LinkMotion_{wpStr}");
            _actionContainerName = $"Action_{wpStr}";
            _motionContainerName = $"LinkMotion_{wpStr}";

            Ensure.NotNull(_actionSetDti);
            Ensure.NotNull(_linkMotionDti);
        }

        // Create nodes
        foreach (var node in fsm.RootCluster.Nodes)
        {
            _nodes.Add(_isWeaponFsm ? new XFsmWeaponNode(node) : new XFsmNode(node));
        }

        // Create links
        foreach (var node in _nodes)
        {
            foreach (var output in node.OutputPins)
            {
                var target = GetNodeById(output.BackingLink.DestinationNodeId, true);
                if (target is null)
                    continue;

                _links.Add(new XFsmLink(output, target.InputPin, output.BackingLink));
            }
        }

        
        foreach (var (treeInfo, i) in fsm.ConditionTree.TreeList.Select((t, i) => (t, i)))
        {
            _treeInfoMap.TryAdd(treeInfo.Name.Id, new XFsmConditionTreeInfo(treeInfo, i));
        }

        ImGuiExtensions.NotificationSuccess($"""
            Successfully loaded FSM into Editor
            Nodes: {_nodes.Count}, Links: {_links.Count}
            """);

        Log.Info($"Successfully loaded FSM into Editor");
        Log.Info($"Nodes: {_nodes.Count}, Links: {_links.Count}");

#if RELEASE
        Task.Run(TranslateNames);
#endif
    }

    private async Task TranslateNames()
    {
        if (_fsm is null)
            return;

        _translatedNodeNames.Clear();
        _translatedLinkNames.Clear();

        _translating = true;

        var english = Language.GetLanguage("en");
        var japanese = Language.GetLanguage("ja");

        var totalRequiredTranslations = (float)(_nodes.Count + _links.Count);
        var translated = 0;

        foreach (var node in _nodes)
        {
            var translation = await Translate(node.Name, japanese, english);
            _translationProgress = translated++ / totalRequiredTranslations;

            if (translation is null)
                continue;

            _translatedNodeNames.TryAdd(node.Id, translation);
        }

        foreach (var pin in _nodes.SelectMany(n => n.OutputPins))
        {
            var translation = await Translate(pin.BackingLink.Name, japanese, english);
            _translationProgress = translated++ / totalRequiredTranslations;

            if (translation is null)
                continue;

            _translatedLinkNames.TryAdd(pin.Id, translation);
        }

        _translating = false;

        ImGuiExtensions.NotificationInfo("Finished translating Names");
    }

    private async Task<string?> Translate(string text, ILanguage from, ILanguage to)
    {
        try
        {
            var translation = await _translator.TranslateAsync(text, to, from);
            Log.Debug($"Translated {text} to {translation.Translation}");
            return translation.Translation;
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            return null;
        }
    }

    public void Render()
    {
        if (_fsm is null)
        {
            ImGui.TextColored(new Vector4(1, .33f, .33f, 1), $"{FA6.Xmark} No FSM loaded");
            return;
        }
        
        NodeEditor.SetCurrentEditor(_ctx);

        if (ImGui.Button("Show Style Editor"))
        {
            _showStyleEditor = true;
        }

        ImGui.SameLine();

        if (ImGui.Button("Randomize Positions"))
        {
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

        ImGui.SetNextItemWidth(availX * 0.15f);
        ImGui.DragFloat("Max Area", ref _maxArea);

        ImGui.SameLine();

        ImGui.Checkbox("Show Translated Names", ref _showTranslatedNames);

        if (_translating)
        {
            ImGui.SameLine();
            ImGui.ProgressBar(_translationProgress, new Vector2(-1, 0), "Translating Names...");
        }

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

        if (_showStyleEditor)
        {
            ShowStyleEditor();
        }

        ImGui.BeginChild("##sidebar", new Vector2(), ImGuiChildFlags.Border | ImGuiChildFlags.ResizeX);
        {
            ShowLeftSidePanel();
        }
        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("##center");

        NodeEditor.Begin("XFSM Editor");

        //var cursorTopLeft = ImGui.GetCursorScreenPos();

        var style = ImGui.GetStyle();
        var drawList = ImGui.GetWindowDrawList();

        if (_visualizeFlow)
        {
            DrawFlowFromNode();
        }
        else
        {
            DrawAllNodes();
        }
        
        if (NodeEditor.BeginCreate(new Vector4(1, 1, 1, 1), 2.0f))
        {
            if (NodeEditor.QueryNewLink(out var startPinId, out var endPinId))
            {
                var startPin = GetPinById(startPinId);
                var endPin = GetPinById(endPinId);

                if (startPin is XFsmInputPin && endPin is XFsmOutputPin)
                {
                    (startPin, endPin) = (endPin, startPin);
                }

                if (startPin is not null && endPin is not null)
                {
                    if (startPin == endPin)
                    {
                        NodeEditor.RejectNewItemEx(new Vector4(1, 0, 0, 1), 2.0f);
                    }
                    else if (startPin.GetType() == endPin.GetType())
                    {
                        ShowLabel($"{FA6.Xmark} Incompatible Pin Kind", new MtColor(45, 32, 32, 180));
                        NodeEditor.RejectNewItemEx(new Vector4(1, 0, 0, 1), 2.0f);
                    }
                    else
                    {
                        ShowLabel($"{FA6.Plus} Create Link", new MtColor(32, 45, 32, 180));
                        if (NodeEditor.AcceptNewItemEx(new Vector4(0.5f, 1, 0.5f, 1), 4.0f))
                        {
                            var source = (XFsmOutputPin)startPin;
                            var target = (XFsmInputPin)endPin;

                            var existingLink = _links.Find(l => l.Source == source);
                            if (existingLink is not null)
                                _links.Remove(existingLink);

                            _links.Add(new XFsmLink(
                                source, target,
                                source.BackingLink
                            ));
                        }
                    }
                }
            }

            if (NodeEditor.QueryNewNode(out var pinId))
            {
                _newNodePin = GetPinById(pinId);
                if (_newNodePin is not null)
                {
                    ShowLabel($"{FA6.Plus} Create Node", new MtColor(32, 45, 32, 180));
                }

                if (NodeEditor.AcceptNewItemEx(new Vector4(0.5f, 1, 0.5f, 1), 4.0f))
                {
                    NodeEditor.Suspend();
                    ImGui.OpenPopup("Create New Node");
                    NodeEditor.Resume();
                }
            }
        }
        NodeEditor.EndCreate();

        if (NodeEditor.BeginDelete())
        {
            if (NodeEditor.QueryDeletedNode(out var nodeId))
            {
                if (NodeEditor.AcceptDeletedItem())
                {
                    var node = GetNodeById(nodeId);
                    if (node is not null)
                    {
                        _fsm!.RootCluster!.RemoveNode(node.BackingNode);
                        _nodes.Remove(node);
                    }
                }
            }

            if (NodeEditor.QueryDeletedLink(out var linkId))
            {
                if (NodeEditor.AcceptDeletedItem())
                {
                    var link = GetLinkById(linkId);
                    if (link is not null)
                    {
                        link.Source.Parent.BackingNode.RemoveLink(link.BackingLink);
                        _links.Remove(link);
                    }
                }
            }
        }
        NodeEditor.EndDelete();

        //ImGui.SetCursorScreenPos(cursorTopLeft);

        var openPopupPosition = ImGui.GetMousePos();
        NodeEditor.Suspend();
        if (NodeEditor.ShowNodeContextMenu(out var contextNodeId))
        {
            ImGui.OpenPopup("Node Context Menu");
            _contextNode = GetNodeById(contextNodeId);
        }
        if (NodeEditor.ShowPinContextMenu(out var contextPinId))
        {
            ImGui.OpenPopup("Pin Context Menu");
            _contextPin = GetPinById(contextPinId);
        }
        if (NodeEditor.ShowBackgroundContextMenu())
        {
            ImGui.OpenPopup("Create New Node");
            _newNodePin = null;
        }
        NodeEditor.Resume();

        NodeEditor.Suspend();
        if (ImGui.BeginPopup("Node Context Menu"))
        {
            if (ImGui.MenuItem("Add Link"))
            {
                var link = _contextNode!.BackingNode.AddLink($"{_contextNode.Name} -> Unknown");
                link.HasCondition = false;
                _contextNode.OutputPins.Add(new XFsmOutputPin(_contextNode, link, _contextNode.OutputPins.Count));

                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
        if (ImGui.BeginPopup("Pin Context Menu"))
        {
            var name = _contextPin!.Name;
            if (ImGui.InputText("Name", ref name, 260))
            {
                _contextPin.Name = name;
            }

            if (_contextPin is XFsmInputPin input)
            {
                // TODO: Add input pin context menu items
            }
            else if (_contextPin is XFsmOutputPin output)
            {
                if (ImGui.MenuItem("Remove Pin"))
                {
                    output.Parent.BackingNode.RemoveLink(output.BackingLink);
                    var link = _links.Find(l => l.Source == output);
                    if (link is not null)
                    {
                        _links.Remove(link);
                    }

                    output.Parent.OutputPins.Remove(output);
                    ImGui.CloseCurrentPopup();
                }

                var linkIndex = -1;
                if (ImGui.MenuItem("Insert Link Before"))
                {
                    linkIndex = output.Parent.OutputPins.IndexOf(output);
                }

                if (ImGui.MenuItem("Insert Link After"))
                {
                    linkIndex = output.Parent.OutputPins.IndexOf(output) + 1;
                }

                if (ImGui.SmallButton(FA6.ArrowUp))
                {
                    // TODO: Move pin up
                }
                ImGui.SameLine();
                if (ImGui.SmallButton(FA6.ArrowDown))
                {
                    // TODO: Move pin down
                }

                if (linkIndex != -1)
                {
                    var node = output.Parent;
                    var link = node.BackingNode.AddLink($"{node.Name}_t{linkIndex}");
                    link.HasCondition = false;

                    if (linkIndex == node.OutputPins.Count)
                    {
                        node.OutputPins.Add(new XFsmOutputPin(node, link, linkIndex));
                    }
                    else
                    {
                        node.OutputPins.Insert(linkIndex, new XFsmOutputPin(node, link, linkIndex));
                    }

                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("Create New Node"))
        {
            if (_isWeaponFsm)
            {
                var create = false;
                var action = false;
                if (ImGui.MenuItem("Action Node"))
                {
                    create = true;
                    action = true;
                }

                if (ImGui.MenuItem("Motion Node"))
                {
                    create = true;
                    action = false;
                }

                if (create)
                {
                    CreateNewWeaponFsmNode(action ? "New Action Node" : "New Motion Node", action, openPopupPosition, _newNodePin);
                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.EndPopup();
        }
        NodeEditor.Resume();

        NodeEditor.End();
        NodeEditor.SetCurrentEditor(0);

        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("##right", new Vector2(), ImGuiChildFlags.ResizeX);
        {
            ShowRightSidePanel();
        }
        ImGui.EndChild();

        ImGui.BeginChild("##bottom", new Vector2(), ImGuiChildFlags.ResizeY);
        {
            ShowConditionTree();
        }
        ImGui.EndChild();

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

    private void DrawAllNodes()
    {
        var builder = new NodeBuilder(_blueprintHeaderBg, _headerBgWidth, _headerBgHeight);

        foreach (var node in _nodes)
        {
            DrawNode(node, builder);
        }

        foreach (var link in _links)
        {
            DrawLink(link);
        }
    }

    private void DrawFlowFromNode()
    {
        if (_flowSourceNode is null)
            return;

        var builder = new NodeBuilder(_blueprintHeaderBg, _headerBgWidth, _headerBgHeight);

        HashSet<XFsmNode> visited = [];
        List<XFsmLink> links = [];

        Traverse(_flowSourceNode);

        foreach (var link in links)
        {
            DrawLink(link);
        }

        return;

        void Traverse(XFsmNode node)
        {
            if (!visited.Add(node))
                return;

            DrawNode(node, builder);

            foreach (var link in _links)
            {
                if (node.OutputPins.Contains(link.Source))
                {
                    links.Add(link);
                    Traverse(link.Target.Parent);
                }
            }
        }
    }

    private void DrawNode(XFsmNode node, NodeBuilder builder)
    {
        var highlight = false;
        if (_highlightMatchingNodes)
        {
            if (_isWeaponFsm)
            {
                var wpNode = (XFsmWeaponNode)node;
                highlight = wpNode.RealId == _nodeMatchValueInt4;
                highlight |= wpNode.NodeType switch
                {
                    WeaponFsmNodeType.Action => wpNode.ActionId == _nodeMatchValueInt1,
                    WeaponFsmNodeType.Motion => wpNode.MotionId == _nodeMatchValueInt2
                                                || wpNode.MotionIdPhase1 == _nodeMatchValueInt3,
                    _ => false
                };

                if (!highlight && !string.IsNullOrEmpty(_nodeMatchValueString))
                {
                    highlight = wpNode.Name.Contains(_nodeMatchValueString, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (highlight)
            {
                NodeEditor.PushStyleColor(StyleColor.NodeBorder, new Vector4(1f, .2f, .2f, 1f));
            }
        }
        builder.Begin(node.Id);
        builder.Header(GetHeaderColorForNode(node));
        {
            ImGui.Spring(0);
            if (_showTranslatedNames && _translatedNodeNames.TryGetValue(node.Id, out var translatedName))
            {
                ImGui.TextUnformatted(translatedName);
            }
            else
            {
                ImGui.TextUnformatted(node.Name);
            }

            ImGui.Spring(1);
            //if (ImGui.Button($"{FA6.Plus} Add Link"))
            //{
            //    var link = node.BackingNode.AddLink($"{node.Name} -> Unknown");
            //    link.HasCondition = false;

            //    node.OutputPins.Add(new XFsmOutputPin(node, link, node.OutputPins.Count));
            //}
            //ImGui.Spring(1);
            ImGui.Dummy(new Vector2(0, 28));
            ImGui.Spring(0);
        }
        builder.EndHeader();

        builder.Input(node.InputPin.Id);
        {
            DrawPinIcon(node.InputPin, _links.Any(l => l.Target == node.InputPin));
            ImGui.Spring(0);
            ImGui.TextUnformatted(node.InputPin.Name);
            ImGui.Spring(0);
        }
        builder.EndInput();

        builder.Middle();
        {
            ImGui.Spring(1, 0);

            if (_isWeaponFsm)
            {
                int val;
                var wpNode = (XFsmWeaponNode)node;

                ImGui.PushItemWidth(100);

                switch (wpNode.NodeType)
                {
                    case WeaponFsmNodeType.Action:
                        val = wpNode.ActionId;
                        if (ImGui.InputInt("Action Id", ref val, 0, 0))
                        {
                            wpNode.ActionId = val;
                        }
                        break;
                    case WeaponFsmNodeType.Motion:
                        val = wpNode.MotionId;
                        if (ImGui.InputInt("Motion Id", ref val, 0, 0))
                        {
                            wpNode.MotionId = val;
                        }

                        val = wpNode.MotionIdPhase1;
                        if (ImGui.InputInt("Motion Id Phase 1", ref val, 0, 0))
                        {
                            wpNode.MotionIdPhase1 = val;
                        }
                        break;
                }

                ImGui.PopItemWidth();

                //if (ImGui.IsItemActive() && !_inputIntWasActive)
                //{
                //    NodeEditor.EnableShortcuts(false);
                //    _inputIntWasActive = true;
                //}
                //else if (!ImGui.IsItemActive() && _inputIntWasActive)
                //{
                //    NodeEditor.EnableShortcuts(true);
                //    _inputIntWasActive = false;
                //}
            }
            else
            {
                // TODO: Show properties for non-weapon FSM nodes
            }

            ImGui.Spring(1, 0);
        }

        foreach (var output in node.OutputPins)
        {
            builder.Output(output.Id);
            {
                ImGui.Spring(0);
                if (_showTranslatedNames && _translatedLinkNames.TryGetValue(output.Id, out var translatedName))
                {
                    ImGui.TextUnformatted(translatedName);
                }
                else
                {
                    ImGui.TextUnformatted(output.Name);
                }
                ImGui.Spring(0, 1);
                DrawPinIcon(output, _links.Any(l => l.Source == output));
            }
            builder.EndOutput();
        }

        builder.End();

        if (highlight)
        {
            NodeEditor.PopStyleColor();
        }
    }

    private void DrawLink(XFsmLink link)
    {
        if (_highlightMatchingLinks)
        {
            var sourceMatch = _linkMatchSourceNode is null || link.Source.Parent == _linkMatchSourceNode;
            var targetMatch = _linkMatchTargetNode is null || link.Target.Parent == _linkMatchTargetNode;

            if (sourceMatch && targetMatch)
            {
                NodeEditor.Link(
                    link.Id,
                    link.Source.Id,
                    link.Target.Id,
                    new Vector4(1f, .2f, .2f, 1f),
                    2f
                );
            }
            else
            {
                NodeEditor.Link(link.Id, link.Source.Id, link.Target.Id, new Vector4(1, 1, 1, 0.2f), 1.5f);
            }
        }
        else
        {
            NodeEditor.Link(link.Id, link.Source.Id, link.Target.Id, new Vector4(1, 1, 1, 1), 1.5f);
        }
    }

    private void ShowLeftSidePanel()
    {
        List<nint> selectedNodes = [];
        NodeEditor.GetSelectedNodes(selectedNodes);

        if (selectedNodes.Count == 1)
        {
            var node = GetNodeById(selectedNodes[0]);
            if (node is not null)
            {
                ImGui.SeparatorText("Selected Node");
                ShowNodeProperties(node);
            }
        }

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Filters"))
        {
            ImGui.PushID("filters");
            if (_isWeaponFsm)
            {
                ImGui.Checkbox("Highlight Matching Nodes", ref _highlightMatchingNodes);

                ImGui.NewLine();

                ImGui.Text("Match by...");
                ImGui.InputInt("Action Id", ref _nodeMatchValueInt1);
                ImGui.InputInt("Motion Id", ref _nodeMatchValueInt2);
                ImGui.InputInt("Motion Id Phase 1", ref _nodeMatchValueInt3);
                ImGui.InputInt("Id", ref _nodeMatchValueInt4);
                ImGui.InputText("Name", ref _nodeMatchValueString, 260);

                ImGui.Separator();

                ImGui.Checkbox("Highlight Matching Links", ref _highlightMatchingLinks);

                ImGui.NewLine();

                ImGui.Text("Match by... (-1 = ignore)");

                if (ImGui.InputInt("Source Node Id##links", ref _linkMatchValueInt1))
                    _linkMatchSourceNode = GetNodeById(_linkMatchValueInt1, true);

                if (ImGui.InputInt("Target Node Id##links", ref _linkMatchValueInt2))
                    _linkMatchTargetNode = GetNodeById(_linkMatchValueInt2, true);

                ImGui.Separator();

                if (ImGui.Checkbox("Visualize Flow from Node", ref _visualizeFlow))
                {
                    if (_visualizeFlow)
                    {
                        _highlightMatchingNodes = false;
                        _highlightMatchingLinks = false;
                    }
                }

                if (ImGui.InputInt("Source Node Id##flow", ref _flowSourceNodeId))
                    _flowSourceNode = GetNodeById(_flowSourceNodeId, true);
            }
            ImGui.PopID();
        }
    }

    private void ShowRightSidePanel()
    {
        Span<nint> selectedLinks = stackalloc nint[1];
        if (NodeEditor.GetSelectedLinkCount() == 1)
        {
            NodeEditor.GetSelectedLinks(selectedLinks, 1);

            var link = GetLinkById(selectedLinks[0]);
            if (link is not null)
            {
                ImGui.SeparatorText("Selected Link");
                ShowLinkProperties(link);
            }
        }
    }

    private void ShowNodeProperties(XFsmNode node)
    {
        ImGui.Text($"Node Id: {node.RealId}");
        var name = node.Name;
        ImGui.InputText("Name", ref name, 260);
        node.Name = name;

        ImGui.Separator();

        if (_isWeaponFsm)
        {
            var wpNode = (XFsmWeaponNode)node;

            if (ImGui.BeginCombo("Node Type", wpNode.NodeType.ToString()))
            {
                foreach (var type in Enum.GetValues<WeaponFsmNodeType>())
                {
                    var isSelected = wpNode.NodeType == type;
                    if (ImGui.Selectable(type.ToString(), isSelected))
                    {
                        wpNode.NodeType = type;
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            var process = wpNode.BackingNode.Processes[0];
            var parameter = process.Parameter;
            var containerName = process.ContainerName;
            ImGui.Text($"Process Container: {containerName}");

            if (parameter is not null)
            {
                int val;
                switch (wpNode.NodeType)
                {
                    case WeaponFsmNodeType.Action:
                        val = wpNode.ActionId;
                        if (ImGui.InputInt("Action Id", ref val))
                        {
                            wpNode.ActionId = val;
                        }

                        break;
                    case WeaponFsmNodeType.Motion:
                        val = wpNode.MotionId;
                        if (ImGui.InputInt("Motion Id", ref val))
                        {
                            wpNode.MotionId = val;
                        }

                        val = wpNode.MotionIdPhase1;
                        if (ImGui.InputInt("Motion Id Phase 1", ref val))
                        {
                            wpNode.MotionIdPhase1 = val;
                        }
                        break;
                }
            }
            else
            {
                ImGui.Text("No parameter found");
                if (ImGui.Button("Add Parameter"))
                {
                    process.Parameter = wpNode.NodeType switch
                    {
                        WeaponFsmNodeType.Action => _actionSetDti!.CreateInstance<MtObject>(),
                        WeaponFsmNodeType.Motion => _linkMotionDti!.CreateInstance<MtObject>(),
                    };
                }
            }
        }
        else
        {
            // TODO: Show properties for non-weapon FSM nodes
        }


    }

    private void ShowLinkProperties(XFsmLink link)
    {
        ImGui.Text($"Source: {link.Source.Parent.Name}");
        ImGui.Text($"Target: {link.Target.Parent.Name}");

        var fsmLink = link.Source.BackingLink;
        if (!fsmLink.HasCondition)
            return;

        ImGui.Text("Has Condition");
        var condition = GetConditionById(fsmLink.ConditionId);
        if (condition is null)
        {
            ImGui.TextColored(new Vector4(1, .3f, .3f, 1), "Condition not found");
            return;
        }

        ImGui.Text($"Condition Id: {condition.Id}");
        ImGui.Text($"Condition Name: {condition.BackingInfo.Name.Name}");

        DisplayConditionNode(condition.RootNode, condition);

        if (ImGui.BeginPopup("Add Child"))
        {
            if (ImGui.MenuItem("ConstEnumNode"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.ConstEnumNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstF32Node"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.ConstF32Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstF64Node"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.ConstF64Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstS32Node"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.ConstS32Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstS64Node"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.ConstS64Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstStringNode"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.ConstStringNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("OperationNode"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.OperationNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("StateNode"))
            {
                condition.RootNode?.AddChild(ConditionTreeNodeType.StateNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("VariableNode"))
            {
                var node = condition.RootNode?.AddChild(ConditionTreeNodeType.VariableNode).As<AIConditionTreeVariableNode>();
                if (node is not null)
                {
                    node.Variable.PropertyName = "";
                    node.Variable.OwnerName = _fsm?.OwnerObjectName ?? "";
                }
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        return;

        
    }

    private void ShowConditionTree()
    {
        foreach (var (id, treeInfo) in _treeInfoMap)
        {
            if (ImGui.TreeNode(id.ToString()))
            {
                DisplayConditionNode(treeInfo.RootNode, treeInfo);
                ImGui.TreePop();
            }
        }
    }

    private void DrawPinIcon(XFsmPin pin, bool connected)
    {
        MtColor color;
        IconType type;
        switch (pin)
        {
            case XFsmInputPin input:
            {
                color = new MtColor(124, 21, 153, 255);
                if (input.Parent is XFsmWeaponNode wpNode)
                {
                    type = wpNode.NodeType == WeaponFsmNodeType.Motion
                        ? IconType.Flow
                        : IconType.Circle;
                }
                else
                {
                    type = IconType.Circle;
                }

                break;
            }
            case XFsmOutputPin output:
            {
                color = new MtColor(147, 226, 74, 255);
                if (output.Parent is XFsmWeaponNode wpNode)
                {
                    type = wpNode.NodeType == WeaponFsmNodeType.Motion
                        ? IconType.Flow
                        : IconType.Circle;
                }
                else
                {
                    type = IconType.Circle;
                }

                break;
            }
            default:
                throw new ArgumentException("Invalid pin type");
        }

        NodeEditor.Icon(new Vector2(24, 24), type, connected, color.ToVector4(), new Vector4(32, 32, 32, 255));
    }

    private void ShowStyleEditor()
    {
        if (ImGui.Begin("Node Editor Style", ref _showStyleEditor))
        {
            ref var style = ref NodeEditor.GetStyle();
            ImGui.DragFloat4("Node Padding", ref style.NodePadding, 0.1f);
            ImGui.DragFloat("Node Rounding", ref style.NodeRounding, 0.1f);
            ImGui.DragFloat("Node Border Width", ref style.NodeBorderWidth, 0.1f);
            ImGui.DragFloat("Hovered Node Border Width", ref style.HoveredNodeBorderWidth, 0.1f);
            ImGui.DragFloat("Hovered Node Border Offset", ref style.HoveredNodeBorderOffset, 0.1f);
            ImGui.DragFloat("Selected Node Border Width", ref style.SelectedNodeBorderWidth, 0.1f);
            ImGui.DragFloat("Selected Node Border Offset", ref style.SelectedNodeBorderOffset, 0.1f);
            ImGui.DragFloat("Pin Rounding", ref style.PinRounding, 0.1f);
            ImGui.DragFloat("Pin Border Width", ref style.PinBorderWidth, 0.1f);
            ImGui.DragFloat("Link Strength", ref style.LinkStrength, 0.1f);
            ImGui.DragFloat2("Source Direction", ref style.SourceDirection, 0.1f);
            ImGui.DragFloat2("Target Direction", ref style.TargetDirection, 0.1f);
            ImGui.DragFloat("Scroll Duration", ref style.ScrollDuration, 0.1f);
            ImGui.DragFloat("Flow Marker Distance", ref style.FlowMarkerDistance, 0.1f);
            ImGui.DragFloat("Flow Speed", ref style.FlowSpeed, 0.1f);
            ImGui.DragFloat("Flow Duration", ref style.FlowDuration, 0.1f);
            ImGui.DragFloat2("Pivot Alignment", ref style.PivotAlignment, 0.1f);
            ImGui.DragFloat2("Pivot Size", ref style.PivotSize, 0.1f);
            ImGui.DragFloat2("Pivot Scale", ref style.PivotScale, 0.1f);
            ImGui.DragFloat("Pin Corners", ref style.PinCorners, 0.1f);
            ImGui.DragFloat("Pin Radius", ref style.PinRadius, 0.1f);
            ImGui.DragFloat("Pin Arrow Size", ref style.PinArrowSize, 0.1f);
            ImGui.DragFloat("Pin Arrow Width", ref style.PinArrowWidth, 0.1f);
            ImGui.DragFloat("Group Rounding", ref style.GroupRounding, 0.1f);
            ImGui.DragFloat("Group Border Width", ref style.GroupBorderWidth, 0.1f);
            ImGui.InputFloat("Highlight Connected Links", ref style.HighlightConnectedLinks);

            for (var i = 0; i < (int)StyleColor.Count; i++)
            {
                ImGui.ColorEdit4(((StyleColor)i).ToString(), ref style.Colors[i]);
            }
        }
        ImGui.End();
    }

    private static void DisplayConditionNode(AIConditionTreeNode? node, object? parent, int index = 0)
    {
        if (node is null)
            return;

        ImGui.PushID(node.Instance);

        var typeChanged = false;
        if (ImGui.BeginCombo($"##conditionNode{node.Instance}", $"{node.Type}##conditionNode{node.Instance}"))
        {
            foreach (var type in Enum.GetValues<ConditionTreeNodeType>())
            {
                var isSelected = node.Type == type;
                if (ImGui.Selectable(type.ToString(), isSelected))
                {
                    if (parent is XFsmConditionTreeInfo condition)
                    {
                        var root = AIConditionTreeNode.Create(type);
                        condition.RootNode?.Destroy(true);
                        condition.RootNode = root;
                        condition.BackingInfo.RootNode = root;
                    }
                    else if (parent is AIConditionTreeNode p)
                    {
                        p?.SetChild(index, type);
                    }

                    typeChanged = true;
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        if (typeChanged)
        {
            ImGui.PopID();

            // The call to SetChild destroyed the current node
            // so we need to return here to avoid using the destroyed node
            return;
        }

        switch (node.Type)
        {
            case ConditionTreeNodeType.ConstEnumNode:
                ImGui.InputInt("Enum Value", ref node.Value<int>(0x8));
                break;
            case ConditionTreeNodeType.ConstF32Node:
                ImGui.DragFloat("F32 Value", ref node.Value<float>());
                break;
            case ConditionTreeNodeType.ConstF64Node:
                ImGui.DragScalar("F64 Value", ImGuiDataType.Double, MemoryUtil.AddressOf(ref node.Value<double>()));
                break;
            case ConditionTreeNodeType.ConstS32Node:
                ImGui.InputInt("S32 Value", ref node.Value<int>());
                break;
            case ConditionTreeNodeType.ConstS64Node:
                ImGuiExtensions.InputScalar("S64 Value", ref node.Value<long>());
                break;
            case ConditionTreeNodeType.ConstStringNode:
            {
                var strNode = node.As<AIConditionTreeConstStringNode>();
                var str = strNode.Value;
                if (ImGui.InputText("String Value", ref str, 1024))
                {
                    strNode.Value = str;
                }
                break;
            }
            case ConditionTreeNodeType.OperationNode:
                ImGui.Combo("Operator", ref node.Value<int>(), OperatorTypeNames, OperatorTypeNames.Length);
                break;
            case ConditionTreeNodeType.StateNode:
                ImGui.InputInt("State Id", ref node.Value<int>());
                break;
            case ConditionTreeNodeType.VariableNode:
            {
                var varNode = node.As<AIConditionTreeVariableNode>();
                ImGui.SeparatorText("Variable");
                DisplayVariableInfo(ref varNode.Variable);
                ImGui.SeparatorText("Index Variable");
                DisplayVariableInfo(ref varNode.IndexVariable);
                ImGui.Separator();
                ImGui.Checkbox("Is BitNo", ref varNode.IsBitNo);
                ImGui.Checkbox("Is Array", ref varNode.IsArray);
                ImGui.Checkbox("Is Dynamic Index", ref varNode.IsDynamicIndex);
                ImGui.InputInt("Index", ref varNode.Index);
                ImGui.Checkbox("Use Index Enum", ref varNode.UseIndexEnum);
                if (varNode.UseIndexEnum)
                {
                    ImGui.SeparatorText("Index Enum");
                    DisplayEnumProp(ref varNode.IndexEnum);
                }
                break;
            }
        }

        if (ImGui.Button("Add Child"))
        {
            ImGui.OpenPopup("Add Child");
        }

        ImGui.SeparatorText("Children");
        ImGui.Indent();

        int i = 0;
        foreach (var child in node.Children)
        {
            ImGui.BeginChild($"##conditionNodeChild{i}", new Vector2(0, 0), ImGuiChildFlags.Border);

            DisplayConditionNode(child, node, i);
            i++;

            ImGui.EndChild();
        }

        ImGui.Unindent();
        ImGui.PopID();
    }

    private static void DisplayVariableInfo(ref AIConditionTreeVariableNode.VariableInfo variable)
    {
        var propertyName = variable.PropertyName;
        var ownerName = variable.OwnerName;

        ImGui.PushID(MemoryUtil.AddressOf(ref variable));

        if (ImGui.InputText("Property Name", ref propertyName, 260))
        {
            variable.PropertyName = propertyName;
        }

        if (ImGui.InputText("Owner Name", ref ownerName, 260))
        {
            variable.OwnerName = ownerName;
        }

        ImGui.Checkbox("Is Singleton Owner", ref variable.IsSingletonOwner);

        ImGui.PopID();
    }

    private static void DisplayEnumProp(ref EnumProp prop)
    {
        ImGui.PushID(MemoryUtil.AddressOf(ref prop));

        var name = prop.Name;
        var enumName = prop.EnumName;

        ImGui.InputInt("Value", ref prop.Value);

        if (ImGui.InputText("Name", ref name, 260))
            prop.Name = name;

        if (ImGui.InputText("Enum Name", ref enumName, 260))
            prop.EnumName = enumName;

        ImGuiExtensions.InputScalar("Name Hash", ref prop.NameCrc, format: "%X");
        ImGui.SameLine();
        if (ImGui.Button("Auto-Fill##nameHash"))
            prop.NameCrc = Utility.Crc32(name);

        ImGuiExtensions.InputScalar("Enum Name Hash", ref prop.EnumNameCrc, format: "%X");
        ImGui.SameLine();
        if (ImGui.Button("Auto-Fill##enumNameHash"))
            prop.EnumNameCrc = Utility.Crc32(enumName);

        ImGui.PopID();
    }

    private XFsmWeaponNode CreateNewWeaponFsmNode(string name, bool action, Vector2 position, XFsmPin? originPin = null) 
    {
        var node = _fsm!.RootCluster!.AddNode(name);
        node.Id = GetNextFreeNodeId();
        var process = node.AddProcess(action ? _actionContainerName : _motionContainerName);

        process.Parameter = action
            ? _actionSetDti!.CreateInstance<MtObject>()
            : _linkMotionDti!.CreateInstance<MtObject>();

        var fsmNode = new XFsmWeaponNode(node)
        {
            Position = position
        };

        NodeEditor.SetNodePosition(fsmNode.Id, fsmNode.Position);
        _nodes.Add(fsmNode);

        switch (originPin)
        {
            case XFsmInputPin input:
            {
                var link = node.AddLink($"{node.Name} -> {input.Parent.Name}");
                link.DestinationNodeId = input.Parent.Id;
                var pin = new XFsmOutputPin(fsmNode, link, 0);
                fsmNode.OutputPins.Add(pin);
                _links.Add(new XFsmLink(pin, input, link));
                break;
            }
            case XFsmOutputPin output:
            {
                var existingLink = _links.Find(l => l.Source == output);
                if (existingLink is not null)
                    _links.Remove(existingLink);

                output.BackingLink.DestinationNodeId = node.Id;
                _links.Add(new XFsmLink(output, fsmNode.InputPin, output.BackingLink));
                break;
            }
        }

        return fsmNode;
    }

    private static bool IsSingleConditionNode(XFsmConditionTreeInfo condition)
    {
        return condition.Type == ConditionTreeNodeType.OperationNode
            && condition.RootNode?.Value<OperatorType>() == OperatorType.None;
    }

    private XFsmNode? GetNodeById(nint id, bool useRealId = false)
    {
        return useRealId
            ? _nodes.Find(n => n.RealId == id)
            : _nodes.Find(n => n.Id == id);
    }

    private XFsmPin? GetPinById(nint id)
    {
        foreach (var node in _nodes)
        {
            if (node.InputPin.Id == id)
                return node.InputPin;

            foreach (var outputPin in node.OutputPins)
            {
                if (outputPin.Id == id)
                    return outputPin;
            }
        }

        return null;
    }

    private XFsmLink? GetLinkById(nint id)
    {
        foreach (var link in _links)
        {
            if (link.Id == id)
                return link;
        }

        return null;
    }

    private XFsmConditionTreeInfo? GetConditionById(int id)
    {
        return _treeInfoMap.GetValueOrDefault(id);
    }

    private int GetNextFreeNodeId()
    {
        return _nodes.Count == 0 ? 0 : _nodes.Max(x => x.Id) + 1;
    }

    private static MtColor GetHeaderColorForNode(XFsmNode node)
    {
        return node switch
        {
            XFsmWeaponNode wpNode => wpNode.NodeType switch
            {
                WeaponFsmNodeType.Action => new MtColor(90, 90, 255, 255),
                WeaponFsmNodeType.Motion => new MtColor(140, 220, 140, 255),
                _ => new MtColor(255, 255, 255, 255)
            },
            _ => new MtColor(255, 255, 255, 255)
        };
    }

    // ID Breakdown:
    // Node Ids:
    // They are 32-bit integers. Bit 29 is set to 1 because 0 is reserved for invalid ids.
    //
    // Link Ids:
    // They are 64-bit integers. The upper 32 bits are the source node id, the lower 32 bits are the target node id.
    // Bits 63 and 31 are set to 1 to distinguish them from node ids.
    // This limits the maximum number of nodes to 2^31 - 1, which is 2,147,483,647.
    //
    // Output Pin Ids:
    // They are 32-bit integers. The upper 16 bits are the link index, the lower 16 bits are the parent node id.
    // Bit 31 is set to 1 to distinguish them from other kinds of ids.
    // This further limits the maximum number of nodes to 2^15 - 1, which is 32,767. This should still be plenty.
    //
    // Input Pin Ids:
    // They are 32-bit integers. Input pin ids only consist of the parent node id,
    // with bit 30 set to 1 to distinguish them from other kinds of ids.
    // Nothing else is needed since there can only be one input pin per node.

    public static int MakeNodeId(AIFSMNode node)
    {
        return (1 << 29) | node.Id;
    }

    public static nint MakeLinkId(XFsmNode source, XFsmNode target)
    {
        // Link id format: 0b1SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS1PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
        // S = Source Id, P = Target Id
        // 31 bits for both source and target id
        // The extra 0x8000000080000000 is to make the link id unique from node ids
        // Since without this, if you had a link from node 0 to node 1, the link id would be 1
        // and if you had a node with id 1, it would conflict with the link id
        return unchecked((nint)0x8000000080000000) | (nint)((long)source.Id << 32 | (uint)target.Id);
    }

    public static int MakeOutputPinId(AIFSMNode parent, int index)
    {
        // Output pin id format: 0b1NNNNNNNNNNNNNNNPPPPPPPPPPPPPPPP
        // N = Link Index, P = Parent Id
        // 15 bits for link index, 16 bits for parent id
        // Due to this, the maximum number of links per node is 32767
        return (1 << 31) | (index << 16) | parent.Id;
    }

    public static int MakeInputPinId(AIFSMNode parent)
    {
        return (1 << 30) | parent.Id;
    }
}

public class XFsmNode
{
    public int Id { get; }
    public int RealId => BackingNode.Id;
    public string Name { get; set; }
    public Vector2 Position { get; set; }

    public XFsmInputPin InputPin { get; }
    public List<XFsmOutputPin> OutputPins { get; }

    public AIFSMNode BackingNode { get; }


    public XFsmNode(AIFSMNode node)
    {
        Id = XFsmEditor.MakeNodeId(node);
        BackingNode = node;
        Name = node.Name;
        InputPin = new XFsmInputPin(this);
        OutputPins = [];

        for (var i = 0; i < node.LinkCount; i++)
        {
            OutputPins.Add(new XFsmOutputPin(this, node.Links[i], i));
        }
    }
}

public class XFsmWeaponNode : XFsmNode
{
    private WeaponFsmNodeType _nodeType;
    private int _actionId;
    private int _motionId;
    private int _motionIdPhase1;

    public WeaponFsmNodeType NodeType
    {
        get => _nodeType;
        set
        {
            if (value == _nodeType)
                return;

            _nodeType = value;

            var process = BackingNode.Processes[0];
            var oldName = process.ContainerName;

            switch (value)
            {
                case WeaponFsmNodeType.Action:
                    process.ContainerName = oldName.Replace("LinkMotion_W", "Action_W");
                    process.Parameter?.Destroy(true);
                    process.Parameter = MtDti.Find($"nPlFSM::ActionSet_W{oldName[12..]}")?.CreateInstance<MtObject>();
                    break;
                case WeaponFsmNodeType.Motion:
                    process.ContainerName = oldName.Replace("Action_W", "LinkMotion_W");
                    process.Parameter?.Destroy(true);
                    process.Parameter = MtDti.Find($"nPlFSM::LinkMotion_W{oldName[8..]}")?.CreateInstance<MtObject>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    public int ActionId
    {
        get => _actionId;
        set
        {
            if (value == _actionId || _nodeType != WeaponFsmNodeType.Action)
                return;

            _actionId = value;
            var param = BackingNode.Processes[0].Parameter;
            if (param is not null)
            {
                param.GetRef<int>(0x10) = value;
            }
        }
    }

    public int MotionId
    {
        get => _motionId;
        set
        {
            if (value == _motionId || _nodeType != WeaponFsmNodeType.Motion)
                return;

            _motionId = value;
            var param = BackingNode.Processes[0].Parameter;
            if (param is not null)
            {
                param.GetRef<int>(0x8) = value;
            }
        }
    }

    public int MotionIdPhase1
    {
        get => _motionIdPhase1;
        set
        {
            if (value == _motionIdPhase1 || _nodeType != WeaponFsmNodeType.Motion)
                return;

            _motionIdPhase1 = value;
            var param = BackingNode.Processes[0].Parameter;
            if (param is not null)
            {
                param.GetRef<int>(0xC) = value;
            }
        }
    }

    public XFsmWeaponNode(AIFSMNode node) : base(node)
    {
        if (node.Processes.Count == 0)
        {
            throw new ArgumentException("Weapon FSM node must have at least one process");
        }

        _nodeType = node.Processes[0].ContainerName switch
        {
            { } name when name.StartsWith("Action_W") => WeaponFsmNodeType.Action,
            { } name when name.StartsWith("LinkMotion_W") => WeaponFsmNodeType.Motion,
            _ => throw new ArgumentException($"Invalid process container name {node.Processes[0].ContainerName}")
        };

        var param = node.Processes[0].Parameter;
        if (param is not null)
        {
            switch (_nodeType)
            {
                case WeaponFsmNodeType.Action:
                    _actionId = param.Get<int>(0x10);
                    break;
                case WeaponFsmNodeType.Motion:
                    _motionId = param.Get<int>(0x8);
                    _motionIdPhase1 = param.Get<int>(0xC);
                    break;
            }
        }
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

public class XFsmOutputPin(XFsmNode parent, AIFSMLink link, int index) 
    : XFsmPin(link.Name, XFsmEditor.MakeOutputPinId(parent.BackingNode, index))
{
    public XFsmNode Parent { get; } = parent;
    public AIFSMLink BackingLink { get; } = link;
}

public class XFsmLink(XFsmOutputPin source, XFsmInputPin target, AIFSMLink link)
{
    public nint Id { get; } = XFsmEditor.MakeLinkId(source.Parent, target.Parent);
    public string Name { get; set; } = link.Name;
    public XFsmOutputPin Source { get; } = source;
    public XFsmInputPin Target { get; } = target;
    public AIFSMLink BackingLink { get; } = link;
}

public class XFsmConditionTreeInfo(AIConditionTreeInfo info, int treeIndex)
{
    public int Id => BackingInfo.Name.Id;
    public AIConditionTreeInfo BackingInfo { get; } = info;
    public AIConditionTreeNode? RootNode { get; set; } = info.RootNode;
    public ConditionTreeNodeType Type { get; } = info.RootNode?.Type ?? ConditionTreeNodeType.None;
    public int TreeIndex { get; } = treeIndex;
}

public enum WeaponFsmNodeType
{
    Action,
    Motion,
}
