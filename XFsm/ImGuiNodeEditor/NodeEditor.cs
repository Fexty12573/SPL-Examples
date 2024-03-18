using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.InternalCallGenerator;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpPluginLoader.Core.Resources;
using SharpPluginLoader.Core.Entities;

namespace XFsm.ImGuiNodeEditor;


using EditorContextPtr = nint;

[InternalCallManager]
public static unsafe partial class InternalCalls
{
    [InternalCall]
    public static partial void RenderDocTriggerCapture();

    [InternalCall]
    public static partial bool InjectRenderDoc([WideString] string dllPath);

    [InternalCall]
    public static partial void ShowStyleEditor(in bool show = true);

    [InternalCall]
    public static partial void SetCurrentImGuiContext(nint ctx);

    [InternalCall]
    public static partial void SetCurrentEditor(EditorContextPtr ctx);

    [InternalCall]
    public static partial EditorContextPtr GetCurrentEditor();

    
    [InternalCall]
    public static partial EditorContextPtr CreateEditor(Config.NativeConfig* config);

    public static EditorContextPtr CreateEditor(Config? config = null)
    {
        return CreateEditor(config is not null ? config._nativeConfig : null);
    }
    
    [InternalCall]
    public static partial void DestroyEditor(EditorContextPtr ctx);
    
    [InternalCall] // TODO: Expose
    public static partial Config.NativeConfig* GetConfig(EditorContextPtr ctx = 0);
    
    [InternalCall]
    public static partial ref Style GetStyle();
    
    [InternalCall]
    public static partial string GetStyleColorName(StyleColor colorIndex);

    
    [InternalCall]
    public static partial void PushStyleColor(StyleColor colorIndex, ref Vector4 color);
    public static void PushStyleColor(StyleColor colorIndex, Vector4 color) => PushStyleColor(colorIndex, ref color);
    
    [InternalCall]
    public static partial void PopStyleColor(int count = 1);

    
    [InternalCall]
    public static partial void PushStyleVarFloat(StyleVar varIndex, float value);
    
    [InternalCall]
    public static partial void PushStyleVarVec2(StyleVar varIndex, ref Vector2 value);
    public static void PushStyleVarVec2(StyleVar varIndex, Vector2 value) => PushStyleVarVec2(varIndex, ref value);
    
    [InternalCall]
    public static partial void PushStyleVarVec4(StyleVar varIndex, ref Vector4 value);
    public static void PushStyleVarVec4(StyleVar varIndex, Vector4 value) => PushStyleVarVec4(varIndex, ref value);
    
    [InternalCall]
    public static partial void PopStyleVar(int count = 1);

    
    [InternalCall]
    public static partial void Begin(string id, in Vector2 size);
    public static void Begin(string id) => Begin(id, new Vector2(0, 0));
    
    [InternalCall]
    public static partial void End();

    
    [InternalCall]
    public static partial void BeginNode(nint id);
    
    [InternalCall]
    public static partial void BeginPin(nint id, PinKind kind);
    
    [InternalCall]
    public static partial void PinRect(in Vector2 a, in Vector2 b);
    
    [InternalCall]
    public static partial void PinPivotRect(in Vector2 a, in Vector2 b);
    
    [InternalCall]
    public static partial void PinPivotSize(in Vector2 size);
    
    [InternalCall]
    public static partial void PinPivotScale(in Vector2 scale);
    
    [InternalCall]
    public static partial void PinPivotAlignment(in Vector2 alignment);
    
    [InternalCall]
    public static partial void EndPin();
    
    [InternalCall]
    public static partial void Group(in Vector2 size);
    
    [InternalCall]
    public static partial void EndNode();

    
    [InternalCall]
    public static partial bool BeginGroupHint(nint nodeId);
    
    [InternalCall]
    public static partial Vector2 GetGroupMin();
    
    [InternalCall]
    public static partial Vector2 GetGroupMax();
    
    [InternalCall]
    public static partial ImDrawList* GetHintForegroundDrawList();
    
    [InternalCall]
    public static partial ImDrawList* GetHintBackgroundDrawList();
    
