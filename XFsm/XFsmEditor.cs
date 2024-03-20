using System.Collections.Immutable;
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
using System.Runtime.InteropServices.Marshalling;
using SharpPluginLoader.Core.Entities;
using System.Drawing;
using System.Diagnostics;

namespace XFsm;

using NodeEditor = InternalCalls;
using FA6 = FontAwesome6;
using static SharpPluginLoader.Core.Components.CollisionComponent;

public class XFsmEditor
{
    private bool _isWeaponFsm;
    private WeaponType _weaponType = WeaponType.None;
    private nint _ctx;
    private nint _gvCtx;
    private readonly BingTranslator _translator = new();

    private bool _translating = false;
    private float _translationProgress = 0f;

    private readonly List<XFsmNode> _nodes = [];
    private readonly List<XFsmLink> _links = [];

    private readonly Dictionary<int, XFsmConditionTreeInfo> _treeInfoMap = [];

    private bool _showStyleEditor = false;
    private bool _showTranslatedNames = false;
    private bool _inputIntWasActive;

    private string _engineText = "dot";

    private XFsmPin? _newNodePin = null;
    private XFsmPin? _contextPin = null;
    private XFsmNode? _contextNode = null;

    private AIConditionTreeNode? _addChildNode;
    private uint _addChildPopupId = 0;

    private TextureHandle _blueprintHeaderBg;
    private uint _headerBgWidth;
    private uint _headerBgHeight;

    private readonly Random _random = new();
    private float _maxArea = 130f;

    private const float GraphvizSizeDivider = 60f;

    private Vector4 _actioNodeColor = new MtColor(255, 224, 4, 255).ToVector4();
    private Vector4 _motionNodeColor = new MtColor(140, 87, 195, 255).ToVector4();

    private bool _firstRender = true;

    private readonly Dictionary<nint, string> _translatedNodeNames = [];
    private readonly Dictionary<nint, string> _translatedLinkNames = [];

    private readonly List<(string Name, PropType Type)> _questFsmVariables = [];
    private readonly Dictionary<string, XFsmProcessDescriptor> _questFsmProcesses = [];
    private ImmutableDictionary<string, XFsmProcessDescriptor> _baseFsmProcessDescriptors = null!;

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

    ~XFsmEditor()
    {
        if (_ctx != 0)
        {
            NodeEditor.DestroyEditor(_ctx);
            _ctx = 0;
        }

        if (_gvCtx != 0)
        {
            NodeEditor.GvFreeContext(_gvCtx);
            _gvCtx = 0;
        }
    }