    [InternalCall]
    public static partial void EndGroupHint();

    [InternalCall]
    public static partial ImDrawList* GetNodeBackgroundDrawList(nint nodeId);
    
    [InternalCall]
    public static partial bool Link(nint id, nint startPinId, nint endPinId, in Vector4 color, float thickness = 1.0f);
    public static bool Link(nint id, nint startPinId, nint endPinId) => Link(id, startPinId, endPinId, new Vector4(1, 1, 1, 1));

    
    [InternalCall]
    public static partial void Flow(nint linkId, FlowDirection direction = FlowDirection.Forward);

    
    [InternalCall]
    public static partial bool BeginCreate(in Vector4 color, float thickness = 1.0f);
    public static bool BeginCreate() => BeginCreate(new Vector4(1, 1, 1, 1));
    
    [InternalCall]
    public static partial bool QueryNewLink(out nint startId, out nint endId);
    
    [InternalCall]
    public static partial bool QueryNewLinkEx(out nint startId, out nint endId, in Vector4 color, float thickness = 1.0f);
    public static bool QueryNewLinkEx(out nint startId, out nint endId) => QueryNewLinkEx(out startId, out endId, new Vector4(1, 1, 1, 1));
    
    [InternalCall]
    public static partial bool QueryNewNode(out nint pinId);
    
    [InternalCall]
    public static partial bool QueryNewNodeEx(out nint pinId, in Vector4 color, float thickness = 1.0f);
    public static bool QueryNewNodeEx(out nint pinId) => QueryNewNodeEx(out pinId, new Vector4(1, 1, 1, 1));
    
    [InternalCall]
    public static partial bool AcceptNewItem();
    
    [InternalCall]
    public static partial bool AcceptNewItemEx(in Vector4 color, float thickness = 1.0f);
    
    [InternalCall]
    public static partial void RejectNewItem();
    
    [InternalCall]
    public static partial void RejectNewItemEx(in Vector4 color, float thickness = 1.0f);
    
    [InternalCall]
    public static partial void EndCreate();

    
    [InternalCall]
    public static partial bool BeginDelete();
    
    [InternalCall]
    public static partial bool QueryDeletedLink(out nint linkId, out nint startId, out nint endId);
    public static bool QueryDeletedLink(out nint linkId) => QueryDeletedLink(out linkId, out _, out _);
    
    [InternalCall]
    public static partial bool QueryDeletedNode(out nint nodeId);
    
    [InternalCall]
    public static partial bool AcceptDeletedItem(bool deleteDependencies = true);
    
    [InternalCall]
    public static partial void RejectDeletedItem();
    
    [InternalCall]
    public static partial void EndDelete();

    
    [InternalCall]
    public static partial void SetNodePosition(nint nodeId, in Vector2 editorPosition);
    
    [InternalCall]
    public static partial void SetGroupSize(nint nodeId, in Vector2 size);
    
    [InternalCall]
    public static partial void GetNodePosition(nint nodeId, out Vector2 outPos);
    public static Vector2 GetNodePosition(nint nodeId)
    {
        GetNodePosition(nodeId, out var pos);
        return pos;
    }
    
    [InternalCall]
    public static partial void GetNodeSize(nint nodeId, out Vector2 outSize);
    public static Vector2 GetNodeSize(nint nodeId)
    {
        GetNodeSize(nodeId, out var size);
        return size;
    }
    
    [InternalCall]
    public static partial void CenterNodeOnScreen(nint nodeId);
    
    [InternalCall]
    public static partial void SetNodeZPosition(nint nodeId, float z);
    
    [InternalCall]
    public static partial float GetNodeZPosition(nint nodeId);

    
    [InternalCall]
    public static partial void RestoreNodeState(nint nodeId);

    
    [InternalCall]
    public static partial void Suspend();
    
    [InternalCall]
    public static partial void Resume();
    
    [InternalCall]
    public static partial bool IsSuspended();

    
    [InternalCall]
    public static partial bool IsActive();

    
    [InternalCall]
    public static partial bool HasSelectionChanged();
    
    [InternalCall]
    public static partial int GetSelectedObjectCount();
    
    [InternalCall]
    public static partial int GetSelectedNodes(Span<nint> nodes, int size);
    public static int GetSelectedNodeCount() => _GetSelectedNodesPtr(null, 0);
    public static void GetSelectedNodes(List<nint> nodes) // Will resize the list to fit the result
    {
        var count = GetSelectedNodeCount();
        CollectionsMarshal.SetCount(nodes, count);
        GetSelectedNodes(CollectionsMarshal.AsSpan(nodes), count);
    }
    
    [InternalCall]
    public static partial int GetSelectedLinks(Span<nint> links, int size);
    public static int GetSelectedLinkCount() => _GetSelectedLinksPtr(null, 0);
    public static void GetSelectedLinks(List<nint> links) // Will resize the list to fit the result
    {
        var count = GetSelectedLinkCount();
        CollectionsMarshal.SetCount(links, count);
        GetSelectedLinks(CollectionsMarshal.AsSpan(links), count);
    }
    
    [InternalCall]
    public static partial bool IsNodeSelected(nint nodeId);
    
    [InternalCall]
    public static partial bool IsLinkSelected(nint linkId);
    
    [InternalCall]
    public static partial void ClearSelection();
    
    [InternalCall]
    public static partial void SelectNode(nint nodeId, bool append = false);
    
    [InternalCall]
    public static partial void SelectLink(nint linkId, bool append = false);
    
    [InternalCall]
    public static partial void DeselectNode(nint nodeId);
    
    [InternalCall]
    public static partial void DeselectLink(nint linkId);

    
    [InternalCall]
    public static partial bool DeleteNode(nint nodeId);
    
    [InternalCall]
    public static partial bool DeleteLink(nint linkId);

    
    [InternalCall]
    public static partial bool NodeHasAnyLinks(nint nodeId); // Returns true if node has any link connected
    
    [InternalCall]
    public static partial bool PinHasAnyLinks(nint pinId); // Return true if pin has any link connected
    
    [InternalCall]
    public static partial int PinBreakLinks(nint nodeId); // Break all links connected to this node
    
    [InternalCall]
    public static partial int NodeBreakLinks(nint pinId); // Break all links connected to this pin

    
    [InternalCall]
    public static partial void NavigateToContent(float duration = -1);
    
    [InternalCall]
    public static partial void NavigateToSelection(bool zoomIn = false, float duration = -1);

    
    [InternalCall]
    public static partial bool ShowNodeContextMenu(out nint nodeId);
    
    [InternalCall]
    public static partial bool ShowPinContextMenu(out nint pinId);
    
    [InternalCall]
    public static partial bool ShowLinkContextMenu(out nint linkId);
    
    [InternalCall]
    public static partial bool ShowBackgroundContextMenu();

    
    [InternalCall]
    public static partial void EnableShortcuts(bool enable);
    
    [InternalCall]
    public static partial bool AreShortcutsEnabled();

    
    [InternalCall]
    public static partial bool BeginShortcut();
    
    [InternalCall]
    public static partial bool AcceptCut();
    
    [InternalCall]
    public static partial bool AcceptCopy();
    
    [InternalCall]
    public static partial bool AcceptPaste();
    
    [InternalCall]
    public static partial bool AcceptDuplicate();
    
    [InternalCall]
    public static partial bool AcceptCreateNode();
    
    [InternalCall]
    public static partial int GetActionContextSize();
    
    [InternalCall]
    public static partial int GetActionContextNodes(Span<nint> nodes, int size);
    public static int GetActionContextNodeCount() => _GetActionContextNodesPtr(null, 0);
    public static void GetActionContextNodes(List<nint> nodes) // Will resize the list to fit the result
    {
        var count = GetActionContextNodeCount();
        CollectionsMarshal.SetCount(nodes, count);
        GetActionContextNodes(CollectionsMarshal.AsSpan(nodes), count);
    }
    