    public unsafe void SetFsm(AIFSM fsm)
    {
        if (_ctx == 0)
        {
            _gvCtx = NodeEditor.GvContext();
            _ctx = NodeEditor.CreateEditor();
            
            NodeEditor.SetCurrentEditor(_ctx);

            ref var style = ref NodeEditor.GetStyle();
            style.LinkStrength = 170f;
            style.NodeRounding = 9f;
            style.NodeBorderWidth = 0f;
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

            var baseFsmDti = MtDti.Find("cFSMQuest");
            Ensure.NotNull(baseFsmDti);

            var baseFsm = baseFsmDti.CreateInstance<MtObject>();
            if (baseFsm.Instance == 0)
            {
                ImGuiExtensions.NotificationError("Failed to create instance of cFSMQuest");
                Log.Error("Failed to create instance of cFSMQuest");
                return;
            }

            baseFsmDti = null;
            paramDti = null;

            var dict = new Dictionary<string, XFsmProcessDescriptor>();
            var functions = FsmFunction.CreateArray(
                new NativeFunction<nint, nint>(baseFsm.GetVirtualFunction(5)).Invoke(baseFsm.Instance)
            );

            for (var i = 0; i < functions.Length; i++)
            {
                ref var function = ref functions[i];
                Log.Info($"{i}: {function.Name}");
                dict.Add(function.Name, new XFsmProcessDescriptor(function.Name, function.ParamDti));
            }

            _baseFsmProcessDescriptors = dict.ToImmutableDictionary();

            Log.Debug("Setting Callbacks");
            NodeEditor.SetCallbacks();
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

        Fsm = fsm;

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

            if (int.TryParse(wpStr[1..], out var wpId))
            {
                _weaponType = (WeaponType)wpId;
            }
            else
            {
                _weaponType = WeaponType.None;
                Log.Warn($"Failed to parse weapon type from {wpStr}");
                ImGuiExtensions.NotificationWarning($"Failed to parse weapon type from {wpStr}");
            }

            Ensure.NotNull(_actionSetDti);
            Ensure.NotNull(_linkMotionDti);
        }
        else
        {
            var tempObjectDti = MtDti.Find(fsm.OwnerObjectName);
            if (tempObjectDti is null)
            {
                ImGuiExtensions.NotificationError($"Invalid Owner Object: {fsm.OwnerObjectName}");
                Log.Error($"Invalid Owner Object: {fsm.OwnerObjectName}");
                return;
            }

            var tempObject = tempObjectDti.CreateInstance<MtObject>();
            if (tempObject.Instance == 0)
            {
                ImGuiExtensions.NotificationError($"Failed to create instance of {fsm.OwnerObjectName}");
                Log.Error($"Failed to create instance of {fsm.OwnerObjectName}");
                return;
            }

            tempObjectDti = null;

            _questFsmVariables.Clear();

            var properties = tempObject.GetProperties();
            foreach (var prop in properties)
            {
                _questFsmVariables.Add((prop.HashName, prop.Type));
            }

            _questFsmProcesses.Clear();

            var functions = FsmFunction.CreateArray(
                new NativeFunction<nint, nint>(tempObject.GetVirtualFunction(5)).Invoke(tempObject.Instance)
            );

            for (var i = 0; i < functions.Length; i++)
            {
                ref var function = ref functions[i];
                _questFsmProcesses.TryAdd(function.Name, new XFsmProcessDescriptor(function.Name, function.ParamDti));
            }

            foreach (var (name, descriptor) in _baseFsmProcessDescriptors)
            {
                _questFsmProcesses.TryAdd(name, descriptor);
            }
            
            tempObject.Destroy(true);
        }

        // Create nodes
        foreach (var node in fsm.RootCluster.Nodes)
        {
            _nodes.Add(_isWeaponFsm ? new XFsmWeaponNode(node) : new XFsmQuestNode(node, _questFsmProcesses));
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

    public void ApplyEditorToObject()
    {
        if (Fsm?.RootCluster is null)
            return;

        foreach (var node in _nodes)
        {
            foreach (var pin in node.OutputPins)
            {
                var link = GetLinkFrom(pin);
                pin.BackingLink.DestinationNodeId = link?.Target.Parent.BackingNode.Id ?? 0;
            }
        }
    }

    public bool HasFsm => Fsm is not null;

    public AIFSM? Fsm { get; private set; }

    private async Task TranslateNames()
    {
        if (Fsm is null)
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
        if (_firstRender)
        {
            NodeEditor.SetCurrentImGuiContext(ImGui.GetCurrentContext());
            _firstRender = false;
        }

        if (Fsm is null)
        {
            ImGui.TextColored(new Vector4(1, .33f, .33f, 1), $"{FA6.Xmark} No FSM loaded");
            return;
        }

        _addChildPopupId = ImGui.GetID("Add Child##popup");

        NodeEditor.SetCurrentEditor(_ctx);

#if DEBUG
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
#endif

        if (ImGui.Button("Navigate to Content"))
        {
            NodeEditor.NavigateToContent();
        }

#if DEBUG
        ImGui.SameLine();

        var availX = ImGui.GetContentRegionAvail().X;

        ImGui.SetNextItemWidth(availX * 0.15f);
        ImGui.DragFloat("Max Area", ref _maxArea);
#endif

        ImGui.SameLine();

        ImGui.Checkbox("Show Translated Names", ref _showTranslatedNames);

        if (_translating)
        {
            ImGui.SameLine();
            ImGui.ProgressBar(_translationProgress, new Vector2(-1, 0), "Translating Names...");
        }

        ImGui.Separator();

        ImGui.IsItemToggledSelection();

        if (ImGui.Button("Apply Layout"))
        {
            Span<XFsmGvcNode> gvcNodes = stackalloc XFsmGvcNode[_nodes.Count];
            for (var i = 0; i < _nodes.Count; i++)
            {
                gvcNodes[i] = new XFsmGvcNode(_nodes[i], GraphvizSizeDivider);
            }

            Span<XFsmGvcLink> gvcLinks = stackalloc XFsmGvcLink[_links.Count];
            for (var i = 0; i < _links.Count; i++)
            {
                gvcLinks[i] = new XFsmGvcLink(_links[i].Source.Parent.Id, _links[i].Target.Parent.Id);
            }

            NodeEditor.GvLayout(_gvCtx, gvcNodes, gvcNodes.Length, gvcLinks, gvcLinks.Length, _engineText);

            for (var i = 0; i < _nodes.Count; i++)
            {
                _nodes[i].Position = gvcNodes[i].Position;
                NodeEditor.SetNodePosition(_nodes[i].Id, _nodes[i].Position);
            }
        }

        if (_showStyleEditor)
        {
            ShowStyleEditor();
        }

        ImGui.Begin("Nodes##left");
        {
            ShowLeftSidePanel();
        }
        ImGui.End();

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

                            source.BackingLink.DestinationNodeId = target.Parent.BackingNode.Id;
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
                        Fsm!.RootCluster!.RemoveNode(node.BackingNode);
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

                var pinIndex = output.Parent.OutputPins.IndexOf(output);
                var linkIndex = -1;
                if (ImGui.MenuItem("Insert Link Before"))
                {
                    linkIndex = pinIndex;
                }

                if (ImGui.MenuItem("Insert Link After"))
                {
                    linkIndex = pinIndex + 1;
                }

                var colorsPushed = false;
                if (pinIndex == 0)
                {
                    // Pin can't be moved up, so disable the button
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    colorsPushed = true;
                }

                var framePadding = style.FramePadding.Y;
                style.FramePadding.Y = 6f;

                if (ImGui.ArrowButton("##link-up", ImGuiDir.Up) && !colorsPushed)
                {
                    output.Parent.OutputPins.Reverse(pinIndex - 1, 2);
                    output.Parent.BackingNode.Links.Swap(pinIndex - 1, pinIndex);
                }

                if (colorsPushed)
                {
                    ImGui.PopStyleColor(3);
                    colorsPushed = false;
                }

                if (pinIndex >= output.Parent.OutputPins.Count - 1)
                {
                    // Pin can't be moved down, so disable the button
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    colorsPushed = true;
                }

                ImGui.SameLine();
                if (ImGui.ArrowButton("##link-down", ImGuiDir.Down) && !colorsPushed)
                {
                    output.Parent.OutputPins.Reverse(pinIndex, 2);
                    output.Parent.BackingNode.Links.Swap(pinIndex, pinIndex + 1);
                }

                if (colorsPushed)
                    ImGui.PopStyleColor(3);

                style.FramePadding.Y = framePadding;

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
                        // Need to pass node.OutputPins.Count as the index otherwise there will be
                        // duplicate pins with the same ID
                        node.OutputPins.Insert(linkIndex, new XFsmOutputPin(node, link, node.OutputPins.Count));
                        node.BackingNode.Links.Swap(linkIndex, node.BackingNode.Links.Count - 1);
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

        ImGui.Begin("Links##right");
        {
            ShowRightSidePanel();
        }
        ImGui.End();

        ImGui.Begin("ConditionTree##bottom");
        {
            ShowConditionTree();
        }
        ImGui.End();

        ImGui.Begin("Conditions");
        {
            ShowConditionList();
        }
        ImGui.End();

        // Popups
        if (ImGui.BeginPopup("Add Child##popup"))
        {
            Ensure.NotNull(_addChildNode);

            if (ImGui.MenuItem("ConstEnumNode"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.ConstEnumNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstF32Node"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.ConstF32Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstF64Node"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.ConstF64Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstS32Node"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.ConstS32Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstS64Node"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.ConstS64Node);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("ConstStringNode"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.ConstStringNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("OperationNode"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.OperationNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("StateNode"))
            {
                _addChildNode.AddChild(ConditionTreeNodeType.StateNode);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.MenuItem("VariableNode"))
            {
                var child = _addChildNode.AddChild(ConditionTreeNodeType.VariableNode).As<AIConditionTreeVariableNode>();
                if (child is not null)
                {
                    child.Variable.PropertyName = "";
                    child.Variable.OwnerName = Fsm?.OwnerObjectName ?? "";
                }
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

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
                NodeEditor.PushStyleVarFloat(StyleVar.NodeBorderWidth, 1f);
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
            ImGui.Dummy(new Vector2(0, 28));
            ImGui.Spring(0);
        }
        builder.EndHeader();

        builder.Input(node.InputPin.Id);
        {
            //DrawPinIcon(node.InputPin, _links.Any(l => l.Target == node.InputPin));
            var isConnected = _links.Any(l => l.Target == node.InputPin);
            ImGui.PushStyleColor(ImGuiCol.Text, isConnected
                ? new MtColor(242, 121, 227, 255)
                : new MtColor(244, 181, 236, 254));
            ImGui.TextUnformatted(isConnected ? FA6.ArrowRightFromBracket : FA6.ArrowRight);
            ImGui.PopStyleColor();
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
                ImGui.Spring(0);
                
                var isConnected = _links.Any(l => l.Source == output);
                ImGui.PushStyleColor(ImGuiCol.Text, isConnected
                    ? new MtColor(121, 176, 250, 255)
                    : new MtColor(181, 207, 244, 254));
                ImGui.TextUnformatted(isConnected ? FA6.ArrowRightToBracket : FA6.ArrowRight);
                ImGui.PopStyleColor();
                //DrawPinIcon(output, _links.Any(l => l.Source == output));
            }
            builder.EndOutput();
        }

        builder.End();

        node.Size = NodeEditor.GetNodeSize(node.Id);
        node.Position = NodeEditor.GetNodePosition(node.Id);

        if (highlight)
        {
            NodeEditor.PopStyleColor();
            NodeEditor.PopStyleVar();
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
            using var _ = new ScopedId("filters");
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
        }
    }

    private void ShowRightSidePanel()
    {
        if (NodeEditor.GetSelectedNodeCount() == 1)
        {
            Span<nint> selectedNodes = stackalloc nint[1];
            NodeEditor.GetSelectedNodes(selectedNodes, selectedNodes.Length);

            var node = GetNodeById(selectedNodes[0]);
            if (node is not null)
            {
                ImGui.SeparatorText($"Links for Node {node.Name} : {node.RealId}");
                foreach (var link in _links)
                {
                    if (link.Source.Parent == node)
                    {
                        if (ImGui.CollapsingHeader(link.Name))
                        {
                            ShowLinkProperties(link);
                        }
                    }
                }
            }
        }
        else
        {
            Span<nint> selectedLinks = stackalloc nint[NodeEditor.GetSelectedLinkCount()];
            NodeEditor.GetSelectedLinks(selectedLinks, selectedLinks.Length);

            ImGui.SeparatorText(selectedLinks.Length > 1 ? "Selected Links" : "Selected Link");

            foreach (var linkId in selectedLinks)
            {
                var link = GetLinkById(linkId);
                if (link is not null)
                {
                    if (ImGui.CollapsingHeader(link.Name))
                    {
                        ShowLinkProperties(link);
                    }
                }
            }
        }
    }

    private int _processInsertIndex = -1;
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
            if (process is null)
            {
                ImGui.Text("No process found");
                if (ImGui.Button("Add Process"))
                {
                    wpNode.BackingNode.AddProcess("New Process");
                }

                return;
            }

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
            var indicesToSwap = (-1, -1);
            XFsmNodeProcess? processToRemove = null;
            var qnode = (XFsmQuestNode)node;

            if (ImGui.Button("Add Process"))
            {
                _processInsertIndex = -1;
                ImGui.OpenPopup("New Process");
            }
            var newProcessPopupId = ImGui.GetID("New Process");

            foreach (var process in qnode.Processes)
            {
                ImGui.PushID(process.BackingProcess.Instance);

                var procIndex = qnode.Processes.IndexOf(process);

                if (procIndex == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                }

                if (ImGui.ArrowButton("##up", ImGuiDir.Up) && procIndex > 0)
                {
                    var index = procIndex;
                    indicesToSwap = (index - 1, index);
                }

                if (procIndex == 0)
                {
                    ImGui.PopStyleColor(3);
                }

                ImGui.SameLine();

                if (procIndex == qnode.Processes.Count - 1)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                }

                if (ImGui.ArrowButton("##down", ImGuiDir.Down))
                {
                    var index = procIndex;
                    indicesToSwap = (index, index + 1);
                }

                if (procIndex == qnode.Processes.Count - 1)
                {
                    ImGui.PopStyleColor(3);
                }

                ImGui.SameLine();

                if (ImGui.Button(FA6.Xmark))
                {
                    processToRemove = process;
                }

                ImGui.SameLine();

                var open = ImGui.CollapsingHeader(process.Descriptor.Name);

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("Process Context Menu");
                }
                
                if (ImGui.BeginPopup("Process Context Menu"))
                {
                    if (ImGui.MenuItem("Shift Up") && procIndex > 0)
                    {
                        var index = procIndex;
                        indicesToSwap = (index - 1, index);
                    }
                    if (ImGui.MenuItem("Shift Down") && procIndex < qnode.Processes.Count - 1)
                    {
                        var index = procIndex;
                        indicesToSwap = (index, index + 1);
                    }
                    if (ImGui.MenuItem("Remove"))
                    {
                        processToRemove = process;
                    }
                    if (ImGui.MenuItem("Insert Process Before"))
                    {
                        _processInsertIndex = procIndex;
                        ImGui.OpenPopup(newProcessPopupId);
                    }
                    if (ImGui.MenuItem("Insert Process After"))
                    {
                        _processInsertIndex = procIndex + 1;
                        if (_processInsertIndex >= qnode.Processes.Count)
                            _processInsertIndex = -1; // Append
                        ImGui.OpenPopup(newProcessPopupId);
                    }

                    ImGui.EndPopup();
                }

                if (open)
                {
                    if (ImGui.BeginCombo("Type", process.Descriptor.Name))
                    {
                        foreach (var (procName, descriptor) in _questFsmProcesses)
                        {
                            var isSelected = procName == process.Descriptor.Name;
                            if (ImGui.Selectable(descriptor.Name, isSelected))
                            {
                                process.ChangeKind(descriptor);
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    name = process.BackingProcess.ContainerName;
                    if (ImGui.InputText("Container Name", ref name, 260))
                    {
                        process.BackingProcess.ContainerName = name;
                    }

                    name = process.BackingProcess.CategoryName;
                    if (ImGui.InputText("Category Name", ref name, 260))
                    {
                        process.BackingProcess.CategoryName = name;
                    }

                    if (process.Descriptor.ParamDti is not null)
                    {
                        if (process.Parameter is null)
                        {
                            ImGui.Text("No parameter found");
                            if (ImGui.Button("Add Parameter"))
                            {
                                process.Parameter = process.Descriptor.ParamDti.CreateInstance<MtObject>();
                            }
                        }

                        if (process.Parameter is not null)
                        {
                            NodeEditor.DisplayMtObject(process.Parameter.Instance);
                        }
                    }
                }

                ImGui.PopID();
            }

            if (indicesToSwap != (-1, -1))
            {
                qnode.Processes.Reverse(indicesToSwap.Item1, 2);
                qnode.BackingNode.Processes.Swap(indicesToSwap.Item1, indicesToSwap.Item2);
            }

            if (processToRemove is not null)
            {
                qnode.RemoveProcess(processToRemove);
            }

            if (qnode.Processes.Count == 0)
            {
                ImGui.Text("No processes found");
            }

            if (ImGui.BeginPopup("New Process"))
            {
                var shouldClose = false;

                if (ImGui.BeginCombo("Type", "Select a type..."))
                {
                    foreach (var (procName, descriptor) in _questFsmProcesses.OrderBy(kv => kv.Key))
                    {
                        if (ImGui.Selectable(procName))
                        {
                            if (_processInsertIndex != -1)
                            {
                                qnode.AddProcess(descriptor, _processInsertIndex);
                                _processInsertIndex = -1;
                            }
                            else
                            {
                                qnode.AddProcess(descriptor);
                            }

                            shouldClose = true;
                        }
                    }

                    ImGui.EndCombo();
                }

                if (shouldClose)
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }

    private void ShowLinkProperties(XFsmLink link)
    {
        ImGui.Text($"Source: {link.Source.Parent.Name}");
        ImGui.Text($"Target: {link.Target.Parent.Name}");

        var fsmLink = link.Source.BackingLink;
        if (!fsmLink.HasCondition)
        {
            ImGui.Text("No Condition");
            if (ImGui.Button("Add Condition"))
            {
                fsmLink.HasCondition = true;
                fsmLink.ConditionId = 0;
            }

            return;
        }

        if (ImGui.Button("Remove Condition"))
        {
            fsmLink.HasCondition = false;
            fsmLink.ConditionId = 0;
            return;
        }

        ImGui.InputInt("Condition Id", ref fsmLink.ConditionId);

        var condition = GetConditionById(fsmLink.ConditionId);
        if (condition is null)
        {
            ImGui.TextColored(new Vector4(1, .3f, .3f, 1), "Condition not found");
            return;
        }

        var conditionName = condition.BackingInfo.Name.Name;
        if (ImGui.InputText("Condition Name", ref conditionName, 1024))
        {
            condition.BackingInfo.Name.Name = conditionName;
        }

        DisplayConditionNode(condition.RootNode, condition);
    }

    private void ShowConditionTree()
    {
        if (ImGui.Button("Add New Condition"))
        {
            var treeInfo = Fsm!.ConditionTree?.AddTreeInfo("New TreeInfo");
            if (treeInfo is not null)
            {
                treeInfo.Name.Id = GetNextFreeTreeInfoId();
                treeInfo.RootNode = AIConditionTreeNode.Create(ConditionTreeNodeType.OperationNode);
                _treeInfoMap.Add(treeInfo.Name.Id, new XFsmConditionTreeInfo(treeInfo, _treeInfoMap.Count));
            }
        }

        foreach (var (id, treeInfo) in _treeInfoMap)
        {
            var name = treeInfo.BackingInfo.Name.HasName
                ? $"{id}: {treeInfo.BackingInfo.Name.Name}"
                : $"{id}";
            if (ImGui.TreeNode(name))
            {
                name = treeInfo.BackingInfo.Name.Name;
                if (ImGui.InputText("Name", ref name, 0x50))
                {
                    treeInfo.BackingInfo.Name.Name = name;
                }

                DisplayConditionNode(treeInfo.RootNode, treeInfo);
                ImGui.TreePop();
            }
        }
    }

    private string _conditionFilter = "";
    private bool _filterMatchTranslated = false;
    private bool _conditionShowTranslated = false;
    private void ShowConditionList()
    {
        if (_isWeaponFsm)
        {
            ImGui.InputText("Filter", ref _conditionFilter, 1024);
            ImGui.Checkbox("Match Translated", ref _filterMatchTranslated);
            ImGui.SameLine();
            ImGui.Checkbox("Show Translated", ref _conditionShowTranslated);

            var ignoreFilter = string.IsNullOrEmpty(_conditionFilter);

            if (ImGui.TreeNode("Generic"))
            {
                foreach (var (original, translated) in ConditionList.Generic)
                {
                    if (ignoreFilter || original.Contains(_conditionFilter)
                                     || (_filterMatchTranslated && translated.Contains(_conditionFilter)))
                    {
                        if (ImGui.Selectable(_conditionShowTranslated ? $"{translated} ({original})" : original))
                        {
                            ImGui.SetClipboardText(original);
                            ImGuiExtensions.NotificationInfo("Copied to clipboard");
                        }
                    }
                }

                ImGui.TreePop();
            }

            if (_weaponType != WeaponType.None)
            {
                if (ImGui.TreeNode(_weaponType.ToString()))
                {
                    foreach (var (original, translated) in ConditionList.WeaponSpecific[_weaponType])
                    {
                        if (ignoreFilter || original.Contains(_conditionFilter)
                                         || (_filterMatchTranslated && translated.Contains(_conditionFilter)))
                        {
                            if (ImGui.Selectable(_conditionShowTranslated ? $"{translated} ({original})" : original))
                            {
                                ImGui.SetClipboardText(original);
                                ImGuiExtensions.NotificationInfo("Copied to clipboard");
                            }
                        }
                    }
                    
                    ImGui.TreePop();
                }
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

            ImGui.ColorEdit4("Action Node Color", ref _actioNodeColor);
            ImGui.ColorEdit4("Motion Node Color", ref _motionNodeColor);

            for (var i = 0; i < (int)StyleColor.Count; i++)
            {
                ImGui.ColorEdit4(((StyleColor)i).ToString(), ref style.Colors[i]);
            }
        }
        ImGui.End();
    }

    private void DisplayConditionNode(AIConditionTreeNode? node, object? parent, int index = 0)
    {
        if (node is null)
            return;

        using var _ = new ScopedId(node.Instance);

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
                        p.SetChild(index, type);
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
                if (ImGui.BeginCombo("Operator", node.Value<OperatorType>().ToString()))
                {
                    foreach (var op in Enum.GetValues<OperatorType>())
                    {
                        var isSelected = node.Value<int>() == (int)op;
                        if (ImGui.Selectable($"{(int)op}: {op}", isSelected))
                        {
                            node.Value<int>() = (int)op;
                        }

                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }

                    ImGui.EndCombo();
                }
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
            _addChildNode = node;
            ImGui.OpenPopup(_addChildPopupId);
        }

        if (node.ChildCount == 0)
            return;

        ImGui.SeparatorText("Children");
        ImGui.Indent();

        var drawList = ImGui.GetWindowDrawList();

        var i = 0;
        foreach (var child in node.Children)
        {
            ImGui.BeginGroup();

            DisplayConditionNode(child, node, i);
            i++;

            ImGui.EndGroup();

            var rectMin = ImGui.GetItemRectMin();
            var rectMax = ImGui.GetItemRectMax();
            var paddedMin = rectMin - new Vector2(4, 4);
            var paddedMax = rectMax + new Vector2(4, 4);
            drawList.AddRect(paddedMin, paddedMax, new MtColor(77, 77, 77, 200));
        }

        ImGui.Unindent();
    }

    private static void DisplayVariableInfo(ref AIConditionTreeVariableNode.VariableInfo variable)
    {
        var propertyName = variable.PropertyName;
        var ownerName = variable.OwnerName;

        using var _ = new ScopedId(MemoryUtil.AddressOf(ref variable));

        if (ImGui.InputText("Property Name", ref propertyName, 260))
        {
            variable.PropertyName = propertyName;
        }

        if (ImGui.InputText("Owner Name", ref ownerName, 260))
        {
            variable.OwnerName = ownerName;
        }

        ImGui.Checkbox("Is Singleton Owner", ref variable.IsSingletonOwner);
    }

    private static void DisplayEnumProp(ref EnumProp prop)
    {
        using var _ = new ScopedId(MemoryUtil.AddressOf(ref prop));

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
    }

    private XFsmWeaponNode CreateNewWeaponFsmNode(string name, bool action, Vector2 position, XFsmPin? originPin = null) 
    {
        var node = Fsm!.RootCluster!.AddNode(name);
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

    private XFsmLink? GetLinkFrom(XFsmOutputPin pin)
    {
        return _links.Find(l => l.Source == pin);
    }

    private XFsmConditionTreeInfo? GetConditionById(int id)
    {
        return _treeInfoMap.GetValueOrDefault(id);
    }

    private int GetNextFreeNodeId()
    {
        return _nodes.Count == 0 ? 0 : _nodes.Max(x => x.Id) + 1;
    }

    private int GetNextFreeTreeInfoId()
    {
        return _treeInfoMap.Count == 0 ? 0 : _treeInfoMap.Keys.Max(x => x) + 1;
    }

    private MtColor GetHeaderColorForNode(XFsmNode node)
    {
        return node switch
        {
            XFsmWeaponNode wpNode => wpNode.NodeType switch
            {
                WeaponFsmNodeType.Action => MtColor.FromVector4(_actioNodeColor),
                WeaponFsmNodeType.Motion => MtColor.FromVector4(_motionNodeColor),
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

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct XFsmGvcNode(XFsmNode node, float divisor) : IDisposable
{
    public readonly int Id = node.Id;
    public readonly byte* Name = Utf8StringMarshaller.ConvertToUnmanaged(node.Name);
    public readonly Vector2 Position = node.Position;
    public readonly Vector2 Size = node.Size / divisor;

    public void Dispose()
    {
        Utf8StringMarshaller.Free(Name);
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct XFsmGvcLink(int source, int target)
{
    public readonly int Source = source;
    public readonly int Target = target;
}

public class XFsmNode
{
    public int Id { get; }
    public int RealId => BackingNode.Id;
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }

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

public class XFsmNodeProcess(AIFSMNodeProcess process, XFsmProcessDescriptor xfsmProcess)
{
    public AIFSMNodeProcess BackingProcess { get; } = process;
    public XFsmProcessDescriptor Descriptor { get; private set; } = xfsmProcess;
    private MtObject? _parameter = process.Parameter;
    public MtObject? Parameter 
    {
        get => _parameter;
        set
        {
            _parameter = value;
            BackingProcess.Parameter = value;
        }
    }

    public void ChangeKind(XFsmProcessDescriptor descriptor)
    {
        if (descriptor.Name == Descriptor.Name)
            return;

        Descriptor = descriptor;
        Parameter?.Destroy(true);
        Parameter = descriptor.ParamDti.CreateInstance<MtObject>();
        BackingProcess.ContainerName = descriptor.Name;
    }
}

public class XFsmQuestNode : XFsmNode
{
    public List<XFsmNodeProcess> Processes { get; }

    public XFsmNodeProcess AddProcess(XFsmProcessDescriptor descriptor)
    {
        var fsmProcess = BackingNode.AddProcess(descriptor.Name);
        var nodeProcess = new XFsmNodeProcess(fsmProcess, descriptor)
        {
            Parameter = descriptor.ParamDti?.CreateInstance<MtObject>()
        };
        Processes.Add(nodeProcess);
        return nodeProcess;
    }

    public XFsmNodeProcess AddProcess(XFsmProcessDescriptor descriptor, int index)
    {
        var fsmProcess = BackingNode.InsertProcess(index, descriptor.Name);
        var nodeProcess = new XFsmNodeProcess(fsmProcess, descriptor)
        {
            Parameter = descriptor.ParamDti.CreateInstance<MtObject>()
        };
        Processes.Insert(index, nodeProcess);
        return nodeProcess;
    }

    public void RemoveProcess(XFsmNodeProcess process)
    {
        Processes.Remove(process);
        BackingNode.RemoveProcess(process.BackingProcess);
    }

    public XFsmQuestNode(AIFSMNode node, IReadOnlyDictionary<string, XFsmProcessDescriptor> allowedProcesses) : base(node)
    {
        Processes = node.Processes.Select(p => new XFsmNodeProcess(p, allowedProcesses[p.ContainerName])).ToList();
    }
}

public class XFsmPin(string name, int id)
{
    public int Id { get; set; } = id;
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

public class XFsmProcessDescriptor
{
    public string Name { get; }
    public MtDti? ParamDti { get; }

    public XFsmProcessDescriptor(string name, MtDti? paramDti)
    {
        Name = name;
        ParamDti = paramDti;
    }
}

public enum WeaponFsmNodeType
{
    Action,
    Motion,
}