    [InternalCall]
    public static partial int GetActionContextLinks(Span<nint> links, int size);
    public static int GetActionContextLinkCount() => _GetActionContextLinksPtr(null, 0);
    public static void GetActionContextLinks(List<nint> links) // Will resize the list to fit the result
    {
        var count = GetActionContextLinkCount();
        CollectionsMarshal.SetCount(links, count);
        GetActionContextLinks(CollectionsMarshal.AsSpan(links), count);
    }
    
    [InternalCall]
    public static partial void EndShortcut();

    
    [InternalCall]
    public static partial float GetCurrentZoom();

    
    [InternalCall]
    public static partial nint GetHoveredNode();
    
    [InternalCall]
    public static partial nint GetHoveredPin();
    
    [InternalCall]
    public static partial nint GetHoveredLink();
    
    [InternalCall]
    public static partial nint GetDoubleClickedNode();
    
    [InternalCall]
    public static partial nint GetDoubleClickedPin();
    
    [InternalCall]
    public static partial nint GetDoubleClickedLink();
    
    [InternalCall]
    public static partial bool IsBackgroundClicked();
    
    [InternalCall]
    public static partial bool IsBackgroundDoubleClicked();
    
    [InternalCall]
    public static partial ImGuiMouseButton GetBackgroundClickButtonIndex(); // -1 if none
    
    [InternalCall]
    public static partial ImGuiMouseButton GetBackgroundDoubleClickButtonIndex(); // -1 if none

    
    [InternalCall]
    public static partial bool GetLinkPins(nint linkId, out nint startPinId, out nint endPinId); // pass nullptr if particular pin do not interest you
    
    [InternalCall]
    public static partial bool PinHadAnyLinks(nint pinId);

    
    [InternalCall]
    public static partial Vector2 GetScreenSize();
    
    [InternalCall]
    public static partial Vector2 ScreenToCanvas(in Vector2 pos);
    
    [InternalCall]
    public static partial Vector2 CanvasToScreen(in Vector2 pos);

    
    [InternalCall]
    public static partial int GetNodeCount(); // Returns number of submitted nodes since Begin() call
    
    [InternalCall]
    public static partial int GetOrderedNodeIds(Span<nint> nodes, int size);
    public static int GetOrderedNodeCount() => _GetOrderedNodeIdsPtr(null, 0);
    public static void GetOrderedNodeIds(List<nint> nodes) // Will resize the list to fit the result
    {
        var count = GetOrderedNodeCount();
        CollectionsMarshal.SetCount(nodes, count);
        GetOrderedNodeIds(CollectionsMarshal.AsSpan(nodes), count);
    }

    [InternalCall]
    public static partial void Icon(in Vector2 size, IconType type, bool filled, in Vector4 color, in Vector4 innerColor);
    public static void Icon(in Vector2 size, IconType type, bool filled, in Vector4 color)
    {
        Icon(size, type, filled, color, new Vector4(0, 0, 0, 0));
    }
    public static void Icon(in Vector2 size, IconType type, bool filled)
    {
        Icon(size, type, filled, new Vector4(1, 1, 1, 1));
    }

    [InternalCall]
    public static partial void DockBuilderDockWindow(string name, uint id);

    [InternalCall]
    public static partial nint DockBuilderGetNode(uint id);

    [InternalCall]
    public static partial nint DockBuilderGetCentralNode(uint id);

    [InternalCall]
    public static partial uint DockBuilderAddNode(uint id = 0, ImGuiDockNodeFlags flags = ImGuiDockNodeFlags.None);

    [InternalCall]
    public static partial void DockBuilderRemoveNode(uint id);

    [InternalCall]
    public static partial void DockBuilderRemoveNodeDockedWindows(uint id, bool clearSettingsRefs = true);

    [InternalCall]
    public static partial void DockBuilderRemoveNodeChildNodes(uint id);

    [InternalCall]
    public static partial void DockBuilderSetNodePos(uint id, Vector2 pos);

    [InternalCall]
    public static partial void DockBuilderSetNodeSize(uint id, Vector2 size);

    [InternalCall]
    public static partial uint DockBuilderSplitNode(uint id, ImGuiDir splitDir, float sizeRatioForNodeAtDir, out uint outIdAtDir, out uint outIdAtOppositeDir);

    [InternalCall]
    public static partial void DockBuilderCopyDockSpace(uint srcNodeId, uint dstNodeId, in ImVector inWindowRemapPairs);

    [InternalCall]
    public static partial void DockBuilderCopyNode(uint srcNodeId, uint dstNodeId, in ImVector inWindowRemapPairs);

    [InternalCall]
    public static partial void DockBuilderCopyWindowSettings(string srcName, string dstName);

    [InternalCall]
    public static partial void DockBuilderFinish(uint id);

    [InternalCall]
    public static partial nint GvContext();

    [InternalCall]
    public static partial void GvFreeContext(nint ctx);

    [InternalCall]
    public static partial void GvLayout(nint ctx, Span<XFsmGvcNode> nodes, int nodeCount, Span<XFsmGvcLink> links, int linkCount, string engine);

    [InternalCall(Pattern = "48 c1 e8 17 83 e0 3f 48 8b 04 c1 c3", Offset = -10, Options = InternalCallOptions.Unsafe)]
    public static partial nint GetAllocator(nint dti);

    internal static MtAllocator GetAllocator(MtDti? dti)
    {
        if (dti is null) return null!;
        var allocator = GetAllocator(dti.Instance);
        return allocator == 0 ? null! : new MtAllocator(allocator);
    }

    [InternalCall(Pattern = "48 83 ec 20 48 8b d9 48 8b fa 48 8b 09 48 8d 41 08", Offset = -6)]
    public static partial void MtStringAssign(nint str, string value);

    [InternalCall(Pattern = "49 03 D1 33 C0 66 0F 1F 44 00 00 48 83 BC C2 70 80 00 00 00", Offset = -37)]
    private static partial void RegisterResource(nint sMhResource, nint resource, uint x);

    [InternalCall(Pattern = "48 8b 81 90 0d 00 00 4c 8b e1 80 b8 34 89 00 00 00", Offset = -24)]
    public static partial void ReloadPlayerFsm(nint humanController);
    public static void ReloadPlayerFsm(Player player) => ReloadPlayerFsm(player.Get<nint>(0x12608));

    [InternalCall(Pattern = "48 89 78 E0 48 8D B1 B0 76 00 00 4C 89 68 D8 45 33 C0", Offset = -20)]
    public static partial void ChangeWeapon(nint player, WeaponType weaponType, int weaponId);
    public static void ChangeWeapon(WeaponType weaponType, int weaponId) => ChangeWeapon(Player.MainPlayer!.Instance, weaponType, weaponId);

    public static void RegisterResource(Resource resource)
    {
        var sMhResource = ResourceManager.SingletonInstance;
        var resourceTable = new NativeArray<nint>(sMhResource.Get<nint>(0x8070), 0x40000);
        var n = (int)(resource.Get<uint>(0x78) & 0x1FFF) << 5;

        for (var i = 0; i < 32; ++i)
        {
            if (resourceTable[n + i] == 0)
            {
                resourceTable[n + i] = resource.Instance;
                break;
            }
        }

        RegisterResource(sMhResource.Instance, resource.Instance, 1);
    }
}


public enum PinKind
{
    Input,
    Output,
}

public enum FlowDirection
{
    Forward,
    Backward,
}

public enum CanvasSizeMode
{
    FitVerticalView,
    FitHorizontalView,
    CenterOnly,
}

[Flags]
public enum SaveReason : uint
{
    None = 0x00,
    Navigation = 0x01,
    Position = 0x02,
    Size = 0x04,
    Selection = 0x08,
    AddNode = 0x10,
    RemoveNode = 0x20,
    User = 0x40,
}

public enum StyleColor
{
    Bg,
    Grid,
    NodeBg,
    NodeBorder,
    HovNodeBorder,
    SelNodeBorder,
    NodeSelRect,
    NodeSelRectBorder,
    HovLinkBorder,
    SelLinkBorder,
    HighlightLinkBorder,
    LinkSelRect,
    LinkSelRectBorder,
    PinRect,
    PinRectBorder,
    Flow,
    FlowMarker,
    GroupBg,
    GroupBorder,

    Count
}

public enum StyleVar
{
    NodePadding,
    NodeRounding,
    NodeBorderWidth,
    HoveredNodeBorderWidth,
    SelectedNodeBorderWidth,
    PinRounding,
    PinBorderWidth,
    LinkStrength,
    SourceDirection,
    TargetDirection,
    ScrollDuration,
    FlowMarkerDistance,
    FlowSpeed,
    FlowDuration,
    PivotAlignment,
    PivotSize,
    PivotScale,
    PinCorners,
    PinRadius,
    PinArrowSize,
    PinArrowWidth,
    GroupRounding,
    GroupBorderWidth,
    HighlightConnectedLinks,
    SnapLinkToPinDir,
    HoveredNodeBorderOffset,
    SelectedNodeBorderOffset,

    Count
}

public enum IconType
{
    Flow,
    Circle,
    Square,
    Grid,
    RoundSquare,
    Diamond,
}

public unsafe class Config
{
    public ref ImVector<float> CustomZoomLevels => ref _nativeConfig->CustomZoomLevels;
    public ref CanvasSizeMode CanvasSizeMode => ref _nativeConfig->CanvasSizeMode;
    public ImGuiMouseButton DragButton
    {
        get => (ImGuiMouseButton)_nativeConfig->DragButtonIndex;
        set => _nativeConfig->DragButtonIndex = (int)value;
    }
    public ImGuiMouseButton SelectButton
    {
        get => (ImGuiMouseButton)_nativeConfig->SelectButtonIndex;
        set => _nativeConfig->SelectButtonIndex = (int)value;
    }
    public ImGuiMouseButton NavigateButton
    {
        get => (ImGuiMouseButton)_nativeConfig->NavigateButtonIndex;
        set => _nativeConfig->NavigateButtonIndex = (int)value;
    }
    public ImGuiMouseButton ContextMenuButton
    {
        get => (ImGuiMouseButton)_nativeConfig->ContextMenuButtonIndex;
        set => _nativeConfig->ContextMenuButtonIndex = (int)value;
    }

    internal NativeConfig* _nativeConfig;

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeConfig
    {
        public sbyte* SettingsFile;
        public nint BeginSaveSession;
        public nint EndSaveSession;
        public nint SaveSettings;
        public nint LoadSettings;
        public nint LoadNodeSettings;
        public nint SaveNodeSettings;
        public void* UserPtr;
        public ImVector<float> CustomZoomLevels;
        public CanvasSizeMode CanvasSizeMode;
        public int DragButtonIndex;
        public int SelectButtonIndex;
        public int NavigateButtonIndex;
        public int ContextMenuButtonIndex;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Style
{
    public Vector4 NodePadding;
    public float NodeRounding;
    public float NodeBorderWidth;
    public float HoveredNodeBorderWidth;
    public float HoveredNodeBorderOffset;
    public float SelectedNodeBorderWidth;
    public float SelectedNodeBorderOffset;
    public float PinRounding;
    public float PinBorderWidth;
    public float LinkStrength;
    public Vector2 SourceDirection;
    public Vector2 TargetDirection;
    public float ScrollDuration;
    public float FlowMarkerDistance;
    public float FlowSpeed;
    public float FlowDuration;
    public Vector2 PivotAlignment;
    public Vector2 PivotSize;
    public Vector2 PivotScale;
    public float PinCorners;
    public float PinRadius;
    public float PinArrowSize;
    public float PinArrowWidth;
    public float GroupRounding;
    public float GroupBorderWidth;
    public float HighlightConnectedLinks;
    public float SnapLinkToPinDir; // when true link will start on the line defined by pin direction
    public StyleColors Colors;
}

[InlineArray((int)StyleColor.Count)]
public struct StyleColors
{
    private Vector4 _element0;
}
